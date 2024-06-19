using Maths;

namespace PA5;

internal struct Material
{
    public MaterialType MaterialType;

    public float Ior;

    public float Kd;

    public float Ks;

    public Vector3d DiffuseColor;

    public float SpecularExponent;
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
