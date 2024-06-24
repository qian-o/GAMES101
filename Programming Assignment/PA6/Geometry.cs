namespace PA6;

internal abstract class Geometry
{
    protected Geometry()
    {
        Handle = new(this);
        Material = new();
    }

    public Handle<Geometry> Handle { get; }

    public Material Material { get; }
}
