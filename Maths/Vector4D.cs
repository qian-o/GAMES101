using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector4d(double x, double y, double z, double w) : IEquatable<Vector4d>
{
    public double X = x;

    public double Y = y;

    public double Z = z;

    public double W = w;

    public Vector4d(Vector2d vector, double z, double w) : this(vector.X, vector.Y, z, w)
    {
    }

    public Vector4d(Vector3d vector, double w) : this(vector.X, vector.Y, vector.Z, w)
    {
    }

    public readonly double LengthSquared => X * X + Y * Y + Z * Z + W * W;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public readonly bool Equals(Vector4d other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Vector4d d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }

    public static Vector4d operator +(Vector4d left, Vector4d right)
    {
        return new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    public static Vector4d operator -(Vector4d vector)
    {
        return new(-vector.X, -vector.Y, -vector.Z, -vector.W);
    }

    public static Vector4d operator -(Vector4d left, Vector4d right)
    {
        return new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    }

    public static Vector4d operator *(Vector4d vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar, vector.W * scalar);
    }

    public static Vector4d operator /(Vector4d vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar, vector.W / scalar);
    }

    public static bool operator ==(Vector4d left, Vector4d right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4d left, Vector4d right)
    {
        return !(left == right);
    }

    public static double Dot(Vector4d a, Vector4d b)
    {
        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z) + (a.W * b.W);
    }

    public static Vector4d Normalize(Vector4d a)
    {
        return a / a.Length;
    }
}
