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
    public MaterialType Type = MaterialType.DiffuseAndGlossy;

    public Vector3d Color = new(0.2f);

    public Vector3d Emission = new(0.0f);

    public float Ior = 1.3f;

    public float Kd = 0.8f;

    public float Ks = 0.2f;

    public float SpecularExponent = 25.0f;

    public Material()
    {
    }
}
