using Maths;
using Silk.NET.OpenGL;

namespace PA.Graphics;

public unsafe class FrameBuffer : IDisposable
{
    private readonly GL _gl;
    private readonly Buffer<Vector4d>[] _colorBuffer;
    private readonly Buffer<float>[] _depthBuffer;
    private readonly Buffer<Vector4d> _finalColorBuffer;
    private readonly Buffer<float> _finalDepthBuffer;

    public FrameBuffer(GL gl, int width, int height, SampleCount sampleCount = SampleCount.SampleCount1)
    {
        _gl = gl;

        Width = width;
        Height = height;
        Pixels = new Pixel[Width * Height];
        Patterns = GenPatterns(sampleCount);
        Texture = _gl.GenTexture();

        _colorBuffer = new Buffer<Vector4d>[Samples];
        _depthBuffer = new Buffer<float>[Samples];
        _finalColorBuffer = new Buffer<Vector4d>(Width * Height);
        _finalDepthBuffer = new Buffer<float>(Width * Height);

        Init();
    }

    public int Width { get; }

    public int Height { get; }

    public Pixel[] Pixels { get; }

    public Vector2d[] Patterns { get; }

    public int Samples => Patterns.Length;

    public uint Texture { get; }

    public int this[Pixel pixel] => pixel.Y * Width + pixel.X;

    public Fragment this[Pixel pixel, int sample]
    {
        get => new(_colorBuffer[sample][this[pixel]], _depthBuffer[sample][this[pixel]]);
        set
        {
            _colorBuffer[sample][this[pixel]] = value.Color;
            _depthBuffer[sample][this[pixel]] = value.Depth;
        }
    }

    private void Init()
    {
        for (int i = 0; i < Pixels.Length; i++)
        {
            int x = i % Width;
            int y = i / Width;

            Pixels[i] = new Pixel(x, y);
        }

        for (int i = 0; i < Samples; i++)
        {
            _colorBuffer[i] = new Buffer<Vector4d>(Width * Height);
            _depthBuffer[i] = new Buffer<float>(Width * Height);
        }

        _gl.BindTexture(GLEnum.Texture2D, Texture);
        {
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);

            _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, (uint)Width, (uint)Height, 0, GLEnum.Rgba, GLEnum.Float, null);
        }
        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void Clear(Vector4d color = default, float depth = float.NegativeInfinity)
    {
        Parallel.For(0, Samples, (i) =>
        {
            _colorBuffer[i].Fill(color);
            _depthBuffer[i].Fill(depth);
        });
    }

    public void Present()
    {
        if (Samples == 1)
        {
            _colorBuffer[0].CopyTo(_finalColorBuffer);
            _depthBuffer[0].CopyTo(_finalDepthBuffer);
        }
        else
        {
            Parallel.ForEach(Pixels, (pixel) =>
            {
                Fragment fragment = new();

                for (int i = 0; i < Samples; i++)
                {
                    fragment += this[pixel, i];
                }

                fragment /= Samples;

                _finalColorBuffer[this[pixel]] = fragment.Color;
                _finalDepthBuffer[this[pixel]] = fragment.Depth;
            });
        }

        _gl.BindTexture(GLEnum.Texture2D, Texture);
        _gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, 0, (uint)Width, (uint)Height, GLEnum.Rgba, GLEnum.Float, _finalColorBuffer.Data);
        _gl.BindTexture(GLEnum.Texture2D, 0);
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

        _gl.DeleteTexture(Texture);

        GC.SuppressFinalize(this);
    }

    public static Vector2d[] GenPatterns(SampleCount sampleCount)
    {
        return sampleCount switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount))
        };
    }
}
