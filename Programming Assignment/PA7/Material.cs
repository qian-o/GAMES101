using Maths;

namespace PA7;

enum MaterialType
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
}
