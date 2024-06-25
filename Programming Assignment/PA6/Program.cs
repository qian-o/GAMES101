﻿using System.Numerics;
using ImGuiNET;
using Maths;
using PA.Graphics;

namespace PA6;


internal class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static Scene _scene = null!;

    static void Main(string[] _)
    {
        _windowRenderer = new("PA 6");
        _windowRenderer.Load += WindowRenderer_Load;
        _windowRenderer.Render += WindowRenderer_Render;

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Load()
    {
        _scene = new(_windowRenderer.GL, _windowRenderer.Width, _windowRenderer.Height);

        _scene.Geometries.Add(new Sphere(new Vector3d(-1.0f, 0.0f, -12.0f), 2.0f));
        _scene.Geometries.Add(new Sphere(new Vector3d(0.5f, -0.5f, -8.0f), 1.5f));

        _scene.Lights.Add(new Light(new Vector3d(-20.0f, 70.0f, 20.0f), new Vector3d(0.5f)));
        _scene.Lights.Add(new Light(new Vector3d(30.0f, 50.0f, -12.0f), new Vector3d(0.5f)));

        _scene.BuildBVH();
    }

    private static void WindowRenderer_Render(float obj)
    {
        ImGui.Begin("PA 6");
        {
            Vector2 size = ImGui.GetContentRegionAvail();

            if (size.X > 0.0f && size.Y > 0.0f)
            {
                _scene.Width = (int)size.X;
                _scene.Height = (int)size.Y;

                ImGui.Image((nint)_scene.FrameBuffer.Texture, size);
            }
        }
        ImGui.End();
    }
}
