using Maths;
using PA.Graphics;
using PA2;

internal class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static Rasterizer _rasterizer = null!;
    private static int vbo = 0;
    private static int ibo = 0;

    private static void Main(string[] _)
    {
        _windowRenderer = new("PA 2");
        _windowRenderer.Load += WindowRenderer_Load;
        _windowRenderer.Update += WindowRenderer_Update;
        _windowRenderer.Render += WindowRenderer_Render;

        _rasterizer = new Rasterizer(_windowRenderer, SampleCount.SampleCount4);

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Load()
    {
        _rasterizer.Model = Matrix4x4d.Identity;
        _rasterizer.View = Matrix4x4d.CreateLookAt(new(0.0, 0.0, 5.0), new(0.0, 0.0, 0.0), new(0.0, 1.0, 0.0));

        Vertex a = new(new(2.0, 0.0, -2.0), color: new(1, 0, 0,1));
        Vertex b = new(new(0.0, 2.0, -2.0), color: new(0, 1, 0, 1));
        Vertex c = new(new(-2.0, 0.0, -2.0), color: new(0, 0, 1, 1));
        Vertex d = new(new(3.5, -1.0, -5.0), color: new(185 / 255.0, 217 / 255.0, 238 / 255.0, 1));
        Vertex e = new(new(2.5, 1.5, -5.0), color: new(185 / 255.0, 217 / 255.0, 238 / 255.0, 1));
        Vertex f = new(new(-1.0, 0.5, -1.0), color: new(185 / 255.0, 217 / 255.0, 238 / 255.0, 1));

        vbo = _rasterizer.CreateVertexBuffer([a, b, c, d, e, f]);
        ibo = _rasterizer.CreateIndexBuffer([0, 1, 2, 3, 4, 5]);
    }

    private static void WindowRenderer_Update(double delta)
    {
        _rasterizer.Projection = Matrix4x4d.CreatePerspectiveFieldOfView(Angle.FromDegrees(45), (double)_windowRenderer.Width / _windowRenderer.Height, 0.1, 100.0);

        _rasterizer.SetViewport(0, 0, _windowRenderer.Width, _windowRenderer.Height);
    }

    private static void WindowRenderer_Render(double delta)
    {
        _rasterizer.Clear();

        _rasterizer.Render(vbo, ibo);
    }
}