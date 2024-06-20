using Maths;

namespace PA5;

public struct Camera : IEquatable<Camera>
{
    public Vector3d Position = Vector3d.UnitZ;

    public Vector3d Target = Vector3d.Zero;

    public Vector3d Up = Vector3d.UnitY;

    public Camera()
    {
    }

    public readonly bool Equals(Camera other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Camera d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Position, Target, Up);
    }

    public static bool operator ==(Camera left, Camera right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Camera left, Camera right)
    {
        return !(left == right);
    }
}
