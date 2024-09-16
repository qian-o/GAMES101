using Maths;

namespace PA7;

internal struct Intersection
{
    public bool Happened;

    public Vector3d Coords;

    public Vector3d Normal;

    public Vector3d Emit;

    public double Distance = double.MaxValue;

    public Handle<Shape> Shape;

    public Handle<Material> Material;

    public Intersection()
    {
    }
}
