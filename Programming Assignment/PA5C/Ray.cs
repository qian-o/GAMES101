using Maths;

namespace PA5;

internal struct Ray(Vector3d origin, Vector3d direction)
{
    public Vector3d Origin = origin;

    public Vector3d Direction = direction;

    public readonly Vector3d At(float t) => Origin + (t * Direction);
}
