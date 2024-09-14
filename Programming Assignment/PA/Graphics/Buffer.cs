using System.Runtime.InteropServices;

namespace PA.Graphics;

public readonly unsafe struct Buffer<T>(int size) : IDisposable where T : unmanaged
{
    public ref T this[int index] => ref Data[index];

    public int Size { get; } = size;

    public T* Data { get; } = (T*)Marshal.AllocHGlobal(size * sizeof(T));

    public void Fill(T value)
    {
        new Span<T>(Data, Size).Fill(value);
    }

    public void CopyTo(Buffer<T> buffer)
    {
        new Span<T>(Data, Size).CopyTo(new Span<T>(buffer.Data, buffer.Size));
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal((nint)Data);
    }
}
