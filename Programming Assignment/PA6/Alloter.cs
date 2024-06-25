using System.Runtime.InteropServices;

namespace PA6;

internal unsafe class Alloter<T> : IDisposable where T : unmanaged
{
    private readonly List<nint> _allocated = [];

    public T* Allocate()
    {
        T* ptr = (T*)Marshal.AllocHGlobal(sizeof(T));

        _allocated.Add((nint)ptr);

        return ptr;
    }

    public void Dispose()
    {
        foreach (nint ptr in _allocated)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
