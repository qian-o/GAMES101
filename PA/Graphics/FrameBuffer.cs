using Silk.NET.Maths;
using Silk.NET.SDL;

namespace PA.Graphics;

public unsafe class FrameBuffer(Sdl sdl, Renderer* renderer, int width, int height) : IDisposable
{
    private readonly Sdl _sdl = sdl;
    private readonly Renderer* _renderer = renderer;
    private readonly Texture* _texture = sdl.CreateTexture(renderer, (int)PixelFormatEnum.Abgr32, (int)TextureAccess.Streaming, width, height);
    private readonly FixedArray<Vector4D<byte>> _pixels = new(width * height);

    public int Width { get; } = width;

    public int Height { get; } = height;

    public int Pitch { get; } = width * sizeof(Vector4D<byte>);

    public Vector3D<byte> this[int x, int y]
    {
        get => Rgb24(_pixels[y * Width + x]);
        set => _pixels[y * Width + x] = Abgr32(value);
    }

    public void Clear(Vector3D<byte> color)
    {
        _pixels.Fill(Abgr32(color));
    }

    public void Present(int x, int y, bool flipY)
    {
        Rectangle<int> dst = new(x, y, Width, Height);
        RendererFlip flip = flipY ? RendererFlip.Vertical : RendererFlip.None;

        _sdl.UpdateTexture(_texture, null, _pixels.Buffer, Pitch);
        _sdl.RenderCopyEx(_renderer, _texture, null, &dst, 0.0, null, flip);
    }

    public void Dispose()
    {
        _sdl.DestroyTexture(_texture);

        _pixels.Dispose();

        GC.SuppressFinalize(this);
    }

    private static Vector3D<byte> Rgb24(Vector4D<byte> color)
    {
        return new Vector3D<byte>(color.Z, color.Y, color.X);
    }

    private static Vector4D<byte> Abgr32(Vector3D<byte> color)
    {
        return new Vector4D<byte>(255, color.Z, color.Y, color.X);
    }
}
