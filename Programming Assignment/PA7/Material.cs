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

    public bool HasEmission()
    {
        return Emission.Length > 0.0f;
    }

    public Vector3d Sample(Vector3d normal)
    {
        switch (Type)
        {
            case MaterialType.Diffuse:
                {
                    float xi1 = Random.Shared.NextSingle();
                    float xi2 = Random.Shared.NextSingle();
                    float z = MathF.Abs(1.0f - (2.0f * xi1));
                    float r = MathF.Sqrt(1.0f - (z * z));
                    float phi = 2.0f * MathF.PI * xi2;

                    Vector3d local = new(r * MathF.Cos(phi), r * MathF.Sin(phi), z);

                    return ToWorld(local, normal);
                }
            default:
                return new(0.0f);
        }
    }

    public float Pdf(Vector3d wo, Vector3d normal)
    {
        switch (Type)
        {
            case MaterialType.Diffuse:
                {
                    if (Vector3d.Dot(wo, normal) > 0.0f)
                    {
                        return 0.5f / MathF.PI;
                    }

                    return 0.0f;
                }
            default:
                return 0.0f;
        }
    }

    public Vector3d Evaluate(Vector3d wi, Vector3d normal)
    {
        switch (Type)
        {
            case MaterialType.Diffuse:
                {
                    float cosalpha = Vector3d.Dot(wi, normal);
                    if (cosalpha > 0.0f)
                    {
                        return new Vector3d(Kd / MathF.PI);
                    }

                    return new Vector3d(0.0f);
                }
            default:
                return new(0.0f);
        }
    }

    private static Vector3d ToWorld(Vector3d local, Vector3d normal)
    {
        Vector3d b, c;
        if (MathF.Abs(normal.X) > MathF.Abs(normal.Y))
        {
            float invLen = 1.0f / MathF.Sqrt((normal.X * normal.X) + (normal.Z * normal.Z));
            c = new(normal.Z * invLen, 0.0f, -normal.X * invLen);
        }
        else
        {
            float invLen = 1.0f / MathF.Sqrt((normal.Y * normal.Y) + (normal.Z * normal.Z));
            c = new(0.0f, normal.Z * invLen, -normal.Y * invLen);
        }

        b = Vector3d.Cross(c, normal);

        return (local.X * b) + (local.Y * c) + (local.Z * normal);
    }
}
