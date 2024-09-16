using Maths;
using PA.Graphics;

namespace PA7;
internal class Renderer(Scene scene)
{
    #region Scene Properties
    public int Width => scene.Width;

    public int Height => scene.Height;

    public FrameBuffer FrameBuffer => scene.FrameBuffer;
    #endregion

    public void Render()
    {
        const int spp = 2;

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

                if (imageAspectRatio > 1.0)
                {
                    x *= imageAspectRatio * scale;
                    y *= scale;
                }
                else
                {
                    x *= scale;
                    y *= scale / imageAspectRatio;
                }

                Vector3d dir = new(-x, y, 1);
                dir = Vector3d.Normalize(dir);

                Vector3d color = new(0.0f);
                for (int k = 0; k < spp; k++)
                {
                    color += CastRay(new Ray(scene.Camera.Position, dir));
                }

                FrameBuffer[pixel, sample] = new Fragment(new Vector4d(color / spp, 1.0f), 1.0f);
            }
        });

        FrameBuffer.Present();
    }

    private Vector3d CastRay(Ray ray)
    {
        Intersection intersection = scene.GetIntersection(ray);

        if (!intersection.Happened)
        {
            return scene.BackgroundColor;
        }

        Vector3d wo = -ray.Direction;

        return Shade(intersection, in wo);
    }

    private Vector3d Shade(Intersection hitObj, ref readonly Vector3d wo)
    {
        if (hitObj.Material.Target.HasEmission())
        {
            return hitObj.Material.Target.Emission;
        }

        const float Epsilon = 0.0005f;
        const float RussianRoulette = 0.8f;

        Vector3d loDir = new(0.0f);
        {
            float light_pdf = default;
            Intersection hitLight = default;
            SampleLight(ref hitLight, ref light_pdf);
            Vector3d obj2Light = hitLight.Coords - hitObj.Coords;
            Vector3d obj2LightDir = Vector3d.Normalize(obj2Light);

            Intersection t = scene.GetIntersection(new Ray(hitObj.Coords, obj2LightDir));
            if (t.Distance - obj2Light.Length > -Epsilon)
            {
                Vector3d fr = hitObj.Material.Target.Eval(wo, hitObj.Normal);
                float r2 = Vector3d.Dot(obj2Light, obj2Light);
                float cosA = MathF.Max(0.0f, Vector3d.Dot(hitObj.Normal, obj2LightDir));
                float cosB = MathF.Max(0.0f, Vector3d.Dot(hitLight.Normal, -obj2LightDir));
                loDir = hitLight.Emit * fr * cosA * cosB / r2 / light_pdf;
            }
        }

        Vector3d loIndir = new(0.0f);
        {
            if (Random.Shared.NextSingle() < RussianRoulette)
            {
                Vector3d dir2NextObj = Vector3d.Normalize(hitObj.Material.Target.Sample(hitObj.Normal));
                float pdf = hitObj.Material.Target.Pdf(wo, hitObj.Normal);
                if (pdf > Epsilon)
                {
                    Intersection nextObj = scene.GetIntersection(new Ray(hitObj.Coords, dir2NextObj));
                    if (nextObj.Happened && !nextObj.Material.Target.HasEmission())
                    {
                        Vector3d fr = hitObj.Material.Target.Eval(wo, hitObj.Normal);
                        float cos = MathF.Max(0.0f, Vector3d.Dot(dir2NextObj, hitObj.Normal));
                        dir2NextObj = -dir2NextObj;
                        loIndir = Shade(nextObj, ref dir2NextObj) * fr * cos / pdf / RussianRoulette;
                    }
                }
            }
        }

        return loDir + loIndir;
    }

    private void SampleLight(ref Intersection pos, ref float pdf)
    {
        float emitAreaSum = 0.0f;
        for (int i = 0; i < scene.Shapes.Count; i++)
        {
            if (scene.Shapes[i].HasEmit())
            {
                emitAreaSum += scene.Shapes[i].GetArea();
            }
        }

        float p = Random.Shared.NextSingle() * emitAreaSum;
        emitAreaSum = 0.0f;
        for (int i = 0; i < scene.Shapes.Count; i++)
        {
            if (scene.Shapes[i].HasEmit())
            {
                emitAreaSum += scene.Shapes[i].GetArea();
                if (p <= emitAreaSum)
                {
                    scene.Shapes[i].Sample(ref pos, ref pdf);
                    return;
                }
            }
        }
    }
}
