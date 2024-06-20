using System.Numerics;
using ImGuiNET;
using Maths;
using PA.Graphics;

namespace PA5;

internal class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static Scene _scene = null!;
    private static Renderer _renderer = null!;

    static void Main(string[] _)
    {
        _windowRenderer = new("PA 5");
        _windowRenderer.Load += WindowRenderer_Load;
        _windowRenderer.Render += WindowRenderer_Render;

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Load()
    {
        _scene = new(_windowRenderer.GL, _windowRenderer.Width, _windowRenderer.Height, SampleCount.SampleCount16);

        Material mat1 = new()
        {
            MaterialType = MaterialType.DiffuseAndGlossy,
            DiffuseColor = new Vector3d(0.6f, 0.7f, 0.8f)
        };

        Material mat2 = new()
        {
            MaterialType = MaterialType.ReflectAndRefract,
            Ior = 1.5f
        };

        Material mat3 = new()
        {
            MaterialType = MaterialType.DiffuseAndGlossy
        };

        _scene.Materials.Add(mat1);
        _scene.Materials.Add(mat2);
        _scene.Materials.Add(mat3);

        Geometry sph1 = Geometry.CreateSphere(new Vector3d(-1.0f, 0.0f, -12.0f), 2.0f, 0);

        Geometry sph2 = Geometry.CreateSphere(new Vector3d(0.5f, -0.5f, -8.0f), 1.5f, 1);

        Vertex[] verts =
        [
            new(new(-5.0f, -3.0f, -06.0f), texCoord: new(0.0f, 0.0f)),
            new(new( 5.0f, -3.0f, -06.0f), texCoord: new(1.0f, 0.0f)),
            new(new( 5.0f, -3.0f, -16.0f), texCoord: new(1.0f, 1.0f)),
            new(new(-5.0f, -3.0f, -16.0f), texCoord: new(0.0f, 1.0f))
        ];
        int[] indices = [0, 1, 3, 1, 2, 3];

        Geometry tri1 = Geometry.CreateTriangle(new Triangle(verts[indices[0]], verts[indices[1]], verts[indices[2]]), 2);
        Geometry tri2 = Geometry.CreateTriangle(new Triangle(verts[indices[3]], verts[indices[4]], verts[indices[5]]), 2);

        _scene.Objects.Add(sph1);
        _scene.Objects.Add(sph2);

        _scene.Objects.Add(tri1);
        _scene.Objects.Add(tri2);

        Light light1 = new(new Vector3d(-20.0f, 70.0f, 20.0f), new Vector3d(0.5f));
        Light light2 = new(new Vector3d(30.0f, 50.0f, -12.0f), new Vector3d(0.5f));

        _scene.Lights.Add(light1);
        _scene.Lights.Add(light2);

        _renderer = new(_scene);
    }

    private static void WindowRenderer_Render(float obj)
    {
        ImGui.Begin("Scene Properties");
        {
            ImGui.Text("Max Depth");
            ImGui.SliderInt("##MaxDepth", ref _scene.MaxDepth, 1, 10);
        }
        ImGui.End();

        ImGui.Begin("PA 5");
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
