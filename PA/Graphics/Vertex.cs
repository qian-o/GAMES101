using Maths;

namespace PA.Graphics;

public struct Vertex(Vector3d position, Vector3d color = default, Vector2d texCoord = default, Vector3d normal = default)
{
    public Vector3d Position = position;

    public Vector3d Color = color;

    public Vector2d TexCoord = texCoord;

    public Vector3d Normal = normal;
}
