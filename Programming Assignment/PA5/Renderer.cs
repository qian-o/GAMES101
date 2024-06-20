using ILGPU;
using ILGPU.Runtime;
using Maths;
using PA.Graphics;

namespace PA5;

internal unsafe class Renderer : IDisposable
{
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

        frame[index.Y * scene.Width + index.X] = new Vector4d(CastRay(scene, new Ray(scene.Camera.Position, dir), materials, objects, lights), 1.0f);
    }

    private static Vector3d CastRay(SceneProperties scene,
                                    Ray ray,
                                    ArrayView1D<Material, Stride1D.Dense> materials,
                                    ArrayView1D<Geometry, Stride1D.Dense> objects,
                                    ArrayView1D<Light, Stride1D.Dense> lights)
    {
        HitPayload[] reflectPayloads = new HitPayload[MaxDepth];
        int reflectDepth = 0;

        for (int i = 0; i < MaxDepth; i++)
        {
            HitPayload hitPayload = Trace(ray, objects);

            if (hitPayload.ObjectIndex == -1)
            {
                break;
            }

            ray = GetReflectRay(scene, ray, hitPayload, objects);

            reflectDepth++;
        }

        if (reflectDepth == 0)
        {
            return scene.BackgroundColor;
        }

        for (int i = reflectDepth; i <= 0; i--)
        {

        }

        return Vector3d.One;
    }

    private static HitPayload Trace(Ray ray, ArrayView1D<Geometry, Stride1D.Dense> objects)
    {
        float tNear = float.MaxValue;

        HitPayload payload = HitPayload.False;

        for (int i = 0; i < objects.Length; i++)
        {
            if (Geometry.Intersect(objects[i], ray) is Intersection intersection && intersection.TNear < tNear)
            {
                payload = new HitPayload(i, intersection);

                tNear = intersection.TNear;
            }
        }

        return payload;
    }

    private static Ray GetReflectRay(SceneProperties scene,
                                     Ray ray,
                                     HitPayload hitPayload,
                                     ArrayView1D<Geometry, Stride1D.Dense> objects)
    {
        Geometry geometry = objects[hitPayload.ObjectIndex];

        Vector3d position = ray.At(hitPayload.Intersection.TNear);

        SurfaceProperties surface = Geometry.GetSurfaceProperties(geometry, position, ray.Direction, hitPayload.Intersection.UV);

        Vector3d reflectDir = Vector3d.Reflect(ray.Direction, surface.Normal);
        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, surface.Normal) < 0 ? position - surface.Normal * scene.Epsilon : position + surface.Normal * scene.Epsilon;

        return new Ray(reflectOrigin, reflectDir);
    }

    private static Ray GetRefractRay(SceneProperties scene,
                                     Ray ray,
                                     HitPayload hitPayload,
                                     ArrayView1D<Material, Stride1D.Dense> materials,
                                     ArrayView1D<Geometry, Stride1D.Dense> objects)
    {
        Geometry geometry = objects[hitPayload.ObjectIndex];

        Vector3d position = ray.At(hitPayload.Intersection.TNear);

        SurfaceProperties surface = Geometry.GetSurfaceProperties(geometry, position, ray.Direction, hitPayload.Intersection.UV);

        Material material = materials[geometry.MaterialIndex];

        Vector3d refractDir = Vector3d.Refract(ray.Direction, surface.Normal, material.Ior);
        Vector3d refractOrigin = Vector3d.Dot(refractDir, surface.Normal) < 0 ? position - surface.Normal * scene.Epsilon : position + surface.Normal * scene.Epsilon;

        return new Ray(refractOrigin, refractDir);
    }
}
