using Maths;
using Silk.NET.Assimp;
using AssimpMaterial = Silk.NET.Assimp.Material;

namespace PA6;

internal enum MaterialType
{
    DiffuseAndGlossy,
    ReflectionAndRefraction,
    Reflection
}

internal unsafe struct Material
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

    public Material(AssimpMaterial* material)
    {
        for (uint i = 0; i < material->MNumProperties; i++)
        {
            MaterialProperty* property = material->MProperties[i];

            if (property->MKey.AsString == Assimp.MaterialColorDiffuseBase)
            {
                Color = GetVector4d(property->MData).XYZ();
            }
        }
    }

    private readonly Vector4d GetVector4d(byte* data)
    {
        return new(((float*)data)[0], ((float*)data)[1], ((float*)data)[2], ((float*)data)[3]);
    }
}
