using Maths;

namespace PA7;

internal unsafe struct BVHBuildNode
{
    public Bounds3d Bounds;

    public BVHBuildNode* Left;

    public BVHBuildNode* Right;

    public Handle<Shape> Shape;

    public float Area;
}

