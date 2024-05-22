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

    public Color this[int x, int y]
    {
        get => AbgrToRgba(_pixels[y * Width + x]);
        set => _pixels[y * Width + x] = RgbaToAbgr(value);
    }

    public void Clear(Color color)
    {
        _pixels.Fill(RgbaToAbgr(color));
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

    private static Color AbgrToRgba(Vector4D<byte> color)
    {
        return new(color.W, color.Z, color.Y, color.X);
    }

    private static Vector4D<byte> RgbaToAbgr(Color color)
    {
        return new(color.A, color.B, color.G, color.R);
    }
}
