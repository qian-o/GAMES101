using Maths;

namespace PA5;

internal struct Intersection(float tNear, Vector2d uv = default)
{
    public bool Hit = true;

    public float TNear = tNear;

    public Vector2d UV = uv;

    public static Intersection False => new() { Hit = false, TNear = float.MaxValue };
}
