using System.Collections.ObjectModel;
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
    private readonly (double OffsetX, double OffsetY)[] _pattern;
    private readonly FixedArray<Vector4D<byte>>[] _pixels;
    private readonly FixedArray<double>[] _depths;
    private readonly FixedArray<Vector4D<byte>> _finalPixels;
    private readonly FixedArray<double> _finalDepths;

    public FrameBuffer(Sdl sdl, Renderer* renderer, int width, int height, SampleCount sampleCount = SampleCount.SampleCount1)
    {
        _sdl = sdl;
        _renderer = renderer;
        _width = width;
        _height = height;
        _sampleCount = (int)sampleCount;
        _pitch = width * sizeof(Vector4D<byte>);
        _texture = sdl.CreateTexture(renderer, (int)PixelFormatEnum.Abgr32, (int)TextureAccess.Streaming, width, height);
        _pattern = sampleCount switch
        {
            SampleCount.SampleCount1 => [(0.5, 0.5)],
            SampleCount.SampleCount2 =>
            [
                (0.75, 0.75),
                (0.25, 0.25)
            ],
            SampleCount.SampleCount4 =>
            [
                (0.375, 0.125),
                (0.875, 0.375),
                (0.125, 0.625),
                (0.625, 0.875)
            ],
            SampleCount.SampleCount8 =>
            [
                (0.5625, 0.3125),
                (0.4375, 0.6875),
                (0.8125, 0.5625),
                (0.3125, 0.1875),
                (0.1875, 0.8125),
                (0.0625, 0.4375),
                (0.6875, 0.9375),
                (0.9375, 0.0625)
            ],
            SampleCount.SampleCount16 =>
            [
                (0.5625, 0.5625),
                (0.4375, 0.3125),
                (0.3125, 0.625),
                (0.75, 0.4375),
                (0.1875, 0.375),
                (0.625, 0.8125),
                (0.8125, 0.6875),
                (0.6875, 0.1875),
                (0.375, 0.875),
                (0.5, 0.0625),
                (0.25, 0.125),
                (0.125, 0.75),
                (0.0, 0.5),
                (0.9375, 0.25),
                (0.875, 0.9375),
                (0.0625, 0.0)
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount)),
        };

        _pixels = new FixedArray<Vector4D<byte>>[_sampleCount];
        _depths = new FixedArray<double>[_sampleCount];
        for (int i = 0; i < _sampleCount; i++)
        {
            _pixels[i] = new FixedArray<Vector4D<byte>>(width * height);
            _depths[i] = new FixedArray<double>(width * height);
        }

        _finalPixels = new FixedArray<Vector4D<byte>>(width * height);
        _finalDepths = new FixedArray<double>(width * height);
    }

    public ReadOnlyCollection<(double OffsetX, double OffsetY)> Pattern => Array.AsReadOnly(_pattern);

    public void Clear(Color color)
    {
        for (int i = 0; i < _sampleCount; i++)
        {
            _pixels[i].Fill(RgbaToAbgr(color));
            _depths[i].Fill(double.MinValue);
        }

        _finalPixels.Fill(RgbaToAbgr(color));
    }

    public Pixel GetPixel(int x, int y, int index)
    {
        if (index < 0 || index >= _sampleCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        lock (this)
        {
            return new()
            {
                Color = AbgrToRgba(_pixels[index][GetIndex(x, y)]),
                Depth = _depths[index][GetIndex(x, y)]
            };
        }
    }

    public void SetPixel(int x, int y, int index, Pixel pixel)
    {
        if (index < 0 || index >= _sampleCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        lock (this)
        {
            _pixels[index][GetIndex(x, y)] = RgbaToAbgr(pixel.Color);
            _depths[index][GetIndex(x, y)] = pixel.Depth;
        }
    }

    public void Present(int x, int y, bool flipY)
    {
        Parallel.For(0, _finalPixels.Length, (index) =>
        {
            Vector4D<byte> color = new(0, 0, 0, 0);
            double depth = double.MinValue;

            for (int i = 0; i < _sampleCount; i++)
            {
                color += _pixels[i][index] / (byte)_sampleCount;
                depth = Math.Max(depth, _depths[i][index]);
            }

            _finalPixels[index] = color;
            _finalDepths[index] = depth;
        });

        Rectangle<int> dst = new(x, y, _width, _height);
        RendererFlip flip = flipY ? RendererFlip.Vertical : RendererFlip.None;

        _sdl.UpdateTexture(_texture, null, _finalPixels.Buffer, _pitch);
        _sdl.RenderCopyEx(_renderer, _texture, null, &dst, 0.0, null, flip);
    }

    public void Dispose()
    {
        _sdl.DestroyTexture(_texture);

        for (int i = 0; i < _sampleCount; i++)
        {
            _pixels[i].Dispose();
            _depths[i].Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private int GetIndex(int x, int y)
    {
        return y * _width + x;
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
