using Maths;

namespace PA1;

public struct Vertex(Vector3D position, Vector3D color = default, Vector2D texCoord = default, Vector3D normal = default)
{
    public Vector3D Position = position;

    public Vector3D Color = color;

    public Vector2D TexCoord = texCoord;

    public Vector3D Normal = normal;
}
