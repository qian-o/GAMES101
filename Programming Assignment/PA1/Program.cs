using System.Numerics;
using ImGuiNET;
using Maths;
using PA.Graphics;
using Silk.NET.Input;

namespace PA1;

internal unsafe class Program
{
    private static Window _window = null!;
    private static Rasterizer _rasterizer = null!;
    private static int vbo = 0;
    private static int ibo = 0;
    private static float angle;

    private static void Main(string[] _)
    {
        _window = new("PA 1");
        _window.Load += Window_Load;
        _window.Update += Window_Update;
        _window.Render += Window_Render;

        _window.Run();

        _window.Dispose();
    }

    private static void Window_Load()
    {
        _rasterizer = new Rasterizer(_window)
        {
            Model = Matrix4x4d.Identity,
            View = Matrix4x4d.CreateLookAt(new(0.0f, 0.0f, 5.0f), new(0.0f, 0.0f, 0.0f), new(0.0f, 1.0f, 0.0f))
        };

        Vertex a = new(new(2.0f, 0.0f, -2.0f));
        Vertex b = new(new(0.0f, 2.0f, -2.0f));
        Vertex c = new(new(-2.0f, 0.0f, -2.0f));

        vbo = _rasterizer.CreateVertexBuffer([a, b, c]);
        ibo = _rasterizer.CreateIndexBuffer([0, 1, 2]);
    }

    private static void Window_Update(float delta)
    {
        if (_window.Keyboard.IsKeyPressed(Key.A))
        {
            angle += 10;
        }

        if (_window.Keyboard.IsKeyPressed(Key.D))
        {
            angle -= 10;
        }

        _rasterizer.Model = Matrix4x4d.CreateRotationZ(Angle.FromDegrees(angle));
    }

    private static void Window_Render(float delta)
    {
        ImGui.Begin("PA 1");
        {
            Vector2 size = ImGui.GetContentRegionAvail();

            _rasterizer.Projection = Matrix4x4d.CreatePerspectiveFieldOfView(Angle.FromDegrees(45), size.X / size.Y, 0.1f, 100.0f);

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