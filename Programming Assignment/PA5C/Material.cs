using Maths;

namespace PA5;

internal struct Material
{
    public MaterialType MaterialType = MaterialType.DiffuseAndGlossy;

    public float Ior = 1.3f;

    public float Kd = 0.8f;

    public float Ks = 0.2f;

    public Vector3d DiffuseColor = new(0.2f);

    public float SpecularExponent = 25.0f;

    public Material()
    {
    }
}

internal enum MaterialType
{
    /// <summary>
    /// 漫反射和镜面反射。
    /// </summary>
    DiffuseAndGlossy,

    /// <summary>
    /// 反射和折射。
    /// </summary>
    ReflectAndRefract,

    /// <summary>
    /// 反射。
    /// </summary>
    Reflect
}
