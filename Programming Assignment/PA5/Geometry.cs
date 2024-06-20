using Maths;
using PA.Graphics;

namespace PA5;

internal struct Geometry
{
    public GeometryType Type;

    /// <summary>
    /// Used for sphere.
    /// </summary>
    public Vector3d Center;

    /// <summary>
    /// Used for sphere.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Used for triangle.
    /// </summary>
    public Triangle Triangle;

    /// <summary>
    /// Scene material index.
    /// </summary>
    public int MaterialIndex;

    public static Geometry CreateSphere(Vector3d center, float radius, int materialIndex)
    {
        return new Geometry
        {
            Type = GeometryType.Sphere,
            Center = center,
            Radius = radius,
            MaterialIndex = materialIndex
        };
    }

    public static Geometry CreateTriangle(Triangle triangle, int materialIndex)
    {
        return new Geometry
        {
            Type = GeometryType.Triangle,
            Triangle = triangle,
            MaterialIndex = materialIndex
        };
    }

    public static Intersection Intersect(Geometry geometry, Ray ray)
    {
        return geometry.Type switch
        {
            GeometryType.Sphere => IntersectSphere(geometry, ray),
            _ => Intersection.False
        };
    }

    public static SurfaceProperties GetSurfaceProperties(Geometry geometry, Vector3d position, Vector3d dir, Vector2d uv)
    {
        return geometry.Type switch
        {
            GeometryType.Sphere => GetSurfacePropertiesSphere(geometry, position, uv),
            _ => new SurfaceProperties(position, uv)
        };
    }

    public static Vector3d EvalDiffuseColor(Geometry geometry, Material material, Vector2d _1)
    {
        return geometry.Type switch
        {
            GeometryType.Sphere => EvalDiffuseColorSphere(material),
            _ => throw new InvalidOperationException()
        };
    }

    #region Sphere
    private static Intersection IntersectSphere(Geometry geometry, Ray ray)
    {
        Vector3d L = ray.Origin - geometry.Center;
        float a = Vector3d.Dot(ray.Direction, ray.Direction);
        float b = Vector3d.Dot(ray.Direction, L) * 2.0f;
        float c = Vector3d.Dot(L, L) - (geometry.Radius * 2.0f);

        if (!SolveQuadratic(a, b, c, out float t0, out float t1))
        {
            return Intersection.False;
        }

        if (t0 < 0.0f)
        {
            t0 = t1;
        }

        if (t0 < 0.0f)
        {
            return Intersection.False;
        }

        return new Intersection(t0);
    }

    private static SurfaceProperties GetSurfacePropertiesSphere(Geometry geometry, Vector3d position, Vector2d uv)
    {
        return new SurfaceProperties(position, uv, Vector3d.Normalize(position - geometry.Center));
    }

    private static Vector3d EvalDiffuseColorSphere(Material material)
    {
        return material.DiffuseColor;
    }
    #endregion

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

internal enum GeometryType
{
    Sphere,
    Triangle
}