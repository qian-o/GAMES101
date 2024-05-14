namespace Maths;

public struct Vector3D(double x, double y, double z)
{
    public double X = x;

    public double Y = y;

    public double Z = z;

    public readonly double LengthSquared => X * X + Y * Y + Z * Z;

    public readonly double Length => Math.Sqrt(LengthSquared);

    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
        return new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3D operator -(Vector3D a, Vector3D b)
    {
        return new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3D operator *(Vector3D a, double scalar)
    {
        return new(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static Vector3D operator *(double scalar, Vector3D a)
    {
        return a * scalar;
    }

    public static Vector3D operator /(Vector3D a, double scalar)
    {
        return new(a.X / scalar, a.Y / scalar, a.Z / scalar);
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
