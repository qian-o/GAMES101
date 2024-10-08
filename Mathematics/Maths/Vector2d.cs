﻿using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2d(float x, float y) : IEquatable<Vector2d>
{
    public static Vector2d Zero => new(0.0f);

    public static Vector2d One => new(1.0f);

    public static Vector2d UnitX => new(1.0f, 0.0f);

    public static Vector2d UnitY => new(0.0f, 1.0f);

    public float X = x;

    public float Y = y;

    public Vector2d(float value) : this(value, value)
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

    public readonly float LengthSquared => (X * X) + (Y * Y);

    public readonly float Length => MathF.Sqrt(LengthSquared);

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

    public static Vector2d operator *(Vector2d vector, float scalar)
    {
        return new(vector.X * scalar, vector.Y * scalar);
    }

    public static Vector2d operator *(float scalar, Vector2d vector)
    {
        return vector * scalar;
    }

    public static Vector2d operator *(Vector2d left, Vector2d right)
    {
        return new(left.X * right.X, left.Y * right.Y);
    }

    public static Vector2d operator /(Vector2d vector, float scalar)
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

    public static float Dot(Vector2d a, Vector2d b)
    {
        return (a.X * b.X) + (a.Y * b.Y);
    }

    public static float Cross(Vector2d a, Vector2d b)
    {
        return (a.X * b.Y) - (a.Y * b.X);
    }

    public static Vector2d Normalize(Vector2d a)
    {
        return a / a.Length;
    }

    public static Vector2d Pow(Vector2d a, float b)
    {
        return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b));
    }
}
