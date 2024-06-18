using System.Runtime.InteropServices;
using Maths;

namespace PA.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct Fragment(Vector4d color, float depth)
{
    public Vector4d Color = color;

    public float Depth = depth;

    public Fragment(Vector4d color) : this(color, default)
    {
    }

    public Fragment(float depth) : this(default, depth)
    {
    }

    public static Fragment operator +(Fragment a, Fragment b)
    {
        return new Fragment(a.Color + b.Color, a.Depth + b.Depth);
    }

    public static Fragment operator -(Fragment a, Fragment b)
    {
        return new Fragment(a.Color - b.Color, a.Depth - b.Depth);
    }

    public static Fragment operator *(Fragment a, float b)
    {
        return new Fragment(a.Color * b, a.Depth * b);
    }

    public static Fragment operator /(Fragment a, float b)
    {
        return new Fragment(a.Color / b, a.Depth / b);
    }
}
