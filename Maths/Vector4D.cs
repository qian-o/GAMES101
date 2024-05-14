namespace Maths;

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

    public static Vector4D operator *(Vector4D a, double scalar)
    {
        return new(a.X * scalar, a.Y * scalar, a.Z * scalar, a.W * scalar);
    }

    public static Vector4D operator *(double scalar, Vector4D a)
    {
        return a * scalar;
    }

    public static Vector4D operator /(Vector4D a, double scalar)
    {
        return new(a.X / scalar, a.Y / scalar, a.Z / scalar, a.W / scalar);
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
