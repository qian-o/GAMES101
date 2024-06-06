using Maths;
using PA;
using PA.Graphics;

namespace PA3;

internal class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static Rasterizer _rasterizer = null!;
    private static AssimpParsing _assimpParsing = null!;
    private static Sampler2d _sampler = null!;
    private static int[] vbo = [];
    private static int[] ibo = [];

    private static void Main(string[] _)
    {
        _windowRenderer = new("PA 3");
        _windowRenderer.Load += WindowRenderer_Load;
        _windowRenderer.Update += WindowRenderer_Update;
        _windowRenderer.Render += WindowRenderer_Render;

        _rasterizer = new Rasterizer(_windowRenderer);
        _assimpParsing = new AssimpParsing(Path.Combine("Models", "spot", "spot_triangulated_good.obj"));
        _sampler = new Sampler2d(Path.Combine("Models", "spot", "spot_texture.png"));

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Load()
    {
        _rasterizer.Model = Matrix4x4d.Identity;
        _rasterizer.View = Matrix4x4d.CreateLookAt(new(0.0, 0.0, 5.0), new(0.0, 0.0, 0.0), new(0.0, 1.0, 0.0));

        _rasterizer.Frag = TextureFragmentShader;

        int index = 0;
        vbo = new int[_assimpParsing.MeshNames.Length];
        ibo = new int[_assimpParsing.MeshNames.Length];
        foreach (string mesh in _assimpParsing.MeshNames)
        {
            vbo[index] = _rasterizer.CreateVertexBuffer(_assimpParsing.Vertices(mesh));
            ibo[index] = _rasterizer.CreateIndexBuffer(_assimpParsing.Indices(mesh));

            index++;
        }
    }

    private static void WindowRenderer_Update(double delta)
    {
        _rasterizer.Model = Matrix4x4d.CreateRotationY(Angle.FromDegrees(25.0)) * _rasterizer.Model;
        _rasterizer.Projection = Matrix4x4d.CreatePerspectiveFieldOfView(Angle.FromDegrees(45), (double)_windowRenderer.Width / _windowRenderer.Height, 0.1, 100.0);

        _rasterizer.SetViewport(0, 0, _windowRenderer.Width, _windowRenderer.Height);
    }

    private static void WindowRenderer_Render(double delta)
    {
        _rasterizer.Clear();

        for (int i = 0; i < vbo.Length; i++)
        {
            _rasterizer.Render(vbo[i], ibo[i]);
        }
    }

    private static Color TextureFragmentShader(Vertex vertex)
    {
        Vector2d uv = vertex.TexCoord;

        return _sampler.Sample(uv.X, uv.Y);
    }
}
