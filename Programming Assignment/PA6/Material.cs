using Maths;

namespace PA6;

internal enum MaterialType
{
    DiffuseAndGlossy,
    ReflectionAndRefraction,
    Reflection
}

internal struct Material
{
    public MaterialType Type;

    public Vector3d Color;

    public Vector3d Emission;

    public float Ior;

    public float Kd;

    public float Ks;

    public float SpecularExponent;
}
