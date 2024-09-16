using Maths;

namespace PA7;

internal class Mesh : Shape
{
    private readonly Material _material;
    private readonly BVHAccel _bvh;

    public Mesh(Triangle[] triangles, Material material)
    {
        Vector3d minVert = new(float.MaxValue);
        Vector3d maxVert = new(float.MinValue);
        float area = 0.0f;

        foreach (var item in triangles)
        {
            minVert = Vector3d.Min(minVert, item.GetBounds().Min);
            maxVert = Vector3d.Max(maxVert, item.GetBounds().Max);
            area += item.GetArea();
        }

        _material = material;
        _bvh = new BVHAccel(triangles);
    }

    public override bool Intersect(ref readonly Ray ray, ref float tnear, ref uint index)
    {
        return false;
    }

    public override Intersection GetIntersection(ref readonly Ray ray)
    {
        return _bvh.Intersect(ray);
    }

    public override void GetSufaceProperties(ref readonly Vector3d position, ref readonly Vector2d uv, ref readonly uint index, ref Vector3d normal, ref Vector2d st)
    {
    }

    public override Vector3d EvalDiffuseColor(ref readonly Vector2d st)
    {
        return new(0.5f);
    }

    public override Bounds3d GetBounds()
    {
        return _bvh.GetBounds();
    }

    public override float GetArea()
    {
        return _bvh.GetArea();
    }

    public override void Sample(ref Intersection intersection, ref float pdf)
    {
        _bvh.Sample(ref intersection, ref pdf);

        intersection.Emit = _material.Emission;
    }

    public override bool HasEmit()
    {
        return _material.HasEmission();
    }

    public override void Dispose()
    {
        _bvh.Dispose();

        base.Dispose();
    }
}
