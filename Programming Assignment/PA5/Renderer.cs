using ILGPU;
using ILGPU.Runtime;
using Maths;
using PA.Graphics;

namespace PA5;

internal unsafe class Renderer : IDisposable
{
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

    public void Render()
    {
        if (!UpdateBuffers())
        {
            return;
        }

        renderKernel!(new Index2D(currentWidth, currentHeight),
                      _scene.GetProperties(),
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

        frame[index.Y * scene.Width + index.X] = new Vector4d(CastRay(scene, scene.Camera.Position, dir, materials, objects, lights, 0), 1.0f);
    }

    private static Vector3d CastRay(SceneProperties scene,
                                    Vector3d orig,
                                    Vector3d dir,
                                    ArrayView1D<Material, Stride1D.Dense> materials,
                                    ArrayView1D<Geometry, Stride1D.Dense> objects,
                                    ArrayView1D<Light, Stride1D.Dense> lights,
                                    int depth)
    {
        if (depth > scene.MaxDepth)
        {
            return Vector3d.Zero;
        }

        Vector3d hitColor = scene.BackgroundColor;
        if (Trace(scene, orig, dir, objects) is HitPayload payload)
        {
            Geometry obj = objects[payload.ObjectIndex];
            Material mat = materials[obj.MaterialIndex];

            Vector3d hitPoint = orig + dir * payload.TNear;
            Vector3d normal = default;
            Vector2d st = default;

            Geometry.GetSurfaceProperties(obj, hitPoint, dir, payload.UV, ref normal, ref st);

            return new Vector3d(payload.TNear);
        }

        return hitColor;
    }

    private static HitPayload? Trace(SceneProperties scene,
                                     Vector3d orig,
                                     Vector3d dir,
                                     ArrayView1D<Geometry, Stride1D.Dense> objects)
    {
        float tNear = float.MaxValue;

        HitPayload? payload = null;

        for (int i = 0; i < scene.ObjectsLength; i++)
        {
            float tNearK = float.MaxValue;
            Vector2d uvK = Vector2d.Zero;

            if (Geometry.Intersect(objects[i], orig, dir, ref tNearK, ref uvK) && tNearK < tNear)
            {
                payload = new HitPayload
                {
                    TNear = tNearK,
                    UV = uvK,
                    ObjectIndex = i
                };

                tNear = tNearK;
            }
        }

        return payload;
    }
}
