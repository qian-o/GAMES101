using Maths;

namespace PA6;

internal abstract class Geometry : IDisposable
{
    protected Geometry()
    {
        Handle = new(this);
        Material = new();
    }

    public Handle<Geometry> Handle { get; }

    public Material Material { get; }

    public abstract Intersection GetIntersection(Ray ray);

    public abstract Vector3d EvalDiffuseColor(Intersection intersection);

    public abstract Bounds3d GetBounds();

    public virtual void Dispose()
    {
        Handle.Dispose();

        GC.SuppressFinalize(this);
    }
}
