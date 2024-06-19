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

    public Material Material;

    public static Geometry CreateSphere(Vector3d center, float radius, Material material)
    {
        return new Geometry
        {
            Type = GeometryType.Sphere,
            Center = center,
            Radius = radius,
            Material = material
        };
    }

    public static Geometry CreateTriangle(Triangle triangle, Material material)
    {
        return new Geometry
        {
            Type = GeometryType.Triangle,
            Triangle = triangle,
            Material = material
        };
    }

    public readonly bool Intersect(Vector3d orig, Vector3d dir, ref float tnear, ref uint _4, ref Vector2d _5)
    {
        return Type switch
        {
            GeometryType.Sphere => IntersectBySphere(orig, dir, ref tnear),
            _ => false
        };
    }

    public readonly void GetSurfaceProperties(Vector3d position, Vector3d _2, uint _3, Vector2d _4, ref Vector3d normal, ref Vector2d _6)
    {
        switch (Type)
        {
            case GeometryType.Sphere:
                GetSurfacePropertiesBySphere(position, ref normal);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    public readonly Vector3d EvalDiffuseColor(Vector2d _1)
    {
        return Type switch
        {
            GeometryType.Sphere => EvalDiffuseColorBySphere(),
            _ => throw new InvalidOperationException()
        };
    }

    #region Sphere
    private readonly bool IntersectBySphere(Vector3d orig, Vector3d dir, ref float tnear)
    {
        // analytic solution
        Vector3d L = orig - Center;
        float a = Vector3d.Dot(dir, dir);
        float b = 2 * Vector3d.Dot(dir, L);
        float c = Vector3d.Dot(L, L) - (Radius * 2.0f);

        if (!MathsHelper.SolveQuadratic(a, b, c, out float t0, out float t1))
        {
            return false;
        }

        if (t0 < 0)
        {
            t0 = t1;
        }

        if (t0 < 0)
        {
            return false;
        }

        tnear = t0;

        return true;
    }

    private readonly void GetSurfacePropertiesBySphere(Vector3d position, ref Vector3d normal)
    {
        normal = Vector3d.Normalize(position - Center);
    }

    private readonly Vector3d EvalDiffuseColorBySphere()
    {
        return Material.DiffuseColor;
    }
    #endregion
}

internal enum GeometryType
{
    Sphere,
    Triangle
}