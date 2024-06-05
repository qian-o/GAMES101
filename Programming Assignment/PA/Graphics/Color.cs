using System.Runtime.InteropServices;
using Maths;

namespace PA.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct Color(byte r, byte g, byte b, byte a) : IEquatable<Color>
{
    public byte A = a;

    public byte B = b;

    public byte G = g;

    public byte R = r;

    public Color(byte r, byte g, byte b) : this(r, g, b, 255)
    {
    }

    public readonly bool Equals(Color other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Color d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(A, B, G, R);
    }

    public override readonly string ToString()
    {
        return $"({R}, {G}, {B}, {A})";
    }

    public static Color operator +(Color left, Color right)
    {
        return new(Clamp(left.R + right.R), Clamp(left.G + right.G), Clamp(left.B + right.B), Clamp(left.A + right.A));
    }

    public static Color operator -(Color left, Color right)
    {
        return new(Clamp(left.R - right.R), Clamp(left.G - right.G), Clamp(left.B - right.B), Clamp(left.A - right.A));
    }

    public static Color operator *(Color color, double scalar)
    {
        return new(Clamp(color.R * scalar), Clamp(color.G * scalar), Clamp(color.B * scalar), Clamp(color.A * scalar));
    }

    public static Color operator /(Color color, double scalar)
    {
        return new(Clamp(color.R / scalar), Clamp(color.G / scalar), Clamp(color.B / scalar), Clamp(color.A / scalar));
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }

    public static Color FromUint(uint argb)
    {
        return FromRgba((byte)((argb & 0x00ff0000) >> 0x10), (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff), (byte)((argb & 0xff000000) >> 0x18));
    }

    public static Color FromInt(int argb)
    {
        return FromRgba((byte)((argb & 0x00ff0000) >> 0x10), (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff), (byte)((argb & 0xff000000) >> 0x18));
    }

    public static Color FromRgb(byte r, byte g, byte b)
    {
        return new(r, g, b);
    }

    public static Color FromRgba(byte r, byte g, byte b, byte a)
    {
        return new(r, g, b, a);
    }

    public static Color FromRgb(uint r, uint g, uint b)
    {
        return new(Clamp(r), Clamp(g), Clamp(b));
    }

    public static Color FromRgba(uint r, uint g, uint b, uint a)
    {
        return new(Clamp(r), Clamp(g), Clamp(b), Clamp(a));
    }

    public static Color FromColor(Vector4d vector)
    {
        return new(Clamp(vector.X * 255.0), Clamp(vector.Y * 255), Clamp(vector.Z * 255), Clamp(vector.W * 255));
    }

    private static byte Clamp(uint value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }

    private static byte Clamp(double value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }
}