using Maths;
using Silk.NET.Input;

namespace PA1;

internal unsafe class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static Rasterizer _rasterizer = null!;
    private static double angle;

    private static void Main(string[] _)
    {
        _windowRenderer = new();

        _windowRenderer.Load += WindowRenderer_Load;
        _windowRenderer.Update += WindowRenderer_Update;
        _windowRenderer.Render += WindowRenderer_Render;

        _rasterizer = new Rasterizer(_windowRenderer);

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Load()
    {
        _rasterizer.Model = Matrix4x4d.Identity;
        _rasterizer.View = Matrix4x4d.CreateLookAt(new(0.0, 0.0, 5.0), new(0.0, 0.0, 0.0), new(0.0, 1.0, 0.0));

        Vertex a = new(new(2.0, 0.0, -2.0));
        Vertex b = new(new(0.0, 2.0, -2.0));
        Vertex c = new(new(-2.0, -0.0, -2.0));

        _rasterizer.SetTriangles([new(a, b, c)]);
    }

    private static void WindowRenderer_Update(double delta)
    {
        _rasterizer.Projection = Matrix4x4d.CreatePerspectiveFieldOfView(Angle.FromDegrees(45), (double)_windowRenderer.Width / _windowRenderer.Height, 0.1f, 100.0f);

        _rasterizer.SetViewport(0, 0, _windowRenderer.Width, _windowRenderer.Height);

        if (_windowRenderer.Keyboard.IsKeyPressed(Key.A))
        {
            angle += 10;
        }

        if (_windowRenderer.Keyboard.IsKeyPressed(Key.D))
        {
            angle -= 10;
        }

        _rasterizer.Model = Matrix4x4d.CreateRotationZ(Angle.FromDegrees(angle));
    }

    private static void WindowRenderer_Render(double delta)
    {
        _rasterizer.Render();
    }
}