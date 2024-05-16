namespace Maths;

public struct Angle(double radians) : IEquatable<Angle>
{
    public double Radians = radians;

    public readonly double Degrees => Radians * 180 / Math.PI;

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

    public static Angle operator *(Angle angle, double scalar)
    {
        return new(angle.Radians * scalar);
    }

    public static Angle operator /(Angle angle, double scalar)
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

    public static Angle FromRadians(double radians)
    {
        return new(radians);
    }

    public static Angle FromDegrees(double degrees)
    {
        return new(degrees * Math.PI / 180);
    }
}
