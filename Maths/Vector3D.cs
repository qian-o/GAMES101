using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector3D(double x, double y, double z) : IEquatable<Vector3D>
{
    public double X = x;

    public double Y = y;

    public double Z = z;

    public readonly double LengthSquared => X * X + Y * Y + Z * Z;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public readonly bool Equals(Vector3D other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Vector3D d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public static Vector3D operator +(Vector3D left, Vector3D right)
    {
        return new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vector3D operator -(Vector3D vector)
    {
        return new(-vector.X, -vector.Y, -vector.Z);
    }
    public static Vector3D operator -(Vector3D left, Vector3D right)
    {
        return new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vector3D operator *(Vector3D vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    public static Vector3D operator /(Vector3D vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    }

    public static bool operator ==(Vector3D left, Vector3D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3D left, Vector3D right)
    {
        return !(left == right);
    }

    public static double Dot(Vector3D a, Vector3D b)
    {
        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
    }

    public static Vector3D Cross(Vector3D a, Vector3D b)
    {
        return new((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
    }

    public static Vector3D Normalize(Vector3D a)
    {
        return a / a.Length;
    }
}
