using System.Numerics;
using ImGuiNET;
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
        _windowRenderer.Render += WindowRenderer_Render;

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Load()
    {
        _rasterizer = new Rasterizer(_windowRenderer, SampleCount.SampleCount4)
        {
            Model = Matrix4x4d.Identity,
            View = Matrix4x4d.CreateLookAt(new(0.0, 0.0, 5.0), new(0.0, 0.0, 0.0), new(0.0, 1.0, 0.0))
        };

        Vertex a = new(new(2.0, 0.0, -2.0), color: new(1, 0, 0, 1));
        Vertex b = new(new(0.0, 2.0, -2.0), color: new(0, 1, 0, 1));
        Vertex c = new(new(-2.0, 0.0, -2.0), color: new(0, 0, 1, 1));
        Vertex d = new(new(3.5, -1.0, -5.0), color: new(185 / 255.0, 217 / 255.0, 238 / 255.0, 1));
        Vertex e = new(new(2.5, 1.5, -5.0), color: new(185 / 255.0, 217 / 255.0, 238 / 255.0, 1));
        Vertex f = new(new(-1.0, 0.5, -1.0), color: new(185 / 255.0, 217 / 255.0, 238 / 255.0, 1));

        vbo = _rasterizer.CreateVertexBuffer([a, b, c, d, e, f]);
        ibo = _rasterizer.CreateIndexBuffer([0, 1, 2, 3, 4, 5]);
    }

    private static void WindowRenderer_Render(double delta)
    {
        ImGui.Begin("PA 2");
        {
            Vector2 size = ImGui.GetContentRegionAvail();

            _rasterizer.Projection = Matrix4x4d.CreatePerspectiveFieldOfView(Angle.FromDegrees(45), size.X / size.Y, 0.1, 100.0);

            _rasterizer.SetViewport(0, 0, (int)size.X, (int)size.Y);

            _rasterizer.Clear();

            _rasterizer.Render(vbo, ibo);

            if (_rasterizer.FlipY)
            {
                ImGui.Image((nint)_rasterizer.FrameBuffer!.Texture, size, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f));
            }
            else
            {
                ImGui.Image((nint)_rasterizer.FrameBuffer!.Texture, size);
            }
        }
        ImGui.End();
    }
}