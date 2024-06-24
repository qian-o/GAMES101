using Maths;

namespace PA6;

internal class Sphere(Vector3d center, float radius) : Geometry
{
    public Vector3d Center { get; set; } = center;

    public float Radius { get; set; } = radius;

    public override Intersection GetIntersection(Ray ray)
    {
        Intersection intersection = new();

        Vector3d L = ray.Origin - Center;

        float a = ray.Direction.LengthSquared;
        float b = Vector3d.Dot(ray.Direction, L) * 2.0f;
        float c = Vector3d.Dot(L, L) - (Radius * 2.0f);

        if (!SolveQuadratic(a, b, c, out float t0, out float t1))
        {
            return intersection;
        }

        if (t0 < 0.0f)
        {
            t0 = t1;
        }

        if (t0 < 0.0f)
        {
            return intersection;
        }

        intersection.Happened = true;
        intersection.Distance = t0;
        intersection.Geometry = Handle;
        intersection.Position = ray.PointAt(t0);
        intersection.Normal = Vector3d.Normalize(intersection.Position - Center);

        return intersection;
    }

    public override Vector3d EvalDiffuseColor(Intersection intersection)
    {
        return Material.Color;
    }

    public override Bounds3d GetBounds()
    {
        return new Bounds3d(Center - Radius, Center + Radius);
    }

    private static bool SolveQuadratic(float a, float b, float c, out float x0, out float x1)
    {
        float discr = (b * b) - (4.0f * a * c);

        if (discr < 0.0f)
        {
            x0 = x1 = 0.0f;

            return false;
        }
        else if (discr == 0.0f)
        {
            x0 = x1 = -0.5f * b / a;
        }
        else
        {
            float q = (b > 0.0f) ? -0.5f * (b + MathF.Sqrt(discr)) : -0.5f * (b - MathF.Sqrt(discr));
            x0 = q / a;
            x1 = c / q;
        }

        if (x0 > x1)
        {
            (x1, x0) = (x0, x1);
        }

        return true;
    }
}
