using Maths;

namespace PA.Graphics;

public struct Vertex(Vector3d position, Color color = default, Vector2d texCoord = default, Vector3d normal = default)
{
    public Vector3d Position = position;

    public Color Color = color;

    public Vector2d TexCoord = texCoord;

    public Vector3d Normal = normal;
}
