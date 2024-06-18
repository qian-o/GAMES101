using System.Collections.Concurrent;
using Maths;
using Silk.NET.OpenGL;

namespace PA.Graphics;

public unsafe class FrameBuffer : IDisposable
{
    private readonly GL _gl;
    private readonly int _width;
    private readonly int _height;
    private readonly uint _sampleCount;
    private readonly Vector2d[] _pattern;
    private readonly Pixel[] _pixels;
    private readonly Buffer<Vector4d>[] _colorBuffer;
    private readonly Buffer<float>[] _depthBuffer;
    private readonly Buffer<Vector4d> _finalColorBuffer;
    private readonly Buffer<float> _finalDepthBuffer;
    private readonly uint _texture;

    public FrameBuffer(GL gl, int width, int height, SampleCount sampleCount = SampleCount.SampleCount1)
    {
        _gl = gl;
        _width = width;
        _height = height;
        _sampleCount = (uint)sampleCount;
        _pattern = sampleCount switch
        {
            SampleCount.SampleCount1 => [new(0.5f, 0.5f)],
            SampleCount.SampleCount2 =>
            [
                new(0.75f, 0.75f),
                new(0.25f, 0.25f)
            ],
            SampleCount.SampleCount4 =>
            [
                new(0.375f, 0.125f),
                new(0.875f, 0.375f),
                new(0.125f, 0.625f),
                new(0.625f, 0.875f)
            ],
            SampleCount.SampleCount8 =>
            [
                new(0.5625f, 0.3125f),
                new(0.4375f, 0.6875f),
                new(0.8125f, 0.5625f),
                new(0.3125f, 0.1875f),
                new(0.1875f, 0.8125f),
                new(0.0625f, 0.4375f),
                new(0.6875f, 0.9375f),
                new(0.9375f, 0.0625f)
            ],
            SampleCount.SampleCount16 =>
            [
                new(0.5625f, 0.5625f),
                new(0.4375f, 0.3125f),
                new(0.3125f, 0.625f),
                new(0.75f, 0.4375f),
                new(0.1875f, 0.375f),
                new(0.625f, 0.8125f),
                new(0.8125f, 0.6875f),
                new(0.6875f, 0.1875f),
                new(0.375f, 0.875f),
                new(0.5f, 0.0625f),
                new(0.25f, 0.125f),
                new(0.125f, 0.75f),
                new(0.0f, 0.5f),
                new(0.9375f, 0.25f),
                new(0.875f, 0.9375f),
                new(0.0625f, 0.0f)
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

        _colorBuffer = new Buffer<Vector4d>[_sampleCount];
        _depthBuffer = new Buffer<float>[_sampleCount];

        for (int i = 0; i < _sampleCount; i++)
        {
            _colorBuffer[i] = new Buffer<Vector4d>(width * height);
            _depthBuffer[i] = new Buffer<float>(width * height);
        }

        _finalColorBuffer = new Buffer<Vector4d>(width * height);
        _finalDepthBuffer = new Buffer<float>(width * height);

        _texture = _gl.GenTexture();
        _gl.BindTexture(GLEnum.Texture2D, _texture);
        {
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);

            _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.Float, null);
        }
        _gl.BindTexture(GLEnum.Texture2D, 0);

        Pattern = [.. _pattern];
        Pixels = [.. _pixels];
    }

    public Vector2d[] Pattern { get; }

    public Pixel[] Pixels { get; }

    public int Width => _width;

    public int Height => _height;

    public uint Texture => _texture;

    public void Clear(Vector4d color = default)
    {
        for (int i = 0; i < _sampleCount; i++)
        {
            _colorBuffer[i].Fill(color);
            _depthBuffer[i].Fill(float.NegativeInfinity);
        }
    }

    public Vector4d GetColor(Pixel pixel, int index)
    {
        return _colorBuffer[index][GetIndex(pixel)];
    }

    public float GetDepth(Pixel pixel, int index)
    {
        return _depthBuffer[index][GetIndex(pixel)];
    }

    public void SetColor(Pixel pixel, int index, Vector4d color)
    {
        _colorBuffer[index][GetIndex(pixel)] = color;
    }

    public void SetDepth(Pixel pixel, int index, float depth)
    {
        _depthBuffer[index][GetIndex(pixel)] = depth;
    }

    public void Present()
    {
        if (_sampleCount == 1)
        {
            _colorBuffer[0].CopyTo(_finalColorBuffer);
            _depthBuffer[0].CopyTo(_finalDepthBuffer);
        }
        else
        {
            Parallel.ForEach(_pixels, (pixel) =>
            {
                Vector4d color = new(0.0f, 0.0f, 0.0f, 0.0f);
                float depth = float.MinValue;

                for (int i = 0; i < _sampleCount; i++)
                {
                    color += GetColor(pixel, i);
                    depth = Math.Max(depth, GetDepth(pixel, i));
                }

                _finalColorBuffer[GetIndex(pixel)] = color / _sampleCount;
                _finalDepthBuffer[GetIndex(pixel)] = depth;
            });
        }

        _gl.BindTexture(GLEnum.Texture2D, _texture);
        _gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, 0, (uint)_width, (uint)_height, GLEnum.Rgba, GLEnum.Float, _finalColorBuffer.Data);
        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void ProcessingPixels(Action<Pixel> action, bool isSingleThread = false)
    {
        if (_pixels.Length == 0)
        {
            return;
        }

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
        foreach (Buffer<Vector4d> buffer in _colorBuffer)
        {
            buffer.Dispose();
        }

        foreach (Buffer<float> buffer in _depthBuffer)
        {
            buffer.Dispose();
        }

        _finalColorBuffer.Dispose();
        _finalDepthBuffer.Dispose();

        _gl.DeleteTexture(_texture);

        GC.SuppressFinalize(this);
    }

    private int GetIndex(Pixel pixel)
    {
        return pixel.Y * _width + pixel.X;
    }
}
