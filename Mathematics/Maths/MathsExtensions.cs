using System.Numerics;

namespace Maths;

public static class MathsExtensions
{
    public static Vector2d XY(this Vector3d vector)
    {
        return new(vector.X, vector.Y);
    }

    public static Vector3d XYZ(this Vector4d vector)
    {
        return new(vector.X, vector.Y, vector.Z);
    }

    public static Vector2 ToSystem(this Vector2d vector)
    {
        return new(vector.X, vector.Y);
    }

    public static Vector3 ToSystem(this Vector3d vector)
    {
        return new(vector.X, vector.Y, vector.Z);
    }

    public static Vector4 ToSystem(this Vector4d vector)
    {
        return new(vector.X, vector.Y, vector.Z, vector.W);
    }

    public static Vector2d ToMaths(this Vector2 vector)
    {
        return new(vector.X, vector.Y);
    }

    public static Vector3d ToMaths(this Vector3 vector)
    {
        return new(vector.X, vector.Y, vector.Z);
    }

    public static Vector4d ToMaths(this Vector4 vector)
    {
        return new(vector.X, vector.Y, vector.Z, vector.W);
    }
}
