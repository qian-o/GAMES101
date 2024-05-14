using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2D(double x, double y)
{
    public double X = x;

    public double Y = y;

    public readonly double LengthSquared => X * X + Y * Y;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public static Vector2D operator +(Vector2D a, Vector2D b)
    {
        return new(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2D operator -(Vector2D a, Vector2D b)
    {
        return new(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2D operator *(Vector2D vector, Matrix3X2 matrix)
    {
        double x = Dot(vector, new Vector2D(matrix.M11, matrix.M21)) + matrix.M31;
        double y = Dot(vector, new Vector2D(matrix.M12, matrix.M22)) + matrix.M32;

        return new(x, y);
    }

    public static Vector2D operator *(Vector2D a, double scalar)
    {
        return new(a.X * scalar, a.Y * scalar);
    }

    public static Vector2D operator *(double scalar, Vector2D vector)
    {
        return vector * scalar;
    }

    public static Vector2D operator /(Vector2D vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar);
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

    public override readonly string ToString()
    {
        return $"({X}, {Y})";
    }
}
