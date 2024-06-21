using ILGPU;
using ILGPU.Runtime;
using Maths;
using PA.Graphics;

namespace PA5;

internal unsafe class Renderer : IDisposable
{
    public const int MaxRays = 2;
    public const int MaxDepth = 5;

    private readonly Scene _scene;
    private readonly bool _useCpu;

    private Context? context;
    private Accelerator? accelerator;
    private Action<Index2D, SceneProperties, ArrayView1D<Material, Stride1D.Dense>, ArrayView1D<Geometry, Stride1D.Dense>, ArrayView1D<Light, Stride1D.Dense>, ArrayView1D<Vector4d, Stride1D.Dense>>? renderKernel;

    private int currentWidth;
    private int currentHeight;
    private MemoryBuffer1D<Material, Stride1D.Dense>? materialsBuffer;
    private MemoryBuffer1D<Geometry, Stride1D.Dense>? objectsBuffer;
    private MemoryBuffer1D<Light, Stride1D.Dense>? lightsBuffer;
    private MemoryBuffer1D<Vector4d, Stride1D.Dense>? frameBuffer;

    public Renderer(Scene scene, bool useCpu = false)
    {
        _scene = scene;
        _useCpu = useCpu;

        InitializeILGPU();
    }

    #region Scene Properties
    public int Width => _scene.Width;

    public int Height => _scene.Height;

    public FrameBuffer FrameBuffer => _scene.FrameBuffer;
    #endregion

    #region Renderer Properties
    public Device Device => accelerator!.Device;
    #endregion

    public void Render()
    {
        if (!UpdateBuffers())
        {
            return;
        }

        renderKernel!(new Index2D(currentWidth, currentHeight),
                      _scene.SceneProperties,
                      materialsBuffer!,
                      objectsBuffer!,
                      lightsBuffer!,
                      frameBuffer!);

        accelerator!.Synchronize();

        Span<Vector4d> colorBuffer = new(FrameBuffer.FinalColorBuffer, FrameBuffer.BufferSize);

        frameBuffer!.View.CopyToCPU(ref colorBuffer[0], colorBuffer.Length);

        FrameBuffer.UpdateTexture();
    }

    public void Dispose()
    {
        frameBuffer?.Dispose();
        lightsBuffer?.Dispose();
        objectsBuffer?.Dispose();
        accelerator?.Dispose();
        context?.Dispose();

        GC.SuppressFinalize(this);
    }

    private void InitializeILGPU()
    {
        context = Context.Create(builder => builder.Default().EnableAlgorithms());

        List<Device> devices = [.. context];

        Device? useDevice = null;
        if (!_useCpu)
        {
            if (devices.FirstOrDefault(item => item.AcceleratorType == AcceleratorType.Cuda) is Device device1)
            {
                useDevice = device1;
            }
            else if (devices.FirstOrDefault(item => item.AcceleratorType == AcceleratorType.OpenCL) is Device device2)
            {
                useDevice = device2;
            }
        }

        useDevice ??= devices.First(item => item.AcceleratorType == AcceleratorType.CPU);

        accelerator = useDevice.CreateAccelerator(context);

        renderKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, SceneProperties, ArrayView1D<Material, Stride1D.Dense>, ArrayView1D<Geometry, Stride1D.Dense>, ArrayView1D<Light, Stride1D.Dense>, ArrayView1D<Vector4d, Stride1D.Dense>>(RenderKernel);
    }

    private bool UpdateBuffers()
    {
        if (accelerator == null)
        {
            return false;
        }

        if (materialsBuffer == null || materialsBuffer.Length != _scene.Materials.Count)
        {
            materialsBuffer?.Dispose();
            materialsBuffer = accelerator.Allocate1D<Material>(_scene.Materials.Count);
        }

        if (objectsBuffer == null || objectsBuffer.Length != _scene.Objects.Count)
        {
            objectsBuffer?.Dispose();
            objectsBuffer = accelerator.Allocate1D<Geometry>(_scene.Objects.Count);
        }

        if (lightsBuffer == null || lightsBuffer.Length != _scene.Lights.Count)
        {
            lightsBuffer?.Dispose();
            lightsBuffer = accelerator.Allocate1D<Light>(_scene.Lights.Count);
        }

        if (currentWidth != Width || currentHeight != Height)
        {
            frameBuffer?.Dispose();

            currentWidth = Width;
            currentHeight = Height;

            frameBuffer = accelerator.Allocate1D<Vector4d>(currentWidth * currentHeight);
        }

        materialsBuffer.CopyFromCPU([.. _scene.Materials]);
        objectsBuffer.CopyFromCPU([.. _scene.Objects]);
        lightsBuffer.CopyFromCPU([.. _scene.Lights]);
        frameBuffer!.MemSetToZero();

        return true;
    }

