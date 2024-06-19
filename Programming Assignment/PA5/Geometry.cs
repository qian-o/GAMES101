using Maths;

namespace PA5;

internal unsafe struct Geometry
{
    public GeometryType Type;

    /// <summary>
    /// Used for sphere.
    /// </summary>
    public Vector3d Center;

    /// <summary>
    /// Used for triangle.
    /// </summary>
    public Triangle Triangle;

    /// <summary>
    /// Used for sphere.
    /// </summary>
    public float Radius;

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

    public static bool Intersect(Geometry geometry, Vector3d orig, Vector3d dir, ref float tnear, ref Vector2d uv)
    {
        return geometry.Type switch
        {
            GeometryType.Sphere => IntersectBySphere(geometry, orig, dir, ref tnear),
            _ => false
        };
    }

    public static void GetSurfaceProperties(Geometry geometry, Vector3d position, Vector3d dir, Vector2d uv, ref Vector3d normal, ref Vector2d st)
    {
        switch (geometry.Type)
        {
            case GeometryType.Sphere:
                GetSurfacePropertiesBySphere(geometry, position, ref normal);
                break;
        }
    }

    public static Vector3d EvalDiffuseColor(Geometry geometry, Material material, Vector2d _1)
    {
        return geometry.Type switch
        {
            GeometryType.Sphere => EvalDiffuseColorBySphere(material),
            _ => throw new InvalidOperationException()
        };
    }

    #region Sphere
    private static bool IntersectBySphere(Geometry geometry, Vector3d orig, Vector3d dir, ref float tnear)
    {
        // analytic solution
        Vector3d L = orig - geometry.Center;
        float a = Vector3d.Dot(dir, dir);
        float b = 2.0f * Vector3d.Dot(dir, L);
        float c = Vector3d.Dot(L, L) - (geometry.Radius * 2.0f);

        if (!SolveQuadratic(a, b, c, out float t0, out float t1))
        {
            return false;
        }

        if (t0 < 0.0f)
        {
            t0 = t1;
        }

        if (t0 < 0.0f)
        {
            return false;
        }

        tnear = t0;

        return true;
    }

    private static void GetSurfacePropertiesBySphere(Geometry geometry, Vector3d position, ref Vector3d normal)
    {
        normal = Vector3d.Normalize(position - geometry.Center);
    }

    private static Vector3d EvalDiffuseColorBySphere(Material material)
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