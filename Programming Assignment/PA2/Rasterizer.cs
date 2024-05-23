using Maths;
using PA.Graphics;
using Silk.NET.SDL;
using Color = PA.Graphics.Color;
using Vertex = PA.Graphics.Vertex;

namespace PA2;

public unsafe class Rasterizer(WindowRenderer windowRenderer)
{
    private readonly Sdl _sdl = windowRenderer.Sdl;
    private readonly Renderer* _renderer = windowRenderer.Renderer;
    private readonly Dictionary<int, Vertex[]> bufferVertexes = [];
    private readonly Dictionary<int, int[]> bufferIndices = [];
    private readonly object _lock = new();

    private int x;
    private int y;
    private int width;
    private int height;
    private Matrix4x4d viewport;
    private FrameBuffer? frameBuffer;
    private Matrix4x4d transform;

    public bool FlipY { get; set; } = true;

    public bool CCW { get; set; } = true;

    public Matrix4x4d Model { get; set; }

    public Matrix4x4d View { get; set; }

    public Matrix4x4d Projection { get; set; }

    public int CreateVertexBuffer(Vertex[] vertexes)
    {
        int id = bufferVertexes.Count;
        bufferVertexes.Add(id, vertexes);

        return id;
    }

    public int CreateIndexBuffer(int[] indices)
    {
        int id = bufferIndices.Count;
        bufferIndices.Add(id, indices);

        return id;
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        if (this.x != x || this.y != y || this.width != width || this.height != height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            viewport = Matrix4x4d.CreateViewport(x, y, width, height, 1, -1);

            frameBuffer?.Dispose();
            frameBuffer = new FrameBuffer(_sdl, _renderer, width, height);
        }
    }

    public void Clear()
    {
        if (frameBuffer is null)
        {
            return;
        }

        frameBuffer.Clear(Colors.Black);
    }

    public void Render(int vertexBufferId, int indexBufferId)
    {
        if (frameBuffer is null
            || !bufferVertexes.TryGetValue(vertexBufferId, out Vertex[]? vertexes)
            || !bufferIndices.TryGetValue(indexBufferId, out int[]? indices))
        {
            return;
        }

        Triangle[] triangles = new Triangle[indices.Length / 3];

        for (int i = 0; i < indices.Length; i += 3)
        {
            Vertex a = vertexes[indices[i]];
            Vertex b = vertexes[indices[i + 1]];
            Vertex c = vertexes[indices[i + 2]];

            triangles[i / 3] = new Triangle(a, b, c);
        }

        transform = viewport * Projection * View * Model;

        Parallel.ForEach(triangles, RasterizeTriangle);

        frameBuffer.Present(x, y, FlipY);
    }

    private void RasterizeTriangle(Triangle triangle)
    {
        if (frameBuffer is null)
        {
            return;
        }

        Vector2d a = (transform * triangle.A.Position).XY();
        Vector2d b = (transform * triangle.B.Position).XY();
        Vector2d c = (transform * triangle.C.Position).XY();

        Box2d box = Box2d.FromPoints(a, b, c);

        int minX = (int)Math.Max(box.MinX, 0);
        int minY = (int)Math.Max(box.MinY, 0);
        int maxX = (int)Math.Min(box.MaxX, width - 1);
        int maxY = (int)Math.Min(box.MaxY, height - 1);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (IsPointInTriangle([a, b, c], x, y))
                {
                    (double alpha, double beta, double gamma) = ComputeBarycentric2D([a, b, c], x, y);

                    Vector3d abg = new(alpha, beta, gamma);

                    Vector3d vectorX = new(triangle.A.Position.X, triangle.B.Position.X, triangle.C.Position.X);
                    Vector3d vectorY = new(triangle.A.Position.Y, triangle.B.Position.Y, triangle.C.Position.Y);
                    Vector3d vectorZ = new(triangle.A.Position.Z, triangle.B.Position.Z, triangle.C.Position.Z);

                    Vector3d colorR = new(triangle.A.Color.Rd, triangle.B.Color.Rd, triangle.C.Color.Rd);
                    Vector3d colorG = new(triangle.A.Color.Gd, triangle.B.Color.Gd, triangle.C.Color.Gd);
                    Vector3d colorB = new(triangle.A.Color.Bd, triangle.B.Color.Bd, triangle.C.Color.Bd);

                    Vector3d interpPosition = new(Vector3d.Dot(abg, vectorX), Vector3d.Dot(abg, vectorY), Vector3d.Dot(abg, vectorZ));
                    Color interpColor = new(Vector3d.Dot(abg, colorR), Vector3d.Dot(abg, colorG), Vector3d.Dot(abg, colorB));

                    lock (_lock)
                    {
                        UpdatePixel(frameBuffer, x, y, interpPosition, interpColor);
                    }
                }
            }
        }
    }

    private bool IsPointInTriangle(Vector2d[] vectors, int x, int y)
    {
        Vector2d center = new(x + 0.5, y + 0.5);

        Vector2d ab = vectors[1] - vectors[0];
        Vector2d bc = vectors[2] - vectors[1];
        Vector2d ca = vectors[0] - vectors[2];

        Vector2d ap = center - vectors[0];
        Vector2d bp = center - vectors[1];
        Vector2d cp = center - vectors[2];

        double abp = Vector2d.Cross(ab, ap);
        double bcp = Vector2d.Cross(bc, bp);
        double cap = Vector2d.Cross(ca, cp);

        return CCW ? abp >= 0 && bcp >= 0 && cap >= 0 : abp <= 0 && bcp <= 0 && cap <= 0;
    }

    private static void UpdatePixel(FrameBuffer frameBuffer, int x, int y, Vector3d interpPosition, Color interpColor)
    {
        double depth = frameBuffer.GetDepth(x, y);

        if (interpPosition.Z > depth)
        {
            frameBuffer.SetDepth(x, y, interpPosition.Z);
            frameBuffer.SetColor(x, y, interpColor);
        }
    }

    private static (double Alpha, double Beta, double Gamma) ComputeBarycentric2D(Vector2d[] vectors, int x, int y)
    {
        Vector2d center = new(x + 0.5, y + 0.5);

        Vector2d ab = vectors[1] - vectors[0];
        Vector2d bc = vectors[2] - vectors[1];
        Vector2d ca = vectors[0] - vectors[2];

        Vector2d ap = center - vectors[0];
        Vector2d bp = center - vectors[1];
        Vector2d cp = center - vectors[2];

        double abp = Vector2d.Cross(ab, ap);
        double bcp = Vector2d.Cross(bc, bp);
        double cap = Vector2d.Cross(ca, cp);

        double area = Vector2d.Cross(ab, -ca);

        return (bcp / area, cap / area, abp / area);
    }
}
