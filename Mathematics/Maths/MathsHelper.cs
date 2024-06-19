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

    public static bool SolveQuadratic(float a, float b, float c, out float x0, out float x1)
    {
        float discr = b * b - 4 * a * c;
        if (discr < 0)
        {
            x0 = x1 = 0;

            return false;
        }
        else if (discr == 0)
        {
            x0 = x1 = -0.5f * b / a;
        }
        else
        {
            float q = (b > 0) ? -0.5f * (b + MathF.Sqrt(discr)) : -0.5f * (b - MathF.Sqrt(discr));
            x0 = q / a;
            x1 = c / q;
        }

        if (x0 > x1)
        {
            (x1, x0) = (x0, x1);
        }

        return true;
    }
}
