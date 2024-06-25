using Maths;

namespace PA6;

internal struct Intersection
{
    public bool Happened;

    public float Distance;

    public Handle<Geometry> Geometry;

    public Vector3d Position;

    public Vector3d Normal;

    public Vector3d BarycentricCoords;

    public Vector2d TexCoord;
}
