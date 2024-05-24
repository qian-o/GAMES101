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

    public static Color FromRgb(uint r, uint g, uint b)
    {
        return new(Clamp(r), Clamp(g), Clamp(b));
    }

    public static Color FromRgba(uint r, uint g, uint b, uint a)
    {
        return new(Clamp(r), Clamp(g), Clamp(b), Clamp(a));
    }

    private static byte Clamp(uint value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }
}