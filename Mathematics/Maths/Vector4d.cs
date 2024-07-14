using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector4d(float x, float y, float z, float w) : IEquatable<Vector4d>
{
    public static Vector4d Zero => new(0.0f);

    public static Vector4d One => new(1.0f);

    public static Vector4d UnitX => new(1.0f, 0.0f, 0.0f, 0.0f);

    public static Vector4d UnitY => new(0.0f, 1.0f, 0.0f, 0.0f);

    public static Vector4d UnitZ => new(0.0f, 0.0f, 1.0f, 0.0f);

    public static Vector4d UnitW => new(0.0f, 0.0f, 0.0f, 1.0f);

    public float X = x;

    public float Y = y;

    public float Z = z;

    public float W = w;

    public Vector4d(float value) : this(value, value, value, value)
    {
    }

    public Vector4d(Vector2d vector, float z, float w) : this(vector.X, vector.Y, z, w)
    {
    }

    public Vector4d(Vector3d vector, float w) : this(vector.X, vector.Y, vector.Z, w)
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

    public readonly float LengthSquared => (X * X) + (Y * Y) + (Z * Z) + (W * W);

    public readonly float Length => MathF.Sqrt(LengthSquared);

    public readonly bool Equals(Vector4d other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Vector4d d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }

    public static Vector4d operator +(Vector4d left, Vector4d right)
    {
        return new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    public static Vector4d operator -(Vector4d vector)
    {
        return new(-vector.X, -vector.Y, -vector.Z, -vector.W);
    }

    public static Vector4d operator -(Vector4d left, Vector4d right)
    {
        return new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    }

    public static Vector4d operator *(Vector4d vector, float scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar, vector.W * scalar);
    }

    public static Vector4d operator *(float scalar, Vector4d vector)
    {
        return vector * scalar;
    }

    public static Vector4d operator *(Vector4d left, Vector4d right)
    {
        return new(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
    }

    public static Vector4d operator /(Vector4d vector, float scalar)
    {
        return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar, vector.W / scalar);
    }

    public static Vector4d operator /(Vector4d left, Vector4d right)
    {
        return new(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
    }

    public static bool operator ==(Vector4d left, Vector4d right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4d left, Vector4d right)
    {
        return !(left == right);
    }

    public static float Dot(Vector4d a, Vector4d b)
    {
        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z) + (a.W * b.W);
    }

    public static Vector4d Normalize(Vector4d a)
    {
        return a / a.Length;
    }

    public static Vector4d Pow(Vector4d a, float b)
    {
        return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b), MathF.Pow(a.Z, b), MathF.Pow(a.W, b));
    }
}
