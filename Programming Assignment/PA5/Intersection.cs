using Maths;

namespace PA5;

internal struct Intersection(Geometry geometry, Material materia, float tNear, Vector2d uv)
{
    public Geometry Geometry = geometry;

    public Material Material = materia;

    public float TNear = tNear;

    public Vector2d UV = uv;

    public static Intersection False => new() { Geometry = default, Material = default, TNear = float.MaxValue, UV = Vector2d.Zero };
}
