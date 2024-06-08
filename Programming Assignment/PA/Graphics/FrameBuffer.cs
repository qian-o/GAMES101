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
    private readonly Vector4d[][] _colorBuffer;
    private readonly double[][] _depthBuffer;
    private readonly Vector4d[] _finalColorBuffer;
    private readonly double[] _finalDepthBuffer;
    private readonly byte[] _sdlColors;

    public FrameBuffer(Sdl sdl, Renderer* renderer, int width, int height, SampleCount sampleCount = SampleCount.SampleCount1)
    {
        _sdl = sdl;
        _renderer = renderer;
        _width = width;
        _height = height;
        _sampleCount = (uint)sampleCount;
        _pitch = width * sizeof(byte) * 4;
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

        _colorBuffer = new Vector4d[_sampleCount][];
        _depthBuffer = new double[_sampleCount][];
        for (int i = 0; i < _sampleCount; i++)
        {
            _colorBuffer[i] = new Vector4d[width * height];
            _depthBuffer[i] = new double[width * height];
        }

        _finalColorBuffer = new Vector4d[width * height];
        _finalDepthBuffer = new double[width * height];

        _sdlColors = new byte[width * height * 4];

        Pattern = [.. _pattern];
        Pixels = [.. _pixels];
    }

    public Vector2d[] Pattern { get; }

    public Pixel[] Pixels { get; }

    public void Clear(Vector4d vector = default)
    {
        for (int i = 0; i < _sampleCount; i++)
        {
            Array.Fill(_colorBuffer[i], vector);
            Array.Fill(_depthBuffer[i], double.NegativeInfinity);
        }

        Array.Fill(_finalColorBuffer, vector);
        Array.Fill(_finalDepthBuffer, double.NegativeInfinity);
    }

    public Vector4d GetColor(Pixel pixel, int index)
    {
        return _colorBuffer[index][GetIndex(pixel)];
    }

    public double GetDepth(Pixel pixel, int index)
    {
        return _depthBuffer[index][GetIndex(pixel)];
    }

    public void SetColor(Pixel pixel, int index, Vector4d vector)
    {
        _colorBuffer[index][GetIndex(pixel)] = vector;
    }

    public void SetDepth(Pixel pixel, int index, double depth)
    {
        _depthBuffer[index][GetIndex(pixel)] = depth;
    }

    public Vector4d GetFinalColor(Pixel pixel)
    {
        return _finalColorBuffer[GetIndex(pixel)];
    }

    public double GetFinalDepth(Pixel pixel)
    {
        return _finalDepthBuffer[GetIndex(pixel)];
    }

    public void SetFinalColor(Pixel pixel, Vector4d vector)
    {
        _finalColorBuffer[GetIndex(pixel)] = vector;
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
                double r = 0;
                double g = 0;
                double b = 0;
                double a = 0;
                double depth = double.MinValue;

                for (int i = 0; i < _sampleCount; i++)
                {
                    Vector4d color = GetColor(pixel, i);
                    double d = GetDepth(pixel, i);

                    r += color.X;
                    g += color.Y;
                    b += color.Z;
                    a += color.W;
                    depth = Math.Max(depth, d);
                }

                r /= _sampleCount;
                g /= _sampleCount;
                b /= _sampleCount;
                a /= _sampleCount;

                SetFinalColor(pixel, new Vector4d(r, g, b, a));
                SetFinalDepth(pixel, depth);
            });
        }

        Parallel.For(0, _finalColorBuffer.Length, (i) =>
        {
            _sdlColors[i * 4] = (byte)Math.Clamp(_finalColorBuffer[i].W * 255, 0, 255);
            _sdlColors[i * 4 + 1] = (byte)Math.Clamp(_finalColorBuffer[i].Z * 255, 0, 255);
            _sdlColors[i * 4 + 2] = (byte)Math.Clamp(_finalColorBuffer[i].Y * 255, 0, 255);
            _sdlColors[i * 4 + 3] = (byte)Math.Clamp(_finalColorBuffer[i].X * 255, 0, 255);
        });

        _sdl.UpdateTexture(_texture, null, Unsafe.AsPointer(ref _sdlColors[0]), _pitch);
        _sdl.RenderCopyEx(_renderer, _texture, null, &destination, 0.0, null, flip);
    }

    public void ProcessingPixels(Action<Pixel> action, bool isSingleThread = false)
    {
        if (isSingleThread)
        {
            foreach (Pixel pixel in _pixels)
            {
                action(pixel);
            }
        }
        else
        {
            Parallel.ForEach(Partitioner.Create(0, _pixels.Length), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    action(_pixels[i]);
                }
            });
        }
    }

    public void ProcessingPixelsByBox(Box2d box, Action<Pixel> action, bool isSingleThread = false)
    {
        Pixel[] pixels = Pixels.AsParallel().Where((pixel) => box.Contains(pixel.X, pixel.Y)).ToArray();

        if (pixels.Length == 0)
        {
            return;
        }

        if (isSingleThread)
        {
            foreach (Pixel pixel in pixels)
            {
                action(pixel);
            }
        }
        else
        {
            Parallel.ForEach(Partitioner.Create(0, pixels.Length), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    action(pixels[i]);
                }
            });
        }
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
