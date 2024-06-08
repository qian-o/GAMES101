using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector3d(double x, double y, double z) : IEquatable<Vector3d>
{
    public double X = x;

    public double Y = y;

    public double Z = z;

    public Vector3d(Vector2d vector, double z) : this(vector.X, vector.Y, z)
    {
    }

    public readonly double LengthSquared => X * X + Y * Y + Z * Z;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public readonly bool Equals(Vector3d other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Vector3d d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public static Vector3d operator +(Vector3d vector, double scalar)
    {
        return new(vector.X + scalar, vector.Y + scalar, vector.Z + scalar);
    }

    public static Vector3d operator +(Vector3d left, Vector3d right)
    {
        return new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vector3d operator -(Vector3d vector)
    {
        return new(-vector.X, -vector.Y, -vector.Z);
    }

    public static Vector3d operator -(Vector3d left, Vector3d right)
    {
        return new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vector3d operator *(Vector3d vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    public static Vector3d operator *(Vector3d left, Vector3d right)
    {
        return new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }

    public static Vector3d operator /(Vector3d vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    }

    public static Vector3d operator /(Vector3d left, Vector3d right)
    {
        return new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
    }

    public static bool operator ==(Vector3d left, Vector3d right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3d left, Vector3d right)
    {
        return !(left == right);
    }

    public static double Dot(Vector3d a, Vector3d b)
    {
        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
    }

    public static Vector3d Cross(Vector3d a, Vector3d b)
    {
        return new((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
    }

    public static Vector3d Normalize(Vector3d a)
    {
        return a / a.Length;
    }

    public static Vector3d Pow(Vector3d a, double b)
    {
        return new(Math.Pow(a.X, b), Math.Pow(a.Y, b), Math.Pow(a.Z, b));
    }
}
