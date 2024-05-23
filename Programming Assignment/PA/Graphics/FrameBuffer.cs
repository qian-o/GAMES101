using Silk.NET.Maths;
using Silk.NET.SDL;

namespace PA.Graphics;

public unsafe class FrameBuffer(Sdl sdl, Renderer* renderer, int width, int height) : IDisposable
{
    private readonly Sdl _sdl = sdl;
    private readonly Renderer* _renderer = renderer;
    private readonly Texture* _texture = sdl.CreateTexture(renderer, (int)PixelFormatEnum.Abgr32, (int)TextureAccess.Streaming, width, height);
    private readonly FixedArray<Vector4D<byte>> _pixels = new(width * height);
    private readonly FixedArray<double> _depths = new(width * height);

    public int Width { get; } = width;

    public int Height { get; } = height;

    public int Pitch { get; } = width * sizeof(Vector4D<byte>);

    public void Clear(Color color)
    {
        _pixels.Fill(RgbaToAbgr(color));
        _depths.Fill(double.MinValue);
    }

    public Color GetColor(int x, int y)
    {
        return AbgrToRgba(_pixels[GetIndex(x, y)]);
    }

    public void SetColor(int x, int y, Color color)
    {
        _pixels[GetIndex(x, y)] = RgbaToAbgr(color);
    }

    public double GetDepth(int x, int y)
    {
        return _depths[GetIndex(x, y)];
    }

    public void SetDepth(int x, int y, double depth)
    {
        _depths[GetIndex(x, y)] = depth;
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

    private int GetIndex(int x, int y)
    {
        return y * Width + x;
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
