using Maths;
using PA.Graphics;

namespace PA6;

internal class Renderer(Scene scene)
{
    #region Scene Properties
    public int Width => scene.Width;

    public int Height => scene.Height;

    public FrameBuffer FrameBuffer => scene.FrameBuffer;
    #endregion

    public void Render()
    {
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

                FrameBuffer[pixel, sample] = new Fragment(new Vector4d(CastRay(new Ray(scene.Camera.Position, dir), lights, 0), 1.0f), 1.0f);
            }
        });

        FrameBuffer.Present();
    }

    private Vector3d CastRay(Ray ray, Light[] lights, int depth)
    {
        if (depth > scene.MaxDepth)
        {
            return Vector3d.Zero;
        }

        Vector3d hitColor = scene.BackgroundColor;

        Intersection intersection = scene.GetIntersection(ray);

        if (intersection.Happened)
        {
            Vector3d position = intersection.Position;
            Vector3d normal = intersection.Normal;
            Geometry geometry = intersection.Geometry.Target;
            Material material = geometry.Material;

            switch (material.Type)
            {
                case MaterialType.ReflectionAndRefraction:
                    {
                        Vector3d reflectDir = Vector3d.Normalize(Vector3d.Reflect(ray.Direction, normal));
                        Vector3d refractDir = Vector3d.Normalize(Vector3d.Refract(ray.Direction, normal, material.Ior));

                        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, normal) < 0.0f
                            ? position - (normal * scene.Epsilon)
                            : position + (normal * scene.Epsilon);

                        Vector3d refractOrigin = Vector3d.Dot(refractDir, normal) < 0.0f
                            ? position - (normal * scene.Epsilon)
                            : position + (normal * scene.Epsilon);

                        Ray reflectRay = new(reflectOrigin, reflectDir);
                        Ray refractRay = new(refractOrigin, refractDir);

                        Vector3d reflectColor = CastRay(reflectRay, lights, depth + 1);
                        Vector3d refractColor = CastRay(refractRay, lights, depth + 1);

                        float kr = MathsHelper.Fresnel(ray.Direction, normal, material.Ior);

                        hitColor = (reflectColor * kr) + (refractColor * (1 - kr));
                    }
                    break;
                case MaterialType.Reflection:
                    {
                        Vector3d reflectDir = Vector3d.Normalize(Vector3d.Reflect(ray.Direction, normal));

                        Vector3d reflectOrigin = Vector3d.Dot(reflectDir, normal) < 0.0f
                            ? position - (normal * scene.Epsilon)
                            : position + (normal * scene.Epsilon);

                        Ray reflectRay = new(reflectOrigin, reflectDir);

                        float kr = MathsHelper.Fresnel(ray.Direction, normal, material.Ior);

                        hitColor = CastRay(reflectRay, lights, depth + 1) * kr;
                    }
                    break;
                default:
                    {
                        Vector3d lightAmt = Vector3d.Zero;
                        Vector3d specularColor = Vector3d.Zero;

                        Vector3d shadowOrigin = Vector3d.Dot(ray.Direction, normal) < 0.0f
                            ? position + (normal * scene.Epsilon)
                            : position - (normal * scene.Epsilon);

                        for (int i = 0; i < lights.Length; i++)
                        {
                            Light light = lights[i];

                            Vector3d lightDir = Vector3d.Normalize(light.Position - position);

                            float LdotN = Math.Max(Vector3d.Dot(lightDir, normal), 0.0f);

                            bool inShadow = scene.GetIntersection(new Ray(shadowOrigin, lightDir)) is Intersection test
                                            && test.Happened
                                            && MathF.Pow(test.Distance, 2) < lightDir.LengthSquared;

                            lightAmt += inShadow ? Vector3d.Zero : light.Intensity * LdotN;

                            Vector3d reflectionDirection = Vector3d.Reflect(-lightDir, normal);

                            specularColor += MathF.Pow(Math.Max(-Vector3d.Dot(reflectionDirection, ray.Direction), 0.0f), material.SpecularExponent) * light.Intensity;
                        }

                        hitColor = (lightAmt * geometry.EvalDiffuseColor(intersection) * material.Kd) + (specularColor * material.Ks);
                    }
                    break;
            }
        }

        return hitColor;
    }
}
