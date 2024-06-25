using Maths;

namespace PA6;

internal abstract class Geometry : IDisposable
{
    protected Geometry()
    {
        Handle = new(this);
    }

    private Material material = new();

    public Handle<Geometry> Handle { get; }

    public ref Material Material => ref material;

    public abstract Intersection GetIntersection(Ray ray);

    public abstract Vector3d EvalDiffuseColor(Intersection intersection);

    public abstract Bounds3d GetBounds();

    public virtual void Dispose()
    {
        Handle.Dispose();

        GC.SuppressFinalize(this);
    }
}
