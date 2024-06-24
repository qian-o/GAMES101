using Maths;

namespace PA6;

internal static class Bounds3dExtensions
{
    public static bool IntersectPntersect(this Bounds3d bounds, Ray ray, bool[] dirIsNeg)
    {
        if (dirIsNeg.Length != 3)
        {
            throw new ArgumentException("dirIsNeg must have a length of 3");
        }

        float xMin, xMax, yMin, yMax, zMin, zMax;

        xMin = (bounds.Min.X - ray.Origin.X) * ray.InverseDirection.X;
        xMax = (bounds.Max.X - ray.Origin.X) * ray.InverseDirection.X;

        if (!dirIsNeg[0])
        {
            (xMax, xMin) = (xMin, xMax);
        }

        yMin = (bounds.Min.Y - ray.Origin.Y) * ray.InverseDirection.Y;
        yMax = (bounds.Max.Y - ray.Origin.Y) * ray.InverseDirection.Y;

        if (!dirIsNeg[1])
        {
            (yMax, yMin) = (yMin, yMax);
        }

        zMin = (bounds.Min.Z - ray.Origin.Z) * ray.InverseDirection.Z;
        zMax = (bounds.Max.Z - ray.Origin.Z) * ray.InverseDirection.Z;

        if (!dirIsNeg[2])
        {
            (zMax, zMin) = (zMin, zMax);
        }

        float tMin = Math.Max(xMin, Math.Max(yMin, zMin));
        float tMax = Math.Min(xMax, Math.Min(yMax, zMax));

        return tMin <= tMax;
    }
}
