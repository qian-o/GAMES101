using Maths;

namespace PA7;

internal abstract class Shape : IDisposable
{
    protected Shape()
    {
        Handle = new(this);
    }

    public Handle<Shape> Handle { get; }

    public abstract bool Intersect(ref readonly Ray ray, ref float tnear, ref uint index);

    public abstract Intersection GetIntersection(ref readonly Ray ray);

    public abstract void GetSufaceProperties(ref readonly Vector3d position, ref readonly Vector2d uv, ref readonly uint index, ref Vector3d normal, ref Vector2d st);

    public abstract Vector3d EvalDiffuseColor(ref readonly Vector2d st);

    public abstract Bounds3d GetBounds();

    public abstract float GetArea();

    public abstract void Sample(ref Intersection pos, ref float pdf);

    public abstract bool HasEmit();

    public virtual void Dispose()
    {
        Handle.Dispose();

        GC.SuppressFinalize(this);
    }
}
