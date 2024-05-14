using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
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

    public static Vector3D operator *(Vector3D vector, Matrix4X4 matrix)
    {
        double x = Dot(vector, new(matrix.M11, matrix.M12, matrix.M13)) + matrix.M14;
        double y = Dot(vector, new(matrix.M21, matrix.M22, matrix.M23)) + matrix.M24;
        double z = Dot(vector, new(matrix.M31, matrix.M32, matrix.M33)) + matrix.M34;

        return new(x, y, z);
    }

    public static Vector3D operator *(Vector3D vector, double scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    public static Vector3D operator *(double scalar, Vector3D vector)
    {
        return vector * scalar;
    }

    public static Vector3D operator /(Vector3D vector, double scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
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

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }
}
