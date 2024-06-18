using System.Runtime.InteropServices;

namespace PA.Graphics;

public readonly unsafe struct Buffer<T>(int size) : IDisposable where T : unmanaged
{
    private readonly int _size = size;
    private readonly T* _data = (T*)Marshal.AllocHGlobal(size * sizeof(T));

    public ref T this[int index] => ref _data[index];

    public int Size => _size;

    public T* Data => _data;

    public void Fill(T value)
    {
        new Span<T>(_data, _size).Fill(value);
    }

    public void CopyTo(Buffer<T> buffer)
    {
        new Span<T>(_data, _size).CopyTo(new Span<T>(buffer._data, buffer._size));
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal((nint)_data);
    }
}
