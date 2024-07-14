using System.Numerics;
using ImGuiNET;
using Maths;
using PA;
using PA.Graphics;

namespace PA3;

internal class Program
{
    private struct Light
    {
        public Vector3d Position;

        public Vector3d Intensity;
    }

    private static WindowRenderer _windowRenderer = null!;
    private static Rasterizer _rasterizer = null!;
    private static AssimpParsing _assimpParsing = null!;
    private static Sampler2d _sampler = null!;
    private static Func<Vertex, Vector4d> _shader = null!;
    private static int[] vbo = [];
    private static int[] ibo = [];

    private static void Main(string[] args)
    {
        _windowRenderer = new("PA 3");
        _windowRenderer.Load += WindowRenderer_Load;
        _windowRenderer.Update += WindowRenderer_Update;
        _windowRenderer.Render += WindowRenderer_Render;

        _assimpParsing = new AssimpParsing(Path.Combine("Models", "spot", "spot_triangulated_good.obj"));

        if (args.Length == 0)
        {
            args = ["-d"];
        }

        if (args.Length == 1)
        {
            if (args[0] == "-n")
            {
                _shader = NormalFragmentShader;
            }
            else if (args[0] == "-p")
            {
                _shader = PhongFragmentShader;
            }
            else if (args[0] == "-t")
            {
                _shader = TextureFragmentShader;
                _sampler = new Sampler2d(Path.Combine("Models", "spot", "spot_texture.png"));
            }
            else if (args[0] == "-b")
            {
                _shader = BumpFragmentShader;
                _sampler = new Sampler2d(Path.Combine("Models", "spot", "hmap.jpg"));
            }
            else if (args[0] == "-d")
            {
                _shader = DisplacementFragmentShader;
                _sampler = new Sampler2d(Path.Combine("Models", "spot", "hmap.jpg"));
            }
        }

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Load()
    {
        _rasterizer = new Rasterizer(_windowRenderer)
        {
            Model = Matrix4x4d.Identity,
            View = Matrix4x4d.CreateLookAt(new(0.0f, 0.0f, 4.0f), new(0.0f, 0.0f, 0.0f), new(0.0f, 1.0f, 0.0f)),

            Frag = _shader
        };

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

    private static void WindowRenderer_Update(float delta)
    {
        _rasterizer.Model = Matrix4x4d.CreateRotationY(Angle.FromDegrees(140.0f));
    }

    private static void WindowRenderer_Render(float delta)
    {
        ImGui.Begin("PA 3");
        {
            Vector2 size = ImGui.GetContentRegionAvail();

            _rasterizer.Projection = Matrix4x4d.CreatePerspectiveFieldOfView(Angle.FromDegrees(45), size.X / size.Y, 0.1f, 100.0f);

            _rasterizer.SetViewport(0, 0, (int)size.X, (int)size.Y);

            _rasterizer.Clear();

            for (int i = 0; i < vbo.Length; i++)
            {
                _rasterizer.Render(vbo[i], ibo[i]);
            }

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

    private static Vector4d NormalFragmentShader(Vertex vertex)
    {
        return new Vector4d((Vector3d.Normalize(vertex.Normal) + 1.0f) / 2.0f, 1.0f);
    }

    private static Vector4d PhongFragmentShader(Vertex vertex)
    {
        Vector3d ka = new(0.005f, 0.005f, 0.005f);
        Vector3d kd = vertex.Color.XYZ();
        Vector3d ks = new(0.7937f, 0.7937f, 0.7937f);

        Light l1 = new() { Position = new(20, 20, 20), Intensity = new(500, 500, 500) };
        Light l2 = new() { Position = new(-20, 20, 0), Intensity = new(500, 500, 500) };

        Light[] lights = [l1, l2];
        Vector3d ambLightIntensity = new(10, 10, 10);
        Vector3d eyePos = new(0, 0, 10.0f);

        float p = 150;

        Vector3d point = vertex.Position;
        Vector3d normal = vertex.Normal;

        Vector3d resultColor = new(0, 0, 0);
        foreach (Light light in lights)
        {
            Vector3d l = Vector3d.Normalize(light.Position - point);
            Vector3d n = Vector3d.Normalize(normal);
            Vector3d v = Vector3d.Normalize(eyePos - point);

            // 光源强度 = 光源强度 / 距离^2
            Vector3d ir2 = light.Intensity / (light.Position - point).LengthSquared;

            // 漫反射
            // 通过光线与法线的夹角来计算漫反射。
            {
                Vector3d ld = kd * ir2 * Math.Max(Vector3d.Dot(n, l), 0);

                resultColor += ld;
            }

            // 镜面反射 Blinn-Phong
            // 通过光线与视线的中间向量并与法线的夹角来计算镜面反射。
            {
                Vector3d h = Vector3d.Normalize(l + v);
                Vector3d ls = ks * ir2 * MathF.Pow(MathF.Max(Vector3d.Dot(n, h), 0), p);

                resultColor += ls;
            }
        }

        // 环境光
        {
            Vector3d ia = ka * ambLightIntensity;
            resultColor += ia;
        }

        return new Vector4d(resultColor, 1);
    }

    private static Vector4d TextureFragmentShader(Vertex vertex)
    {
        Vector3d ka = new(0.005f, 0.005f, 0.005f);
        Vector3d kd = _sampler.SampleNormalized(vertex.TexCoord.X, vertex.TexCoord.Y).XYZ();
        Vector3d ks = new(0.7937f, 0.7937f, 0.7937f);

        Light l1 = new() { Position = new(20, 20, 20), Intensity = new(500, 500, 500) };
        Light l2 = new() { Position = new(-20, 20, 0), Intensity = new(500, 500, 500) };

        Light[] lights = [l1, l2];
        Vector3d ambLightIntensity = new(10, 10, 10);
        Vector3d eyePos = new(0, 0, 10.0f);

        float p = 150;

        Vector3d point = vertex.Position;
        Vector3d normal = vertex.Normal;

        Vector3d resultColor = new(0, 0, 0);
        foreach (Light light in lights)
        {
            Vector3d l = Vector3d.Normalize(light.Position - point);
            Vector3d n = Vector3d.Normalize(normal);
            Vector3d v = Vector3d.Normalize(eyePos - point);

            // 光源强度 = 光源强度 / 距离^2
            Vector3d ir2 = light.Intensity / (light.Position - point).LengthSquared;

            // 漫反射
            // 通过光线与法线的夹角来计算漫反射。
            {
                Vector3d ld = kd * ir2 * Math.Max(Vector3d.Dot(n, l), 0);

                resultColor += ld;
            }

            // 镜面反射 Blinn-Phong
            // 通过光线与视线的中间向量并与法线的夹角来计算镜面反射。
            {
                Vector3d h = Vector3d.Normalize(l + v);
                Vector3d ls = ks * ir2 * MathF.Pow(MathF.Max(Vector3d.Dot(n, h), 0), p);

                resultColor += ls;
            }
        }

        // 环境光
        {
            Vector3d ia = ka * ambLightIntensity;
            resultColor += ia;
        }

        return new Vector4d(resultColor, 1);
    }

    private static Vector4d BumpFragmentShader(Vertex vertex)
    {
        Vector3d normal = vertex.Normal;

        float kh = 0.2f;
        float kn = 0.1f;

        float u = vertex.TexCoord.X;
        float v = vertex.TexCoord.Y;

        float x = normal.X * normal.Y / MathF.Sqrt((normal.X * normal.X) + (normal.Z * normal.Z));
        float y = MathF.Sqrt((normal.X * normal.X) + (normal.Z * normal.Z));
        float z = normal.Z * normal.Y / MathF.Sqrt((normal.X * normal.X) + (normal.Z * normal.Z));

        Vector3d t = new(x, y, z);
        Vector3d b = Vector3d.Cross(normal, t);

        Matrix3x3d tbn = new(t, b, normal);
        tbn = Matrix3x3d.Transpose(tbn);

        float du = kh * kn * (_sampler.Sample(u + (1.0f / _sampler.Width), v).XYZ().Length - _sampler.Sample(u, v).XYZ().Length);
        float dv = kh * kn * (_sampler.Sample(u, v + (1.0f / _sampler.Height)).XYZ().Length - _sampler.Sample(u, v).XYZ().Length);

        Vector3d ln = new(-du, -dv, 1.0f);
        normal = Vector3d.Normalize(tbn * ln);

        return new Vector4d(normal, 1.0f);
    }

    private static Vector4d DisplacementFragmentShader(Vertex vertex)
    {
        Vector3d ka = new(0.005f, 0.005f, 0.005f);
        Vector3d kd = vertex.Color.XYZ();
        Vector3d ks = new(0.7937f, 0.7937f, 0.7937f);

        Light l1 = new() { Position = new(20, 20, 20), Intensity = new(500, 500, 500) };
        Light l2 = new() { Position = new(-20, 20, 0), Intensity = new(500, 500, 500) };

        Light[] lights = [l1, l2];
        Vector3d ambLightIntensity = new(10, 10, 10);
        Vector3d eyePos = new(0, 0, 10.0f);

        float p = 150;

        Vector3d point = vertex.Position;
        Vector3d normal = vertex.Normal;

        float kh = 0.2f;
        float kn = 0.1f;

        float tu = vertex.TexCoord.X;
        float tv = vertex.TexCoord.Y;

        float x = normal.X * normal.Y / MathF.Sqrt((normal.X * normal.X) + (normal.Z * normal.Z));
        float y = MathF.Sqrt((normal.X * normal.X) + (normal.Z * normal.Z));
        float z = normal.Z * normal.Y / MathF.Sqrt((normal.X * normal.X) + (normal.Z * normal.Z));

        Vector3d t = new(x, y, z);
        Vector3d b = Vector3d.Cross(normal, t);

        Matrix3x3d tbn = new(t, b, normal);
        tbn = Matrix3x3d.Transpose(tbn);

        float du = kh * kn * (_sampler.Sample(tu + (1.0f / _sampler.Width), tv).XYZ().Length - _sampler.Sample(tu, tv).XYZ().Length);
        float dv = kh * kn * (_sampler.Sample(tu, tv + (1.0f / _sampler.Height)).XYZ().Length - _sampler.Sample(tu, tv).XYZ().Length);

        Vector3d ln = new(-du, -dv, 1.0f);

        point += normal * kn * _sampler.Sample(tu, tv).XYZ().Length;
        normal = Vector3d.Normalize(tbn * ln);

        Vector3d resultColor = new(0, 0, 0);
        foreach (Light light in lights)
        {
            Vector3d l = Vector3d.Normalize(light.Position - point);
            Vector3d n = Vector3d.Normalize(normal);
            Vector3d v = Vector3d.Normalize(eyePos - point);

            // 光源强度 = 光源强度 / 距离^2
            Vector3d ir2 = light.Intensity / (light.Position - point).LengthSquared;

            // 漫反射
            // 通过光线与法线的夹角来计算漫反射。
            {
                Vector3d ld = kd * ir2 * Math.Max(Vector3d.Dot(n, l), 0);

                resultColor += ld;
            }

            // 镜面反射 Blinn-Phong
            // 通过光线与视线的中间向量并与法线的夹角来计算镜面反射。
            {
                Vector3d h = Vector3d.Normalize(l + v);
                Vector3d ls = ks * ir2 * MathF.Pow(MathF.Max(Vector3d.Dot(n, h), 0), p);

                resultColor += ls;
            }
        }

        // 环境光
        {
            Vector3d ia = ka * ambLightIntensity;
            resultColor += ia;
        }

        return new Vector4d(resultColor, 1);
    }
}
