using Maths;

namespace PA7;

internal struct Ray(Vector3d origin, Vector3d direction, float t = 0.0f)
{
    public Vector3d Origin = origin;

    public Vector3d Direction = direction;

    public Vector3d InverseDirection = new(1.0f / direction.X, 1.0f / direction.Y, 1.0f / direction.Z);

    public float T = t;

    public float TMin = 0.0f;

    public float TMax = float.MaxValue;

    public readonly Vector3d PointAt(float t) => Origin + (Direction * t);
}
