using Maths;

namespace PA6;

internal class Model(Mesh[] meshes) : Geometry
{
    private readonly BVHAccel _bvh = new(meshes);

    public override Intersection GetIntersection(Ray ray)
    {
        return _bvh.Intersect(ray);
    }

    public override Vector3d EvalDiffuseColor(Intersection intersection)
    {
        return Material.Color;
    }

    public override Bounds3d GetBounds()
    {
        return _bvh.GetBounds();
    }
}
