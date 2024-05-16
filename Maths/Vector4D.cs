using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector4D(double x, double y, double z, double w) : IEquatable<Vector4D>
{
    public double X = x;

    public double Y = y;

    public double Z = z;

    public double W = w;

    public readonly double LengthSquared => X * X + Y * Y + Z * Z + W * W;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public readonly bool Equals(Vector4D other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Vector4D d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }

    public static Vector4D operator +(Vector4D left, Vector4D right)
    {
        return new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    public static Vector4D operator -(Vector4D left, Vector4D right)
    {
        return new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    }

    public static Vector4D operator *(Vector4D vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar, vector.W * scalar);
    }

    public static Vector4D operator /(Vector4D vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar, vector.W / scalar);
    }

    public static bool operator ==(Vector4D left, Vector4D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4D left, Vector4D right)
    {
        return !(left == right);
    }

    public static double Dot(Vector4D a, Vector4D b)
    {
        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z) + (a.W * b.W);
    }

    public static Vector4D Normalize(Vector4D a)
    {
        return a / a.Length;
    }
}
