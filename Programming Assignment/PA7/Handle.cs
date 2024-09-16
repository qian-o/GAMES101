using System.Runtime.InteropServices;

namespace PA7;

internal struct Handle<TClass>(TClass @class) : IDisposable where TClass : class
{
    public GCHandle GC = GCHandle.Alloc(@class);

    public TClass Target => (TClass)GC.Target!;

    public void Dispose()
    {
        if (GC.IsAllocated)
        {
            GC.Free();
        }
    }
}
