﻿using System.Numerics;
using ImGuiNET;
using Maths;
using PA.Graphics;
using PA2;

internal class Program
{
    private static Window _window = null!;
    private static Rasterizer _rasterizer = null!;
    private static int vbo = 0;
    private static int ibo = 0;

    private static void Main(string[] _)
    {
        _window = new("PA 2");
        _window.Load += Window_Load;
        _window.Render += Window_Render;

        _window.Run();

        _window.Dispose();
    }

    private static void Window_Load()
    {
        _rasterizer = new Rasterizer(_window, SampleCount.SampleCount4)
        {
            Model = Matrix4x4d.Identity,
            View = Matrix4x4d.CreateLookAt(new(0.0f, 0.0f, 5.0f), new(0.0f, 0.0f, 0.0f), new(0.0f, 1.0f, 0.0f))
        };

        Vertex a = new(new(2.0f, 0.0f, -2.0f), color: new(1, 0, 0, 1));
        Vertex b = new(new(0.0f, 2.0f, -2.0f), color: new(0, 1, 0, 1));
        Vertex c = new(new(-2.0f, 0.0f, -2.0f), color: new(0, 0, 1, 1));
        Vertex d = new(new(3.5f, -1.0f, -5.0f), color: new(185 / 255.0f, 217 / 255.0f, 238 / 255.0f, 1));
        Vertex e = new(new(2.5f, 1.5f, -5.0f), color: new(185 / 255.0f, 217 / 255.0f, 238 / 255.0f, 1));
        Vertex f = new(new(-1.0f, 0.5f, -1.0f), color: new(185 / 255.0f, 217 / 255.0f, 238 / 255.0f, 1));

        vbo = _rasterizer.CreateVertexBuffer([a, b, c, d, e, f]);
        ibo = _rasterizer.CreateIndexBuffer([0, 1, 2, 3, 4, 5]);
    }

    private static void Window_Render(float delta)
    {
        ImGui.Begin("PA 2");
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