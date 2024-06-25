using Maths;

namespace PA6;

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

    public BVHAccel(SplitMethod splitMethod, Geometry[] geometries)
    {
        _splitMethod = splitMethod;
        _alloter = new();
        _root = RecursiveBuild(geometries);
    }

    public Intersection Intersect(Ray ray)
    {
        return GetIntersection(ray, _root);
    }

    public void Dispose()
    {
        _alloter.Dispose();
    }

    private BVHBuildNode* RecursiveBuild(Geometry[] geometries)
    {
        BVHBuildNode* node = _alloter.Allocate();

        Bounds3d bounds = new();
        for (int i = 0; i < geometries.Length; i++)
        {
            bounds = Bounds3d.Union(bounds, geometries[i].GetBounds());
        }

        if (geometries.Length == 1)
        {
            node->Bounds = bounds;
            node->Left = null;
            node->Right = null;
            node->Geometry = geometries[0].Handle;
        }
        else if (geometries.Length == 2)
        {
            node->Left = RecursiveBuild([geometries[0]]);
            node->Right = RecursiveBuild([geometries[1]]);
            node->Bounds = Bounds3d.Union(node->Left->Bounds, node->Right->Bounds);
        }
        else
        {
            Bounds3d centroidBounds = new();
            for (int i = 0; i < geometries.Length; i++)
            {
                centroidBounds = Bounds3d.Union(centroidBounds, geometries[i].GetBounds().Centroid);
            }

            int dim = centroidBounds.MaximumExtent;
            switch (dim)
            {
                case 0:
                    {
                        // Sort by x
                        Array.Sort(geometries, (a, b) => a.GetBounds().Centroid.X.CompareTo(b.GetBounds().Centroid.X));
                    }
                    break;
                case 1:
                    {
                        // Sort by y
                        Array.Sort(geometries, (a, b) => a.GetBounds().Centroid.Y.CompareTo(b.GetBounds().Centroid.Y));
                    }
                    break;
                default:
                    {
                        // Sort by z
                        Array.Sort(geometries, (a, b) => a.GetBounds().Centroid.Z.CompareTo(b.GetBounds().Centroid.Z));
                    }
                    break;
            }

            node->Left = RecursiveBuild(geometries[..(geometries.Length / 2)]);
            node->Right = RecursiveBuild(geometries[(geometries.Length / 2)..]);
            node->Bounds = Bounds3d.Union(node->Left->Bounds, node->Right->Bounds);
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
                intersection = node->Geometry.Target.GetIntersection(ray);
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
}
