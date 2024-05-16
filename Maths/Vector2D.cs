using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2D(double x, double y) : IEquatable<Vector2D>
{
    public double X = x;

    public double Y = y;

    public readonly double LengthSquared => X * X + Y * Y;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public readonly bool Equals(Vector2D other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Vector2D d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y})";
    }

    public static Vector2D operator +(Vector2D left, Vector2D right)
    {
        return new(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2D operator -(Vector2D left, Vector2D right)
    {
        return new(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2D operator *(Vector2D vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar);
    }

    public static Vector2D operator /(Vector2D vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar);
    }

    public static bool operator ==(Vector2D left, Vector2D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2D left, Vector2D right)
    {
        return !(left == right);
    }

    public static double Dot(Vector2D a, Vector2D b)
    {
        return (a.X * b.X) + (a.Y * b.Y);
    }

    public static double Cross(Vector2D a, Vector2D b)
    {
        return (a.X * b.Y) - (a.Y * b.X);
    }

    public static Vector2D Normalize(Vector2D a)
    {
        return a / a.Length;
    }
}
