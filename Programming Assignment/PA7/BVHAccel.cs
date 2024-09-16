using Maths;

namespace PA7;

internal enum SplitMethod
{
    NAIVE,
    SAH
}

internal unsafe class BVHAccel : IDisposable
{
    private readonly SplitMethod _splitMethod;
    private readonly Alloter<BVHBuildNode> _alloter;
    private readonly BVHBuildNode* _root;

    public BVHAccel(Shape[] shapes, SplitMethod splitMethod = SplitMethod.NAIVE)
    {
        _splitMethod = splitMethod;
        _alloter = new();
        _root = shapes.Length > 0 ? RecursiveBuild(shapes) : null;
    }

    public Intersection Intersect(Ray ray)
    {
        if (_root == null)
        {
            return default;
        }

        return GetIntersection(ray, _root);
    }

    public Bounds3d GetBounds()
    {
        if (_root == null)
        {
            return default;
        }

        return _root->Bounds;
    }

    public float GetArea()
    {
        if (_root == null)
        {
            return 0;
        }

        return _root->Area;
    }

    public void Sample(ref Intersection intersection, ref float pdf)
    {
        float p = MathF.Sqrt(Random.Shared.NextSingle()) * _root->Area;
    }

    public void Dispose()
    {
        _alloter.Dispose();
    }

    private BVHBuildNode* RecursiveBuild(Shape[] shapes)
    {
        BVHBuildNode* node = _alloter.Allocate();

        Bounds3d bounds = new();
        for (int i = 0; i < shapes.Length; i++)
        {
            bounds = Bounds3d.Union(bounds, shapes[i].GetBounds());
        }

        if (shapes.Length == 1)
        {
            node->Bounds = bounds;
            node->Left = null;
            node->Right = null;
            node->Shape = shapes[0].Handle;
            node->Area = shapes[0].GetArea();
        }
        else if (shapes.Length == 2)
        {
            node->Left = RecursiveBuild([shapes[0]]);
            node->Right = RecursiveBuild([shapes[1]]);
            node->Bounds = Bounds3d.Union(node->Left->Bounds, node->Right->Bounds);
            node->Area = node->Left->Area + node->Right->Area;
        }
        else
        {
            Bounds3d centroidBounds = new();
            for (int i = 0; i < shapes.Length; i++)
            {
                centroidBounds = Bounds3d.Union(centroidBounds, shapes[i].GetBounds().Centroid);
            }

            int dim = centroidBounds.MaximumExtent;
            switch (dim)
            {
                case 0:
                    {
                        // Sort by x
                        Array.Sort(shapes, (a, b) => a.GetBounds().Centroid.X.CompareTo(b.GetBounds().Centroid.X));
                    }
                    break;
                case 1:
                    {
                        // Sort by y
                        Array.Sort(shapes, (a, b) => a.GetBounds().Centroid.Y.CompareTo(b.GetBounds().Centroid.Y));
                    }
                    break;
                default:
                    {
                        // Sort by z
                        Array.Sort(shapes, (a, b) => a.GetBounds().Centroid.Z.CompareTo(b.GetBounds().Centroid.Z));
                    }
                    break;
            }

            node->Left = RecursiveBuild(shapes[..(shapes.Length / 2)]);
            node->Right = RecursiveBuild(shapes[(shapes.Length / 2)..]);
            node->Bounds = Bounds3d.Union(node->Left->Bounds, node->Right->Bounds);
            node->Area = node->Left->Area + node->Right->Area;
        }

        return node;
    }

    private static Intersection GetIntersection(Ray ray, BVHBuildNode* node)
    {
        Intersection intersection = new();

        bool[] dirIsNeg = [ray.Direction.X < 0, ray.Direction.Y < 0, ray.Direction.Z < 0];

        if (node->Bounds.IntersectPntersect(ray, dirIsNeg))
        {
            if (node->Left == null && node->Right == null)
            {
                intersection = node->Shape.Target.GetIntersection(in ray);
            }
            else
            {
                Intersection leftIntersection = GetIntersection(ray, node->Left);
                Intersection rightIntersection = GetIntersection(ray, node->Right);

                if (leftIntersection.Happened && rightIntersection.Happened)
                {
                    if (leftIntersection.Distance < rightIntersection.Distance)
                    {
                        intersection = leftIntersection;
                    }
                    else
                    {
                        intersection = rightIntersection;
                    }
                }
                else if (leftIntersection.Happened)
                {
                    intersection = leftIntersection;
                }
                else if (rightIntersection.Happened)
                {
                    intersection = rightIntersection;
                }
            }
        }

        return intersection;
    }

    private void GetSample(BVHBuildNode* node, float p, ref Intersection intersection, ref float pdf)
    {
        if (node->Left == null && node->Right == null)
        {
            node->Shape.Target.Sample(ref intersection, ref pdf);
            pdf *= node->Area;

            return;
        }

        if (p < node->Left->Area)
        {
            GetSample(node->Left, p, ref intersection, ref pdf);
            pdf *= node->Area;
        }
        else
        {
            GetSample(node->Right, p - node->Left->Area, ref intersection, ref pdf);
            pdf *= node->Area;
        }
    }
}
