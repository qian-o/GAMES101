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
        if (Trace(ray, objects) is HitPayload payload && payload.ObjectIndex >= 0)
        {
            Geometry obj = objects[payload.ObjectIndex];
            Material material = materials[obj.MaterialIndex];

            Vector3d position = ray.At(payload.Intersection.TNear);

            SurfaceProperties surface = Geometry.GetSurfaceProperties(obj, position, payload.Intersection.UV);

            switch (material.MaterialType)
            {
                case MaterialType.ReflectAndRefract:
                    {
                        Vector3d reflectDir = Vector3d.Normalize(Vector3d.Reflect(ray.Direction, surface.Normal));
                        Vector3d refractDir = Vector3d.Normalize(Vector3d.Refract(ray.Direction, surface.Normal, material.Ior));

                        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, surface.Normal) < 0.0f
                            ? position - (surface.Normal * scene.Epsilon)
                            : position + (surface.Normal * scene.Epsilon);

                        Vector3d refractOrigin = Vector3d.Dot(refractDir, surface.Normal) < 0.0f
                            ? position - (surface.Normal * scene.Epsilon)
                            : position + (surface.Normal * scene.Epsilon);

                        Ray reflectRay = new(reflectOrigin, reflectDir);
                        Ray refractRay = new(refractOrigin, refractDir);

                        Vector3d reflectColor = CastRay1(scene, reflectRay, materials, objects, lights);
                        Vector3d refractColor = CastRay1(scene, refractRay, materials, objects, lights);

                        float kr = MathsHelper.Fresnel(ray.Direction, surface.Normal, material.Ior);

                        return (reflectColor * kr) + (refractColor * (1 - kr));
                    }
                case MaterialType.Reflect:
                    {
                        Vector3d reflectDir = Vector3d.Normalize(Vector3d.Reflect(ray.Direction, surface.Normal));

                        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, surface.Normal) < 0.0f
                            ? position - (surface.Normal * scene.Epsilon)
                            : position + (surface.Normal * scene.Epsilon);

                        Ray reflectRay = new(reflectOrigin, reflectDir);

                        float kr = MathsHelper.Fresnel(ray.Direction, surface.Normal, material.Ior);

                        return CastRay1(scene, reflectRay, materials, objects, lights) * kr;
                    }
                default:
                    {
                        Vector3d lightAmt = Vector3d.Zero;
                        Vector3d specularColor = Vector3d.Zero;

                        Vector3d shadowOrigin = Vector3d.Dot(ray.Direction, surface.Normal) < 0.0f
                            ? position + (surface.Normal * scene.Epsilon)
                            : position - (surface.Normal * scene.Epsilon);

                        for (int i = 0; i < lights.Length; i++)
                        {
                            Light light = lights[i];

                            Vector3d lightDir = light.Position - position;
                            float lightDistance2 = lightDir.LengthSquared;
                            lightDir = Vector3d.Normalize(lightDir);
                            float LdotN = Math.Max(Vector3d.Dot(lightDir, surface.Normal), 0.0f);

                            HitPayload shadowHit = Trace(new Ray(shadowOrigin, lightDir), objects);
                            bool inShadow = shadowHit.ObjectIndex >= 0 && shadowHit.Intersection.TNear * shadowHit.Intersection.TNear < lightDistance2;

                            lightAmt += inShadow ? Vector3d.Zero : light.Intensity * LdotN;
                            Vector3d reflectionDirection = Vector3d.Reflect(-lightDir, surface.Normal);

                            specularColor += MathF.Pow(Math.Max(-Vector3d.Dot(reflectionDirection, ray.Direction), 0.0f), material.SpecularExponent) * light.Intensity;
                        }

                        return lightAmt * Geometry.EvalDiffuseColor(obj, material, surface.ST) * material.Kd + specularColor * material.Ks;
                    }
            }
        }

        return scene.BackgroundColor;
    }

    private static Vector3d CastRay1(SceneProperties scene,
                                     Ray ray,
                                     ArrayView1D<Material, Stride1D.Dense> materials,
                                     ArrayView1D<Geometry, Stride1D.Dense> objects,
                                     ArrayView1D<Light, Stride1D.Dense> lights)
    {
        if (Trace(ray, objects) is HitPayload payload && payload.ObjectIndex >= 0)
        {
            Geometry obj = objects[payload.ObjectIndex];
            Material material = materials[obj.MaterialIndex];

            Vector3d position = ray.At(payload.Intersection.TNear);

            SurfaceProperties surface = Geometry.GetSurfaceProperties(obj, position, payload.Intersection.UV);

            switch (material.MaterialType)
            {
                case MaterialType.ReflectAndRefract:
                    {
                        Vector3d reflectDir = Vector3d.Normalize(Vector3d.Reflect(ray.Direction, surface.Normal));
                        Vector3d refractDir = Vector3d.Normalize(Vector3d.Refract(ray.Direction, surface.Normal, material.Ior));

                        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, surface.Normal) < 0.0f
                            ? position - (surface.Normal * scene.Epsilon)
                            : position + (surface.Normal * scene.Epsilon);

                        Vector3d refractOrigin = Vector3d.Dot(refractDir, surface.Normal) < 0.0f
                            ? position - (surface.Normal * scene.Epsilon)
                            : position + (surface.Normal * scene.Epsilon);

                        Ray reflectRay = new(reflectOrigin, reflectDir);
                        Ray refractRay = new(refractOrigin, refractDir);

                        Vector3d reflectColor = CastRay2(scene, reflectRay, materials, objects, lights);
                        Vector3d refractColor = CastRay2(scene, refractRay, materials, objects, lights);

                        float kr = MathsHelper.Fresnel(ray.Direction, surface.Normal, material.Ior);

                        return (reflectColor * kr) + (refractColor * (1 - kr));
                    }
                case MaterialType.Reflect:
                    {
                        Vector3d reflectDir = Vector3d.Normalize(Vector3d.Reflect(ray.Direction, surface.Normal));

                        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, surface.Normal) < 0.0f
                            ? position - (surface.Normal * scene.Epsilon)
                            : position + (surface.Normal * scene.Epsilon);

                        Ray reflectRay = new(reflectOrigin, reflectDir);

                        float kr = MathsHelper.Fresnel(ray.Direction, surface.Normal, material.Ior);

                        return CastRay2(scene, reflectRay, materials, objects, lights) * kr;
                    }
                default:
                    {
                        Vector3d lightAmt = Vector3d.Zero;
                        Vector3d specularColor = Vector3d.Zero;

                        Vector3d shadowOrigin = Vector3d.Dot(ray.Direction, surface.Normal) < 0.0f
                            ? position + (surface.Normal * scene.Epsilon)
                            : position - (surface.Normal * scene.Epsilon);

                        for (int i = 0; i < lights.Length; i++)
                        {
                            Light light = lights[i];

                            Vector3d lightDir = light.Position - position;
                            float lightDistance2 = lightDir.LengthSquared;
                            lightDir = Vector3d.Normalize(lightDir);
                            float LdotN = Math.Max(Vector3d.Dot(lightDir, surface.Normal), 0.0f);

                            HitPayload shadowHit = Trace(new Ray(shadowOrigin, lightDir), objects);
                            bool inShadow = shadowHit.ObjectIndex >= 0 && shadowHit.Intersection.TNear * shadowHit.Intersection.TNear < lightDistance2;

                            lightAmt += inShadow ? Vector3d.Zero : light.Intensity * LdotN;
                            Vector3d reflectionDirection = Vector3d.Reflect(-lightDir, surface.Normal);

                            specularColor += MathF.Pow(Math.Max(-Vector3d.Dot(reflectionDirection, ray.Direction), 0.0f), material.SpecularExponent) * light.Intensity;
                        }

                        return lightAmt * Geometry.EvalDiffuseColor(obj, material, surface.ST) * material.Kd + specularColor * material.Ks;
                    }
            }
        }

        return scene.BackgroundColor;
    }

    private static Vector3d CastRay2(SceneProperties scene,
                                     Ray ray,
                                     ArrayView1D<Material, Stride1D.Dense> materials,
                                     ArrayView1D<Geometry, Stride1D.Dense> objects,
                                     ArrayView1D<Light, Stride1D.Dense> lights)
    {
        if (Trace(ray, objects) is HitPayload payload && payload.ObjectIndex >= 0)
        {
            Geometry obj = objects[payload.ObjectIndex];
            Material material = materials[obj.MaterialIndex];

            Vector3d position = ray.At(payload.Intersection.TNear);

            SurfaceProperties surface = Geometry.GetSurfaceProperties(obj, position, payload.Intersection.UV);

            switch (material.MaterialType)
            {
                case MaterialType.ReflectAndRefract:
                    {
                        return Vector3d.Zero;
                    }
                case MaterialType.Reflect:
                    {
                        return Vector3d.Zero;
                    }
                default:
                    {
                        Vector3d lightAmt = Vector3d.Zero;
                        Vector3d specularColor = Vector3d.Zero;

                        Vector3d shadowOrigin = Vector3d.Dot(ray.Direction, surface.Normal) < 0.0f
                            ? position + (surface.Normal * scene.Epsilon)
                            : position - (surface.Normal * scene.Epsilon);

                        for (int i = 0; i < lights.Length; i++)
                        {
                            Light light = lights[i];

                            Vector3d lightDir = light.Position - position;
                            float lightDistance2 = lightDir.LengthSquared;
                            lightDir = Vector3d.Normalize(lightDir);
                            float LdotN = Math.Max(Vector3d.Dot(lightDir, surface.Normal), 0.0f);

                            HitPayload shadowHit = Trace(new Ray(shadowOrigin, lightDir), objects);
                            bool inShadow = shadowHit.ObjectIndex >= 0 && shadowHit.Intersection.TNear * shadowHit.Intersection.TNear < lightDistance2;

                            lightAmt += inShadow ? Vector3d.Zero : light.Intensity * LdotN;
                            Vector3d reflectionDirection = Vector3d.Reflect(-lightDir, surface.Normal);

                            specularColor += MathF.Pow(Math.Max(-Vector3d.Dot(reflectionDirection, ray.Direction), 0.0f), material.SpecularExponent) * light.Intensity;
                        }

                        return lightAmt * Geometry.EvalDiffuseColor(obj, material, surface.ST) * material.Kd + specularColor * material.Ks;
                    }
            }
        }

        return scene.BackgroundColor;
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
}
