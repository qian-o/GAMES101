using System.Numerics;
using ImGuiNET;
using Maths;
using PA.Graphics;

namespace PA6;

internal class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static Scene _scene = null!;
    private static Renderer _renderer = null!;

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
        Matrix4x4d bunnyModel = Matrix4x4d.CreateRotationX(Angle.FromDegrees(10.0f)) * Matrix4x4d.CreateScale(new Vector3d(40.0f));

        _scene = new(_windowRenderer.GL, _windowRenderer.Width, _windowRenderer.Height);

        _scene.Geometries.Add(AssimpParsing.Parsing(Path.Combine("Models", "bunny", "bunny.obj"), bunnyModel));

        _scene.Lights.Add(new Light(new Vector3d(-40.0f, 140.0f, 40.0f), new Vector3d(2.0f)));
        _scene.Lights.Add(new Light(new Vector3d(60.0f, 100.0f, -40.0f), new Vector3d(2.0f)));

        _scene.BuildBVH();

        _renderer = new(_scene);
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

                _renderer.Render();

                ImGui.Image((nint)_scene.FrameBuffer.Texture, size);
            }
        }
        ImGui.End();
    }
}
