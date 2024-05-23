namespace Maths;

public static class MathsHelper
{
    public static float Lerp(float a, float b, float t)
    {
        return a + ((b - a) * t);
    }

    public static double Lerp(double a, double b, double t)
    {
        return a + ((b - a) * t);
    }
}
