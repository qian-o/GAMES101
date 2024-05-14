namespace Maths;

public struct Angle(double radians)
{
    public double Radians = radians;

    public readonly double Degrees => Radians * 180 / Math.PI;

    public static Angle operator +(Angle a, Angle b) => new(a.Radians + b.Radians);

    public static Angle operator -(Angle a, Angle b) => new(a.Radians - b.Radians);

    public static Angle operator *(Angle a, double b) => new(a.Radians * b);

    public static Angle operator /(Angle a, double b) => new(a.Radians / b);

    public static Angle ByRadians(double radians) => new(radians);

    public static Angle ByDegrees(double degrees) => new(degrees * Math.PI / 180);
}
