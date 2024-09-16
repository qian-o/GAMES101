using Maths;

namespace PA7;

internal class Triangle : Shape
{
    public Triangle(Vector3d v0, Vector3d v1, Vector3d v2, Material material)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Material = material;

        E1 = V1 - V0;
        E2 = V2 - V0;

        T0 = V1 - V0;
        T1 = V2 - V0;
        T2 = V2 - V1;

        Normal = Vector3d.Normalize(Vector3d.Cross(E1, E2));
        Area = 0.5f * Vector3d.Cross(E1, E2).Length;
    }

    public Vector3d V0 { get; }

    public Vector3d V1 { get; }

    public Vector3d V2 { get; }

    public Vector3d E1 { get; }

    public Vector3d E2 { get; }

    public Vector3d T0 { get; }

    public Vector3d T1 { get; }

    public Vector3d T2 { get; }

    public Vector3d Normal { get; }

    public float Area { get; }

    public Material Material { get; }

    public override bool Intersect(ref readonly Ray ray, ref float tnear, ref uint index)
    {
        return false;
    }

    public override Intersection GetIntersection(ref readonly Ray ray)
    {
        Intersection inter = new();

        Vector3d s0 = ray.Origin - V0;
        Vector3d s1 = Vector3d.Cross(ray.Direction, E2);
        Vector3d s2 = Vector3d.Cross(s0, E1);

        float invE1DotS1 = 1.0f / Vector3d.Dot(E1, s1);
        float tnear = Vector3d.Dot(s2, E2) * invE1DotS1;
        float b1 = Vector3d.Dot(s0, s1) * invE1DotS1;
        float b2 = Vector3d.Dot(ray.Direction, s2) * invE1DotS1;

        if (tnear >= 0 && b1 >= 0 && b2 >= 0 && (b1 + b2) <= 1)
        {
            inter.Happened = true;
            inter.Coords = ray.PointAt(tnear);
            inter.Normal = Normal;
            inter.Distance = tnear;
            inter.Shape = Handle;
            inter.Material = Material.Handle;
        }

        return inter;
    }

    public override void GetSufaceProperties(ref readonly Vector3d position, ref readonly Vector2d uv, ref readonly uint index, ref Vector3d normal, ref Vector2d st)
    {
        normal = Normal;
    }

    public override Vector3d EvalDiffuseColor(ref readonly Vector2d st)
    {
        return new(0.5f);
    }

    public override Bounds3d GetBounds()
    {
        return Bounds3d.Union(new Bounds3d(V0, V1), V2);
    }

    public override float GetArea()
    {
        return Area;
    }

    public override void Sample(ref Intersection pos, ref float pdf)
    {
        float x = MathF.Sqrt(Random.Shared.NextSingle());
        float y = Random.Shared.NextSingle();

        pos.Coords = (V0 * (1.0f - x)) + (V1 * (1.0f - y)) + (V2 * x * y);
        pos.Normal = Normal;
        pdf = 1.0f / Area;
    }

    public override bool HasEmit()
    {
        return Material.HasEmission();
    }
}
