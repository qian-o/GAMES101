using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
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
    private readonly uint _sampleCount;
    private readonly int _pitch;
    private readonly Texture* _texture;
    private readonly Vector2d[] _pattern;
    private readonly Pixel[] _pixels;
    private readonly Color[][] _colorBuffer;
    private readonly double[][] _depthBuffer;
    private readonly Color[] _finalColorBuffer;
    private readonly double[] _finalDepthBuffer;

    public FrameBuffer(Sdl sdl, Renderer* renderer, int width, int height, SampleCount sampleCount = SampleCount.SampleCount1)
    {
        _sdl = sdl;
        _renderer = renderer;
        _width = width;
        _height = height;
        _sampleCount = (uint)sampleCount;
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

        _colorBuffer = new Color[_sampleCount][];
        _depthBuffer = new double[_sampleCount][];
        for (int i = 0; i < _sampleCount; i++)
        {
            _colorBuffer[i] = new Color[width * height];
            _depthBuffer[i] = new double[width * height];
        }

        _finalColorBuffer = new Color[width * height];
        _finalDepthBuffer = new double[width * height];

        Pattern = [.. _pattern];
        Pixels = [.. _pixels];
    }

    public Vector2d[] Pattern { get; }

    public Pixel[] Pixels { get; }

    public void Clear(Color color)
    {
        for (int i = 0; i < _sampleCount; i++)
        {
            Array.Fill(_colorBuffer[i], color);
            Array.Fill(_depthBuffer[i], double.MinValue);
        }

        Array.Fill(_finalColorBuffer, color);
        Array.Fill(_finalDepthBuffer, double.MinValue);
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
        Rectangle<int> destination = new(x, y, _width, _height);
        RendererFlip flip = flipY ? RendererFlip.Vertical : RendererFlip.None;

        if (_sampleCount == 1)
        {
            Array.Copy(_colorBuffer[0], _finalColorBuffer, _finalColorBuffer.Length);
            Array.Copy(_depthBuffer[0], _finalDepthBuffer, _finalDepthBuffer.Length);
        }
        else
        {
            Parallel.ForEach(_pixels, (pixel) =>
            {
                uint r = 0;
                uint g = 0;
                uint b = 0;
                uint a = 0;
                double depth = double.MinValue;

                for (int i = 0; i < _sampleCount; i++)
                {
                    Color color = GetColor(pixel, i);
                    double d = GetDepth(pixel, i);

                    r += color.R;
                    g += color.G;
                    b += color.B;
                    a += color.A;
                    depth = Math.Max(depth, d);
                }

                r /= _sampleCount;
                g /= _sampleCount;
                b /= _sampleCount;
                a /= _sampleCount;

                SetFinalColor(pixel, Color.FromRgba(r, g, b, a));
                SetFinalDepth(pixel, depth);
            });
        }

        _sdl.UpdateTexture(_texture, null, Unsafe.AsPointer(ref _finalColorBuffer[0]), _pitch);
        _sdl.RenderCopyEx(_renderer, _texture, null, &destination, 0.0, null, flip);
    }

    public void ProcessingPixels(Action<Pixel> action)
    {
        Parallel.ForEach(Partitioner.Create(0, _pixels.Length), (range) =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                action(_pixels[i]);
            }
        });
    }

    public void Dispose()
    {
        _sdl.DestroyTexture(_texture);

        GC.SuppressFinalize(this);
    }

    private int GetIndex(Pixel pixel)
    {
        return pixel.Y * _width + pixel.X;
    }
}
