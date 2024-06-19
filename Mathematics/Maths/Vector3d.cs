using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector3d(float x, float y, float z) : IEquatable<Vector3d>
{
    public static Vector3d Zero => new(0.0f);

    public static Vector3d One => new(1.0f);

    public static Vector3d UnitX => new(1.0f, 0.0f, 0.0f);

    public static Vector3d UnitY => new(0.0f, 1.0f, 0.0f);

    public static Vector3d UnitZ => new(0.0f, 0.0f, 1.0f);

    public float X = x;

    public float Y = y;

    public float Z = z;

    public Vector3d(float value) : this(value, value, value)
    {
    }

    public Vector3d(Vector2d vector, float z) : this(vector.X, vector.Y, z)
    {
    }

    public unsafe float this[int index]
    {
        get
        {
            fixed (float* p = &X)
            {
                return *(p + index);
            }
        }
        set
        {
            fixed (float* p = &X)
            {
                *(p + index) = value;
            }
        }
    }

    public readonly float LengthSquared => X * X + Y * Y + Z * Z;

    public readonly float Length => MathF.Sqrt(LengthSquared);

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

    public static Vector3d operator +(Vector3d vector, float scalar)
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

    public static Vector3d operator *(Vector3d vector, float scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    public static Vector3d operator *(float scalar, Vector3d vector)
    {
        return vector * scalar;
    }

    public static Vector3d operator *(Vector3d left, Vector3d right)
    {
        return new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }

    public static Vector3d operator /(Vector3d vector, float scalar)
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

    public static float Dot(Vector3d a, Vector3d b)
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

    public static Vector3d Pow(Vector3d a, float b)
    {
        return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b), MathF.Pow(a.Z, b));
    }
}
