using Maths;
using StbiSharp;

namespace PA.Graphics;

public class Sampler2d : IDisposable
{
    private readonly StbiImage _stbiImage;

    public Sampler2d(string image)
    {
        if (!File.Exists(image))
        {
            throw new FileNotFoundException("Image not found", image);
        }

        Stbi.SetFlipVerticallyOnLoad(true);

        _stbiImage = Stbi.LoadFromMemory(File.ReadAllBytes(image), 4);
    }

    public int Width => _stbiImage.Width;

    public int Height => _stbiImage.Height;

    public Vector4d Sample(double u, double v)
    {
        int x = (int)(u * _stbiImage.Width);
        int y = (int)(v * _stbiImage.Height);

        x %= _stbiImage.Width;
        y %= _stbiImage.Height;

        int index = (y * _stbiImage.Width + x) * 4;

        return new Vector4d(_stbiImage.Data[index + 0], _stbiImage.Data[index + 1], _stbiImage.Data[index + 2], _stbiImage.Data[index + 3]);
    }

    public Vector4d SampleNormalized(double u, double v)
    {
        return Sample(u, v) / 255.0;
    }

    public void Dispose()
    {
        _stbiImage.Dispose();

        GC.SuppressFinalize(this);
    }
}
