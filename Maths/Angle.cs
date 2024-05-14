namespace Maths;

public struct Angle(double radians)
{
    public double Radians = radians;

    public readonly double Degrees => Radians * 180 / Math.PI;

    public static Angle operator +(Angle a, Angle b)
    {
        return new(a.Radians + b.Radians);
    }

    public static Angle operator -(Angle a, Angle b)
    {
        return new(a.Radians - b.Radians);
    }

    public static Angle operator *(Angle angle, double scalar)
    {
        return new(angle.Radians * scalar);
    }

    public static Angle operator *(double scalar, Angle angle)
    {
        return angle * scalar;
    }

    public static Angle operator /(Angle angle, double scalar)
    {
        return new(angle.Radians / scalar);
    }

    public static Angle FromRadians(double radians)
    {
        return new(radians);
    }

    public static Angle FromDegrees(double degrees)
    {
        return new(degrees * Math.PI / 180);
    }

    public override readonly string ToString()
    {
        return $"{Degrees}°";
    }
}
