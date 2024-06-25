using Maths;

namespace PA6;

internal class Mesh(Triangle[] triangles) : Geometry
{
    private readonly BVHAccel _bvh = new(triangles);

    public override Intersection GetIntersection(Ray ray)
    {
        return _bvh.Intersect(ray);
    }

    public override Vector3d EvalDiffuseColor(Intersection intersection)
    {
        return intersection.Geometry.Target.EvalDiffuseColor(intersection);
    }

    public override Bounds3d GetBounds()
    {
        return _bvh.GetBounds();
    }

    public override void Dispose()
    {
        _bvh.Dispose();

        base.Dispose();
    }
}