    private static void RenderKernel(Index2D index,
                                     SceneProperties scene,
                                     ArrayView1D<Material, Stride1D.Dense> materials,
                                     ArrayView1D<Geometry, Stride1D.Dense> objects,
                                     ArrayView1D<Light, Stride1D.Dense> lights,
                                     ArrayView1D<Vector4d, Stride1D.Dense> frame)
    {
        float scale = MathF.Tan(scene.Camera.Fov.Radians);
        float imageAspectRatio = scene.Width / (float)scene.Height;

        float x = MathsHelper.RangeMap(index.X + 0.5f, 0.0f, scene.Width - 1.0f, -1.0f, 1.0f);
        float y = MathsHelper.RangeMap(index.Y + 0.5f, 0.0f, scene.Height - 1.0f, 1.0f, -1.0f);

        x *= imageAspectRatio * scale;
        y *= scale;

        Vector3d dir = new(x, y, -1);
        dir = Vector3d.Normalize(dir);

        frame[index.Y * scene.Width + index.X] = new Vector4d(LoopCastRay(scene, new Ray(scene.Camera.Position, dir), materials, objects, lights), 1.0f);
    }

    private static Vector3d LoopCastRay(SceneProperties scene,
                                        Ray ray,
                                        ArrayView1D<Material, Stride1D.Dense> materials,
                                        ArrayView1D<Geometry, Stride1D.Dense> objects,
                                        ArrayView1D<Light, Stride1D.Dense> lights)
    {
        ArrayView<CastRayStack> leaf1Stacks = LocalMemory.Allocate1D<CastRayStack>(MaxDepth);
        ArrayView<CastRayStack> leaf2Stacks = LocalMemory.Allocate1D<CastRayStack>(MaxDepth);

        CastRayStack rootStack = CastRayStack.Create(scene, ray, objects, materials);

        leaf1Stacks[0] = rootStack;
        leaf2Stacks[0] = rootStack;

        if (rootStack.isHit)
        {
            CastRayStack currentStack = rootStack;
            int currentLeafStack = 0;

            for (int i = 0; i < MaxDepth; i++)
            {
                MaterialType materialType = currentStack.Intersection.Material.MaterialType;

                if (materialType == MaterialType.ReflectAndRefract || materialType == MaterialType.Reflect)
                {
                    CastRayStack leaf = CastRayStack.Create(scene, currentStack.ReflectRay(), objects, materials);

                    if (leaf.isHit)
                    {
                        leaf1Stacks[++currentLeafStack] = leaf;

                        leaf1Stacks[currentLeafStack - 1].Leaf1 = currentLeafStack;

                        currentStack = leaf;

                        continue;
                    }
                }

                break;
            }

            currentStack = rootStack;
            currentLeafStack = 0;

            for (int i = 0; i < MaxDepth; i++)
            {
                MaterialType materialType = currentStack.Intersection.Material.MaterialType;

                if (materialType == MaterialType.ReflectAndRefract)
                {
                    CastRayStack leaf = CastRayStack.Create(scene, currentStack.RefractRay(), objects, materials);

                    if (leaf.isHit)
                    {
                        leaf2Stacks[++currentLeafStack] = leaf;

                        leaf2Stacks[currentLeafStack - 1].Leaf2 = currentLeafStack;

                        currentStack = leaf;

                        continue;
                    }
                }

                break;
            }
        }

        rootStack.Leaf1 = leaf1Stacks[0].Leaf1;
        rootStack.Leaf2 = leaf2Stacks[0].Leaf2;

        return rootStack.Color(leaf1Stacks, leaf2Stacks, objects, materials, lights);
    }
}
