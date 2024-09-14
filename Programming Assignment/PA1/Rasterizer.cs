using Maths;
using PA.Graphics;
using Silk.NET.OpenGL;
using Vertex = PA.Graphics.Vertex;

namespace PA1;

public unsafe class Rasterizer(Window window)
{
    private readonly GL _gl = window.GL;
    private readonly Dictionary<int, Vertex[]> bufferVertexes = [];
    private readonly Dictionary<int, int[]> bufferIndices = [];

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

    public FrameBuffer? FrameBuffer => frameBuffer;

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
            frameBuffer = new FrameBuffer(_gl, width, height);
        }
    }

    public void Clear()
    {
        if (frameBuffer is null)
        {
            return;
        }

        frameBuffer.Clear();
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

        ParallelHelper.Foreach(frameBuffer.Pixels, (pixel) =>
        {
            foreach (Triangle triangle in triangles)
            {
                if (IsPointInTriangle(triangle, pixel.X, pixel.Y))
                {
                    frameBuffer[pixel, 0] = new Fragment(Vector4d.One);

                    break;
                }
            }
        });

        frameBuffer.Present();
    }

    private bool IsPointInTriangle(Triangle triangle, int x, int y)
    {
        Vector2d center = new(x + 0.5f, y + 0.5f);

        Vector2d a = (transform * triangle.A.Position).XY();
        Vector2d b = (transform * triangle.B.Position).XY();
        Vector2d c = (transform * triangle.C.Position).XY();

        Vector2d ab = b - a;
        Vector2d bc = c - b;
        Vector2d ca = a - c;

        Vector2d ap = center - a;
        Vector2d bp = center - b;
        Vector2d cp = center - c;

        float abp = Vector2d.Cross(ab, ap);
        float bcp = Vector2d.Cross(bc, bp);
        float cap = Vector2d.Cross(ca, cp);

        return CCW ? abp >= 0 && bcp >= 0 && cap >= 0 : abp <= 0 && bcp <= 0 && cap <= 0;
    }
}
