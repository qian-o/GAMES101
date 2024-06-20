using Maths;

namespace PA5;

internal struct SurfaceProperties(Vector3d position, Vector2d uv, Vector3d normal = default, Vector2d st = default)
{
    public Vector3d Position = position;
    public Vector2d UV = uv;
    public Vector3d Normal = normal;
    public Vector2d ST = st;
}
