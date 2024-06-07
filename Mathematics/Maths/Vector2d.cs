using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2d(double x, double y) : IEquatable<Vector2d>
{
    public double X = x;

    public double Y = y;

    public readonly double LengthSquared => X * X + Y * Y;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public readonly bool Equals(Vector2d other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Vector2d d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y})";
    }

    public static Vector2d operator +(Vector2d left, Vector2d right)
    {
        return new(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2d operator -(Vector2d vector)
    {
        return new(-vector.X, -vector.Y);
    }

    public static Vector2d operator -(Vector2d left, Vector2d right)
    {
        return new(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2d operator *(Vector2d vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar);
    }

    public static Vector2d operator *(Vector2d left, Vector2d right)
    {
        return new(left.X * right.X, left.Y * right.Y);
    }

    public static Vector2d operator /(Vector2d vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar);
    }

    public static Vector2d operator /(Vector2d left, Vector2d right)
    {
        return new(left.X / right.X, left.Y / right.Y);
    }

    public static bool operator ==(Vector2d left, Vector2d right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2d left, Vector2d right)
    {
        return !(left == right);
    }

    public static double Dot(Vector2d a, Vector2d b)
    {
        return (a.X * b.X) + (a.Y * b.Y);
    }

    public static double Cross(Vector2d a, Vector2d b)
    {
        return (a.X * b.Y) - (a.Y * b.X);
    }

    public static Vector2d Normalize(Vector2d a)
    {
        return a / a.Length;
    }

    public static Vector2d Pow(Vector2d a, double b)
    {
        return new(Math.Pow(a.X, b), Math.Pow(a.Y, b));
    }
}
