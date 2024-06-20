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
            GeometryType.Triangle => IntersectTriangle(geometry, ray),
            _ => Intersection.False
        };
    }

    public static SurfaceProperties GetSurfaceProperties(Geometry geometry, Vector3d position, Vector2d uv)
    {
        return geometry.Type switch
        {
            GeometryType.Sphere => GetSurfacePropertiesSphere(geometry, position, uv),
            GeometryType.Triangle => GetSurfacePropertiesTriangle(geometry, position, uv),
            _ => new SurfaceProperties(position, uv)
        };
    }

    public static Vector3d EvalDiffuseColor(Geometry geometry, Material material, Vector2d st)
    {
        return geometry.Type switch
        {
            GeometryType.Sphere => EvalDiffuseColorSphere(material),
            GeometryType.Triangle => EvalDiffuseColorTriangle(material, st),
            _ => default
        };
    }

    #region Sphere
    private static Intersection IntersectSphere(Geometry geometry, Ray ray)
    {
        Vector3d L = ray.Origin - geometry.Center;
        float a = ray.Direction.LengthSquared;
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

    #region Triangle
    private static Intersection IntersectTriangle(Geometry geometry, Ray ray)
    {
        Vector3d v0 = geometry.Triangle.A.Position;
        Vector3d v1 = geometry.Triangle.B.Position;
        Vector3d v2 = geometry.Triangle.C.Position;

        Vector3d e1 = v1 - v0;
        Vector3d e2 = v2 - v0;
        Vector3d s0 = ray.Origin - v0;
        Vector3d s1 = Vector3d.Cross(ray.Direction, e2);
        Vector3d s2 = Vector3d.Cross(s0, e1);

        float invE1DotS1 = 1.0f / Vector3d.Dot(e1, s1);
        float tnear = Vector3d.Dot(s2, e2) * invE1DotS1;
        float b1 = Vector3d.Dot(s0, s1) * invE1DotS1;
        float b2 = Vector3d.Dot(ray.Direction, s2) * invE1DotS1;

        if (tnear >= 0 && b1 >= 0 && b2 >= 0 && (b1 + b2) <= 1)
        {
            return new Intersection(tnear, new Vector2d(b1, b2));
        }

        return Intersection.False;
    }

    private static SurfaceProperties GetSurfacePropertiesTriangle(Geometry geometry, Vector3d position, Vector2d uv)
    {
        float b0 = 1.0f - uv.X - uv.Y;

        Vector3d v0 = geometry.Triangle.A.Position;
        Vector3d v1 = geometry.Triangle.B.Position;
        Vector3d v2 = geometry.Triangle.C.Position;

        Vector3d e0 = Vector3d.Normalize(v1 - v0);
        Vector3d e1 = Vector3d.Normalize(v2 - v0);
        Vector3d normal = Vector3d.Normalize(Vector3d.Cross(e0, e1));

        Vertex vertex = Vertex.Interpolate(geometry.Triangle.A, geometry.Triangle.B, geometry.Triangle.C, new Vector3d(b0, uv.X, uv.Y));

        return new SurfaceProperties(position, uv, normal, vertex.TexCoord);
    }

    private static Vector3d EvalDiffuseColorTriangle(Material material, Vector2d st)
    {
        float scale = 5.0f;
        float pattern = (MathF.Floor(st.X * scale) + MathF.Floor(st.Y * scale)) % 2.0f;

        return (pattern < 1.0f) ? new Vector3d(0.815f, 0.235f, 0.031f) : material.DiffuseColor;
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