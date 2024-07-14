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
        return ((value - min) / (max - min) * (newMax - newMin)) + newMin;
    }

    public static float Clamp(float value, float min, float max)
    {
        return MathF.Max(min, MathF.Min(value, max));
    }

    public static float Fresnel(Vector3d i, Vector3d n, float ior)
    {
        float cosi = Clamp(Vector3d.Dot(i, n), -1.0f, 1.0f);
        float etai = 1.0f;
        float etat = ior;

        if (cosi > 0)
        {
            (etat, etai) = (etai, etat);
        }

        float sint = etai / etat * MathF.Sqrt(MathF.Max(0.0f, 1.0f - (cosi * cosi)));

        if (sint >= 1.0f)
        {
            return 1.0f;
        }

        float cost = MathF.Sqrt(MathF.Max(0.0f, 1.0f - (sint * sint)));
        cosi = MathF.Abs(cosi);
        float Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
        float Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));

        return ((Rs * Rs) + (Rp * Rp)) / 2.0f;
    }
}
