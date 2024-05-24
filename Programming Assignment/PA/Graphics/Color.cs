using System.Runtime.InteropServices;

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

    public Color(float r, float g, float b, float a) : this(ClampToByte(r), ClampToByte(g), ClampToByte(b), ClampToByte(a))
    {
    }

    public Color(float r, float g, float b) : this(r, g, b, 1.0f)
    {
    }

    public Color(double r, double g, double b, double a) : this(ClampToByte(r), ClampToByte(g), ClampToByte(b), ClampToByte(a))
    {
    }

    public Color(double r, double g, double b) : this(r, g, b, 1.0)
    {
    }

    public readonly float Af => A / 255.0f;

    public readonly float Bf => B / 255.0f;

    public readonly float Gf => G / 255.0f;

    public readonly float Rf => R / 255.0f;

    public readonly double Ad => A / 255.0;

    public readonly double Bd => B / 255.0;

    public readonly double Gd => G / 255.0;

    public readonly double Rd => R / 255.0;

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
        return new(left.Rd + right.Rd, left.Gd + right.Gd, left.Bd + right.Bd, left.Ad + right.Ad);
    }

    public static Color operator -(Color left, Color right)
    {
        return new(left.Rd - right.Rd, left.Gd - right.Gd, left.Bd - right.Bd, left.Ad - right.Ad);
    }

    public static Color operator *(Color color, double scalar)
    {
        return new(color.Rd * scalar, color.Gd * scalar, color.Bd * scalar, color.Ad * scalar);
    }

    public static Color operator /(Color color, double scalar)
    {
        return new(color.Rd / scalar, color.Gd / scalar, color.Bd / scalar, color.Ad / scalar);
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
        return new((byte)(argb >> 16 & 0xFF), (byte)(argb >> 8 & 0xFF), (byte)(argb & 0xFF), (byte)(argb >> 24 & 0xFF));
    }

    public static Color FromRgb(byte r, byte g, byte b)
    {
        return new(r, g, b);
    }

    public static Color FromRgba(byte r, byte g, byte b, byte a)
    {
        return new(r, g, b, a);
    }

    public static Color FromRgb(float r, float g, float b)
    {
        return new(r, g, b);
    }

    public static Color FromRgba(float r, float g, float b, float a)
    {
        return new(r, g, b, a);
    }

    public static Color FromRgb(double r, double g, double b)
    {
        return new(r, g, b);
    }

    public static Color FromRgba(double r, double g, double b, double a)
    {
        return new(r, g, b, a);
    }

    private static byte ClampToByte(float value)
    {
        return (byte)Math.Max(0, Math.Min(255, value * 255));
    }

    private static byte ClampToByte(double value)
    {
        return (byte)Math.Max(0, Math.Min(255, value * 255));
    }
}
