using Maths;

namespace PA6;

internal unsafe struct BVHBuildNode
{
    public Bounds3d Bounds;

    public BVHBuildNode* Left;

    public BVHBuildNode* Right;

    public Handle<Geometry> Geometry;
}
