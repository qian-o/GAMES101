using Maths;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace PA.Graphics;

public unsafe class FrameBuffer : IDisposable
{
    private readonly Sdl _sdl;
    private readonly Renderer* _renderer;
    private readonly int _width;
    private readonly int _height;
    private readonly int _sampleCount;
    private readonly int _pitch;
    private readonly Texture* _texture;
    private readonly Vector2d[] _pattern;
    private readonly Pixel[] _pixels;
    private readonly FixedArray<Color>[] _colorBuffer;
    private readonly FixedArray<double>[] _depthBuffer;
    private readonly FixedArray<Color> _finalColorBuffer;
    private readonly FixedArray<double> _finalDepthBuffer;

    public FrameBuffer(Sdl sdl, Renderer* renderer, int width, int height, SampleCount sampleCount = SampleCount.SampleCount1)
    {
        _sdl = sdl;
        _renderer = renderer;
        _width = width;
        _height = height;
        _sampleCount = (int)sampleCount;
        _pitch = width * sizeof(Color);
        _texture = sdl.CreateTexture(renderer, (int)PixelFormatEnum.Abgr32, (int)TextureAccess.Streaming, width, height);
        _pattern = sampleCount switch
        {
            SampleCount.SampleCount1 => [new(0.5, 0.5)],
            SampleCount.SampleCount2 =>
            [
                new(0.75, 0.75),
                new(0.25, 0.25)
            ],
            SampleCount.SampleCount4 =>
            [
                new(0.375, 0.125),
                new(0.875, 0.375),
                new(0.125, 0.625),
                new(0.625, 0.875)
            ],
            SampleCount.SampleCount8 =>
            [
                new(0.5625, 0.3125),
                new(0.4375, 0.6875),
                new(0.8125, 0.5625),
                new(0.3125, 0.1875),
                new(0.1875, 0.8125),
                new(0.0625, 0.4375),
                new(0.6875, 0.9375),
                new(0.9375, 0.0625)
            ],
            SampleCount.SampleCount16 =>
            [
                new(0.5625, 0.5625),
                new(0.4375, 0.3125),
                new(0.3125, 0.625),
                new(0.75, 0.4375),
                new(0.1875, 0.375),
                new(0.625, 0.8125),
                new(0.8125, 0.6875),
                new(0.6875, 0.1875),
                new(0.375, 0.875),
                new(0.5, 0.0625),
                new(0.25, 0.125),
                new(0.125, 0.75),
                new(0.0, 0.5),
                new(0.9375, 0.25),
                new(0.875, 0.9375),
                new(0.0625, 0.0)
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount)),
        };

        _pixels = new Pixel[width * height];
        for (int i = 0; i < _pixels.Length; i++)
        {
            int x = i % width;
            int y = i / width;

            _pixels[i] = new Pixel(x, y);
        }

        _colorBuffer = new FixedArray<Color>[_sampleCount];
        _depthBuffer = new FixedArray<double>[_sampleCount];
        for (int i = 0; i < _sampleCount; i++)
        {
            _colorBuffer[i] = new FixedArray<Color>(width * height);
            _depthBuffer[i] = new FixedArray<double>(width * height);
        }

        _finalColorBuffer = new FixedArray<Color>(width * height);
        _finalDepthBuffer = new FixedArray<double>(width * height);

        Pattern = [.. _pattern];
        Pixels = [.. _pixels];
    }

    public Vector2d[] Pattern { get; }

    public Pixel[] Pixels { get; }

    public void Clear(Color color)
    {
        for (int i = 0; i < _sampleCount; i++)
        {
            _colorBuffer[i].Fill(color);
            _depthBuffer[i].Fill(double.MinValue);
        }

        _finalColorBuffer.Fill(color);
    }

    public Color GetColor(Pixel pixel, int index)
    {
        return _colorBuffer[index][GetIndex(pixel)];
    }

    public double GetDepth(Pixel pixel, int index)
    {
        return _depthBuffer[index][GetIndex(pixel)];
    }

    public void SetColor(Pixel pixel, int index, Color color)
    {
        _colorBuffer[index][GetIndex(pixel)] = color;
    }

    public void SetDepth(Pixel pixel, int index, double depth)
    {
        _depthBuffer[index][GetIndex(pixel)] = depth;
    }

    public Color GetFinalColor(Pixel pixel)
    {
        return _finalColorBuffer[GetIndex(pixel)];
    }

    public double GetFinalDepth(Pixel pixel)
    {
        return _finalDepthBuffer[GetIndex(pixel)];
    }

    public void SetFinalColor(Pixel pixel, Color color)
    {
        _finalColorBuffer[GetIndex(pixel)] = color;
    }

    public void SetFinalDepth(Pixel pixel, double depth)
    {
        _finalDepthBuffer[GetIndex(pixel)] = depth;
    }

    public void Present(int x, int y, bool flipY)
    {
        Rectangle<int> dst = new(x, y, _width, _height);
        RendererFlip flip = flipY ? RendererFlip.Vertical : RendererFlip.None;

        if (_sampleCount == 1)
        {
            _colorBuffer[0].Copy(_finalColorBuffer);
            _depthBuffer[0].Copy(_finalDepthBuffer);
        }
        else
        {
            Parallel.ForEach(_pixels, (pixel) =>
            {
                Color color = GetFinalColor(pixel);
                double depth = GetFinalDepth(pixel);

                for (int i = 0; i < _sampleCount; i++)
                {
                    color += GetColor(pixel, i) / _sampleCount;
                    depth = Math.Max(depth, GetDepth(pixel, i));
                }

                SetFinalColor(pixel, color);
                SetFinalDepth(pixel, depth);
            });
        }

        _sdl.UpdateTexture(_texture, null, _finalColorBuffer.Buffer, _pitch);
        _sdl.RenderCopyEx(_renderer, _texture, null, &dst, 0.0, null, flip);
    }

    public void Dispose()
    {
        _sdl.DestroyTexture(_texture);

        for (int i = 0; i < _sampleCount; i++)
        {
            _colorBuffer[i].Dispose();
            _depthBuffer[i].Dispose();
        }

        _finalColorBuffer.Dispose();
        _finalDepthBuffer.Dispose();

        GC.SuppressFinalize(this);
    }

    private int GetIndex(Pixel pixel)
    {
        return pixel.Y * _width + pixel.X;
    }
}
