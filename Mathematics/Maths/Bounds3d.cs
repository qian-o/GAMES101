using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Bounds3d
{
    public Vector3d Min;

    public Vector3d Max;

    public Bounds3d(Vector3d p1, Vector3d p2)
    {
        Min = Vector3d.Min(p1, p2);
        Max = Vector3d.Max(p1, p2);
    }

    public Bounds3d(Vector3d p)
    {
        Min = p;
        Max = p;
    }

    public Bounds3d()
    {
        Min = Vector3d.MaxValue;
        Max = Vector3d.MinValue;
    }

    public unsafe Vector3d this[int index]
    {
        get
        {
            fixed (Vector3d* p = &Min)
            {
                return *(p + index);
            }
        }
        set
        {
            fixed (Vector3d* p = &Min)
            {
                *(p + index) = value;
            }
        }
    }

    public readonly Vector3d Diagonal => Max - Min;

    public readonly int MaximumExtent
    {
        get
        {
            Vector3d d = Diagonal;

            if (d.X > d.Y && d.X > d.Z)
            {
                return 0;
            }
            else if (d.Y > d.Z)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }

    public readonly double SurfaceArea
    {
        get
        {
            Vector3d d = Diagonal;

            return 2 * ((d.X * d.Y) + (d.X * d.Z) + (d.Y * d.Z));
        }
    }

    public readonly Vector3d Centroid => (Min + Max) / 2.0f;

    public readonly Bounds3d Intersect(Bounds3d bounds)
    {
        return new Bounds3d(Vector3d.Max(Min, bounds.Min), Vector3d.Min(Max, bounds.Max));
    }

    public readonly Vector3d Offset(Vector3d p)
    {
        Vector3d o = p - Min;

        if (Max.X > Min.X)
        {
            o.X /= Max.X - Min.X;
        }

        if (Max.Y > Min.Y)
        {
            o.Y /= Max.Y - Min.Y;
        }

        if (Max.Z > Min.Z)
        {
            o.Z /= Max.Z - Min.Z;
        }

        return o;
    }

    public static bool Overlaps(Bounds3d b1, Bounds3d b2)
    {
        bool x = b1.Max.X >= b2.Min.X && b1.Min.X <= b2.Max.X;
        bool y = b1.Max.Y >= b2.Min.Y && b1.Min.Y <= b2.Max.Y;
        bool z = b1.Max.Z >= b2.Min.Z && b1.Min.Z <= b2.Max.Z;

        return x && y && z;
    }

    public static bool Inside(Vector3d p, Bounds3d b)
    {
        return p.X >= b.Min.X && p.X <= b.Max.X && p.Y >= b.Min.Y && p.Y <= b.Max.Y && p.Z >= b.Min.Z && p.Z <= b.Max.Z;
    }

    public static Bounds3d Union(Bounds3d b1, Bounds3d b2)
    {
        return new Bounds3d(Vector3d.Min(b1.Min, b2.Min), Vector3d.Max(b1.Max, b2.Max));
    }

    public static Bounds3d Union(Bounds3d b, Vector3d p)
    {
        return new Bounds3d(Vector3d.Min(b.Min, p), Vector3d.Max(b.Max, p));
    }
}
