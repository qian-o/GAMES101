using Maths;
using PA.Graphics;

namespace PA5;

internal unsafe class Renderer(Scene scene)
{
    #region Scene Properties
    public int Width => scene.Width;

    public int Height => scene.Height;

    public FrameBuffer FrameBuffer => scene.FrameBuffer;
    #endregion

    public void Render()
    {
        Material[] materials = [.. scene.Materials];
        Geometry[] objects = [.. scene.Objects];
        Light[] lights = [.. scene.Lights];

        ParallelHelper.Foreach(FrameBuffer.Pixels, (pixel) =>
        {
            for (int sample = 0; sample < FrameBuffer!.Samples; sample++)
            {
                Vector2d offset = FrameBuffer.Patterns[sample];

                float x = pixel.X + offset.X;
                float y = pixel.Y + offset.Y;

                float scale = MathF.Tan(scene.Camera.Fov.Radians);
                float imageAspectRatio = scene.Width / (float)scene.Height;

                x = MathsHelper.RangeMap(x, 0.0f, scene.Width - 1.0f, -1.0f, 1.0f);
                y = MathsHelper.RangeMap(y, 0.0f, scene.Height - 1.0f, 1.0f, -1.0f);

                x *= imageAspectRatio * scale;
                y *= scale;

                Vector3d dir = new(x, y, -1);
                dir = Vector3d.Normalize(dir);

                FrameBuffer[pixel, sample] = new Fragment(new Vector4d(CastRay(new Ray(scene.Camera.Position, dir), materials, objects, lights, 0), 1.0f), 1.0f);
            }
        });

        FrameBuffer.Present();
    }

    private Vector3d CastRay(Ray ray, Material[] materials, Geometry[] objects, Light[] lights, int depth)
    {
        if (depth > scene.MaxDepth)
        {
            return scene.BackgroundColor;
        }

        if (Trace(ray, objects, materials) is HitPayload payload && payload.IsHit)
        {
            Geometry obj = payload.Intersection.Geometry;
            Material material = payload.Intersection.Material;

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

                        Vector3d reflectColor = CastRay(reflectRay, materials, objects, lights, depth + 1);
                        Vector3d refractColor = CastRay(refractRay, materials, objects, lights, depth + 1);

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

                        return CastRay(reflectRay, materials, objects, lights, depth + 1) * kr;
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

                            bool inShadow = Trace(new Ray(shadowOrigin, lightDir), objects, materials) is HitPayload hitPayload
                                            && hitPayload.IsHit
                                            && hitPayload.Intersection.TNear * hitPayload.Intersection.TNear < lightDistance2;

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

    private static HitPayload Trace(Ray ray, Geometry[] objects, Material[] materials)
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
