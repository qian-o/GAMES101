using Maths;

namespace PA.Graphics;

public struct Vertex(Vector3d position, Vector4d color = default, Vector2d texCoord = default, Vector3d normal = default)
{
    public Vector3d Position = position;

    public Vector4d Color = color;

    public Vector2d TexCoord = texCoord;

    public Vector3d Normal = normal;

    public static Vertex Interpolate(Vertex v1, Vertex v2, Vertex v3, Vector3d barycentricCoords)
    {
        Vertex result;

        result.Position = v1.Position * barycentricCoords.X + v2.Position * barycentricCoords.Y + v3.Position * barycentricCoords.Z;
        result.Color = v1.Color * barycentricCoords.X + v2.Color * barycentricCoords.Y + v3.Color * barycentricCoords.Z;
        result.TexCoord = v1.TexCoord * barycentricCoords.X + v2.TexCoord * barycentricCoords.Y + v3.TexCoord * barycentricCoords.Z;
        result.Normal = v1.Normal * barycentricCoords.X + v2.Normal * barycentricCoords.Y + v3.Normal * barycentricCoords.Z;

        return result;
    }
}
