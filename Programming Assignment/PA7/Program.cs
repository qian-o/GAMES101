using System.Numerics;
using ImGuiNET;
using Maths;
using PA.Graphics;

namespace PA7;

internal class Program
{
    private static Window _window = null!;
    private static Scene _scene = null!;
    private static Renderer _renderer = null!;

    private static void Main(string[] _)
    {
        _window = new("PA 7");
        _window.Load += Window_Load;
        _window.Render += Window_Render;

        _window.Run();

        _window.Dispose();
    }

    private static void Window_Load()
    {
        Material red = new()
        {
            Type = MaterialType.Diffuse,
            Emission = Vector3d.Zero,
            Kd = new Vector3d(0.63f, 0.065f, 0.05f),
        };
        Material green = new()
        {
            Type = MaterialType.Diffuse,
            Emission = Vector3d.Zero,
            Kd = new Vector3d(0.14f, 0.45f, 0.091f),
        };
        Material white = new()
        {
            Type = MaterialType.Diffuse,
            Emission = Vector3d.Zero,
            Kd = new Vector3d(0.725f, 0.71f, 0.68f),
        };
        Material light = new()
        {
            Type = MaterialType.Diffuse,
            Emission = new Vector3d(47.8348007f, 38.5663986f, 31.0807991f),
            Kd = new Vector3d(0.65f)
        };

        _scene = new(_window.GL, _window.Width, _window.Height);

        _scene.Shapes.Add(AssimpParsing.Parsing("Models/cornellbox/floor.obj", white).First());
        _scene.Shapes.Add(AssimpParsing.Parsing("Models/cornellbox/shortbox.obj", white).First());
        _scene.Shapes.Add(AssimpParsing.Parsing("Models/cornellbox/tallbox.obj", white).First());
        _scene.Shapes.Add(AssimpParsing.Parsing("Models/cornellbox/left.obj", red).First());
        _scene.Shapes.Add(AssimpParsing.Parsing("Models/cornellbox/right.obj", green).First());
        _scene.Shapes.Add(AssimpParsing.Parsing("Models/cornellbox/light.obj", light).First());

        _scene.BuildBVH();

        _renderer = new(_scene);
    }

    private static void Window_Render(float obj)
    {
        ImGui.Begin("PA 7");
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
