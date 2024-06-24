using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Angle(float radians) : IEquatable<Angle>
{
    public float Radians = radians;

    public float Degrees
    {
        readonly get => Radians * 180.0f / MathF.PI;
        set => Radians = value * MathF.PI / 180.0f;
    }

    public readonly bool Equals(Angle other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Angle d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return Radians.GetHashCode();
    }

    public override readonly string ToString()
    {
        return $"{Degrees}°";
    }

    public static Angle operator +(Angle left, Angle right)
    {
        return new(left.Radians + right.Radians);
    }

    public static Angle operator -(Angle left, Angle right)
    {
        return new(left.Radians - right.Radians);
    }

    public static Angle operator *(Angle angle, float scalar)
    {
        return new(angle.Radians * scalar);
    }

    public static Angle operator /(Angle angle, float scalar)
    {
        return new(angle.Radians / scalar);
    }

    public static bool operator ==(Angle left, Angle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Angle left, Angle right)
    {
        return !(left == right);
    }

    public static Angle FromRadians(float radians)
    {
        return new(radians);
    }

    public static Angle FromDegrees(float degrees)
    {
        Angle angle = default;
        angle.Degrees = degrees;

        return angle;
    }
}
