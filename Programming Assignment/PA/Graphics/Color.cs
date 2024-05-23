namespace PA.Graphics;

public struct Color(byte r, byte g, byte b, byte a)
{
    public byte R = r;

    public byte G = g;

    public byte B = b;

    public byte A = a;

    public Color(byte r, byte g, byte b) : this(r, g, b, 255)
    {
    }

    public Color(float r, float g, float b, float a) : this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255))
    {
    }

    public Color(float r, float g, float b) : this(r, g, b, 1.0f)
    {
    }

    public Color(double r, double g, double b, double a) : this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255))
    {
    }

    public Color(double r, double g, double b) : this(r, g, b, 1.0)
    {
    }

    public readonly float Rf => R / 255.0f;

    public readonly float Gf => G / 255.0f;

    public readonly float Bf => B / 255.0f;

    public readonly float Af => A / 255.0f;

    public readonly double Rd => R / 255.0;

    public readonly double Gd => G / 255.0;

    public readonly double Bd => B / 255.0;

    public readonly double Ad => A / 255.0;

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
}
