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

    public static Vector2d Lerp(Vector2d a, Vector2d b, double t)
    {
        return a + ((b - a) * t);
    }

    public static Vector3d Lerp(Vector3d a, Vector3d b, double t)
    {
        return a + ((b - a) * t);
    }

    public static Vector4d Lerp(Vector4d a, Vector4d b, double t)
    {
        return a + ((b - a) * t);
    }
}
