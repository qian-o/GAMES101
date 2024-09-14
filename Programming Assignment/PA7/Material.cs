using Maths;

namespace PA7;

internal enum MaterialType
{
    Diffuse
}

internal class Material(MaterialType type, Vector3d emission)
{
    public MaterialType Type { get; set; } = type;

    public Vector3d Emission { get; set; } = emission;

    public float Ior { get; set; }

    public float Kd { get; set; }

    public float Ks { get; set; }

    public float SpecularExponent { get; set; }

    public bool HasEmission => Emission.Length > 0.0f;

    public Vector3d Sample(Vector3d wi, Vector3d wo, Vector3d normal)
    {
        switch (Type)
        {
            case MaterialType.Diffuse:
                {
                    float xi1 = Random.Shared.NextSingle();
                    float xi2 = Random.Shared.NextSingle();
                    float z = MathF.Abs(1.0f - 2.0f * xi1);
                    float r = MathF.Sqrt(1.0f - z * z);
                    float phi = 2.0f * MathF.PI * xi2;

                    Vector3d local = new(r * MathF.Cos(phi), r * MathF.Sin(phi), z);

                    return ToWorld(local, normal);
                }
            default:
                return new(0.0f);
        }
    }

    public float Pdf(Vector3d wi, Vector3d wo, Vector3d normal)
    {
        return 0.0f;
    }

    public Vector3d Evaluate(Vector3d wi, Vector3d wo, Vector3d normal)
    {
        return new(0.0f);
    }

    private static Vector3d Reflect(Vector3d wi, Vector3d normal)
    {
        return wi - 2.0f * Vector3d.Dot(wi, normal) * normal;
    }

    private static Vector3d Refract(Vector3d wi, Vector3d normal, float ior)
    {
        float cosThetaI = Vector3d.Dot(wi, normal);
        float etaI = 1.0f;
        float etaT = ior;

        if (cosThetaI < 0.0f)
        {
            cosThetaI = -cosThetaI;
        }
        else
        {
            (etaT, etaI) = (etaI, etaT);
            normal = -normal;
        }

        float eta = etaI / etaT;
        float k = 1.0f - eta * eta * (1.0f - cosThetaI * cosThetaI);

        return k < 0.0f ? new(0.0f) : eta * wi + (eta * cosThetaI - MathF.Sqrt(k)) * normal;
    }

    private static float Fresnel(Vector3d wi, Vector3d normal, float ior)
    {
        float cosThetaI = Vector3d.Dot(wi, normal);
        float etaI = 1.0f;
        float etaT = ior;

        if (cosThetaI < 0.0f)
        {
            cosThetaI = -cosThetaI;
        }
        else
        {
            (etaT, etaI) = (etaI, etaT);
        }

        float eta = etaI / etaT;
        float sinThetaT = eta * MathF.Sqrt(MathF.Max(0.0f, 1.0f - cosThetaI * cosThetaI));

        if (sinThetaT >= 1.0f)
        {
            return 1.0f;
        }

        float cosThetaT = MathF.Sqrt(MathF.Max(0.0f, 1.0f - sinThetaT * sinThetaT));

        float rParallel = (etaT * cosThetaI - etaI * cosThetaT) / (etaT * cosThetaI + etaI * cosThetaT);
        float rPerpendicular = (etaI * cosThetaI - etaT * cosThetaT) / (etaI * cosThetaI + etaT * cosThetaT);

        return 0.5f * (rParallel * rParallel + rPerpendicular * rPerpendicular);
    }

    private static Vector3d ToWorld(Vector3d local, Vector3d normal)
    {
        Vector3d b, c;
        if (MathF.Abs(normal.X) > MathF.Abs(normal.Y))
        {
            float invLen = 1.0f / MathF.Sqrt(normal.X * normal.X + normal.Z * normal.Z);
            c = new(normal.Z * invLen, 0.0f, -normal.X * invLen);
        }
        else
        {
            float invLen = 1.0f / MathF.Sqrt(normal.Y * normal.Y + normal.Z * normal.Z);
            c = new(0.0f, normal.Z * invLen, -normal.Y * invLen);
        }

        b = Vector3d.Cross(c, normal);

        return local.X * b + local.Y * c + local.Z * normal;
    }
}
