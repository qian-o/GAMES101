using ILGPU;
using ILGPU.Runtime;
using Maths;

namespace PA5;

internal struct CastRayStack
{
    public bool isHit;

    public SceneProperties Scene;

    public Ray Ray;

    public Intersection Intersection;

    public int Leaf1;

    public int Leaf2;

    public static CastRayStack Create(SceneProperties scene,
                                      Ray ray,
                                      ArrayView1D<Geometry, Stride1D.Dense> objects,
                                      ArrayView1D<Material, Stride1D.Dense> materials)
    {
        HitPayload payload = Trace(ray, objects, materials);

        return new CastRayStack
        {
            isHit = payload.IsHit,
            Scene = scene,
            Ray = ray,
            Intersection = payload.Intersection,
            Leaf1 = -1,
            Leaf2 = -1
        };
    }

    public readonly Ray ReflectRay()
    {
        Vector3d position = Ray.At(Intersection.TNear);

        SurfaceProperties surface = Geometry.GetSurfaceProperties(Intersection.Geometry, position, Intersection.UV);

        Vector3d reflectDir = Vector3d.Normalize(Vector3d.Reflect(Ray.Direction, surface.Normal));

        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, surface.Normal) < 0.0f
            ? position - (surface.Normal * Scene.Epsilon)
            : position + (surface.Normal * Scene.Epsilon);

        Ray reflectRay = new(reflectOrigin, reflectDir);

        return reflectRay;
    }

    public readonly Ray RefractRay()
    {
        Vector3d position = Ray.At(Intersection.TNear);

        SurfaceProperties surface = Geometry.GetSurfaceProperties(Intersection.Geometry, position, Intersection.UV);

        Vector3d refractDir = Vector3d.Normalize(Vector3d.Refract(Ray.Direction, surface.Normal, Intersection.Material.Ior));

        Vector3d refractOrigin = Vector3d.Dot(refractDir, surface.Normal) < 0.0f
            ? position - (surface.Normal * Scene.Epsilon)
            : position + (surface.Normal * Scene.Epsilon);

        Ray refractRay = new(refractOrigin, refractDir);

        return refractRay;
    }

    public readonly Vector3d Color(ArrayView<CastRayStack> stacks1,
                                   ArrayView<CastRayStack> stacks2,
                                   ArrayView1D<Geometry, Stride1D.Dense> objects,
                                   ArrayView1D<Material, Stride1D.Dense> materials,
                                   ArrayView1D<Light, Stride1D.Dense> lights)
    {
        if (!isHit)
        {
            return Scene.BackgroundColor;
        }

        return Intersection.Material.MaterialType switch
        {
            MaterialType.DiffuseAndGlossy => DiffuseAndGlossyColor(objects, materials, lights),
            MaterialType.ReflectAndRefract => ReflectAndRefractColor(stacks1, stacks2, objects, materials, lights),
            MaterialType.Reflect => ReflectColor(stacks1, stacks2, objects, materials, lights),
            _ => Vector3d.Zero
        };
    }

    private readonly Vector3d DiffuseAndGlossyColor(ArrayView1D<Geometry, Stride1D.Dense> objects,
                                                    ArrayView1D<Material, Stride1D.Dense> materials,
                                                    ArrayView1D<Light, Stride1D.Dense> lights)
    {
        Geometry obj = Intersection.Geometry;
        Material material = Intersection.Material;

        Vector3d position = Ray.At(Intersection.TNear);

        SurfaceProperties surface = Geometry.GetSurfaceProperties(obj, position, Intersection.UV);

        Vector3d lightAmt = Vector3d.Zero;
        Vector3d specularColor = Vector3d.Zero;

        Vector3d shadowOrigin = Vector3d.Dot(Ray.Direction, surface.Normal) < 0.0f
            ? position + (surface.Normal * Scene.Epsilon)
            : position - (surface.Normal * Scene.Epsilon);

        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];

            Vector3d lightDir = light.Position - position;
            float lightDistance2 = lightDir.LengthSquared;
            lightDir = Vector3d.Normalize(lightDir);
            float LdotN = Math.Max(Vector3d.Dot(lightDir, surface.Normal), 0.0f);

            bool inShadow = Trace(new Ray(shadowOrigin, lightDir), objects, materials) is HitPayload hitPayload
                            && hitPayload.IsHit
                            && hitPayload.Intersection.TNear * hitPayload.Intersection.TNear < lightDistance2;

            lightAmt += inShadow ? Vector3d.Zero : light.Intensity * LdotN;
            Vector3d reflectionDirection = Vector3d.Reflect(-lightDir, surface.Normal);

            specularColor += MathF.Pow(Math.Max(-Vector3d.Dot(reflectionDirection, Ray.Direction), 0.0f), material.SpecularExponent) * light.Intensity;
        }

        return lightAmt * Geometry.EvalDiffuseColor(obj, material, surface.ST) * material.Kd + specularColor * material.Ks;
    }

    private readonly Vector3d ReflectAndRefractColor(ArrayView<CastRayStack> stacks1,
                                                     ArrayView<CastRayStack> stacks2,
                                                     ArrayView1D<Geometry, Stride1D.Dense> objects,
                                                     ArrayView1D<Material, Stride1D.Dense> materials,
                                                     ArrayView1D<Light, Stride1D.Dense> lights)
    {
        Vector3d position = Ray.At(Intersection.TNear);

        SurfaceProperties surface = Geometry.GetSurfaceProperties(Intersection.Geometry, position, Intersection.UV);

        float kr = MathsHelper.Fresnel(Ray.Direction, surface.Normal, Intersection.Material.Ior);

        Vector3d reflectColor = Leaf1 >= 0 ? stacks1[Leaf1].Color(stacks1, stacks2, objects, materials, lights) : Scene.BackgroundColor;
        Vector3d refractColor = Leaf2 >= 0 ? stacks2[Leaf2].Color(stacks1, stacks2, objects, materials, lights) : Scene.BackgroundColor;

        return (reflectColor * kr) + (refractColor * (1 - kr));
    }

    private readonly Vector3d ReflectColor(ArrayView<CastRayStack> stacks1,
                                           ArrayView<CastRayStack> stacks2,
                                           ArrayView1D<Geometry, Stride1D.Dense> objects,
                                           ArrayView1D<Material, Stride1D.Dense> materials,
                                           ArrayView1D<Light, Stride1D.Dense> lights)
    {
        Vector3d position = Ray.At(Intersection.TNear);

        SurfaceProperties surface = Geometry.GetSurfaceProperties(Intersection.Geometry, position, Intersection.UV);

        float kr = MathsHelper.Fresnel(Ray.Direction, surface.Normal, Intersection.Material.Ior);

        return (Leaf1 >= 0 ? stacks1[Leaf1].Color(stacks1, stacks2, objects, materials, lights) : Scene.BackgroundColor) * kr;
    }

    public static HitPayload Trace(Ray ray,
                                   ArrayView1D<Geometry, Stride1D.Dense> objects,
                                   ArrayView1D<Material, Stride1D.Dense> materials)
    {
        float tNear = float.MaxValue;

        HitPayload payload = HitPayload.False;

        for (int i = 0; i < objects.Length; i++)
        {
            Geometry geometry = objects[i];
            Material material = materials[geometry.MaterialIndex];

            if (Geometry.Intersect(geometry, material, ray) is Intersection intersection && intersection.TNear < tNear)
            {
                payload = new HitPayload(intersection);

                tNear = intersection.TNear;
            }
        }

        return payload;
    }
}
