using System.Numerics;

namespace Maths;

public static class MathsExtensions
{
    public static Vector2d XY(this Vector3d vector)
    {
        return new(vector.X, vector.Y);
    }

    public static Vector4d XYZW(this Vector2d vector, double z = 0, double w = 1)
    {
        return new(vector.X, vector.Y, z, w);
    }

    public static Vector2 ToSystem(this Vector2d vector)
    {
        return new((float)vector.X, (float)vector.Y);
    }

    public static Vector2d ToMaths(this Vector2 vector)
    {
        return new(vector.X, vector.Y);
    }

    public static Vector3 ToSystem(this Vector3d vector)
    {
        return new((float)vector.X, (float)vector.Y, (float)vector.Z);
    }

    public static Vector3d ToMaths(this Vector3 vector)
    {
        return new(vector.X, vector.Y, vector.Z);
    }

    public static Vector4 ToSystem(this Vector4d vector)
    {
        return new((float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W);
    }

    public static Vector4d ToMaths(this Vector4 vector)
    {
        return new(vector.X, vector.Y, vector.Z, vector.W);
    }
}
