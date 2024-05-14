using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector4D(double x, double y, double z, double w)
{
    public double X = x;

    public double Y = y;

    public double Z = z;

    public double W = w;

    public readonly double LengthSquared => X * X + Y * Y + Z * Z + W * W;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public static Vector4D operator +(Vector4D a, Vector4D b)
    {
        return new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    }

    public static Vector4D operator -(Vector4D a, Vector4D b)
    {
        return new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    }

    public static Vector4D operator *(Vector4D vector, Matrix4X4 matrix)
    {
        double x = Dot(vector, matrix.Row1);
        double y = Dot(vector, matrix.Row2);
        double z = Dot(vector, matrix.Row3);
        double w = Dot(vector, matrix.Row4);

        return new(x, y, z, w);
    }

    public static Vector4D operator *(Vector4D vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar, vector.W * scalar);
    }

    public static Vector4D operator *(double scalar, Vector4D vector)
    {
        return vector * scalar;
    }

    public static Vector4D operator /(Vector4D vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar, vector.W / scalar);
    }

    public static double Dot(Vector4D a, Vector4D b)
    {
        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z) + (a.W * b.W);
    }

    public static Vector4D Normalize(Vector4D a)
    {
        return a / a.Length;
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }
}
