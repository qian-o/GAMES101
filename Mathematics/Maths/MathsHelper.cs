namespace Maths;

public static class MathsHelper
{
    public static float Lerp(float a, float b, float t)
    {
        return a + ((b - a) * t);
    }

    public static Vector2d Lerp(Vector2d a, Vector2d b, float t)
    {
        return a + ((b - a) * t);
    }

    public static Vector3d Lerp(Vector3d a, Vector3d b, float t)
    {
        return a + ((b - a) * t);
    }

    public static Vector4d Lerp(Vector4d a, Vector4d b, float t)
    {
        return a + ((b - a) * t);
    }

    public static float RangeMap(float value, float min, float max, float newMin, float newMax)
    {
        return ((value - min) / (max - min)) * (newMax - newMin) + newMin;
    }
}
