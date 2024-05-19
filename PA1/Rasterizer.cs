using Maths;
using Silk.NET.SDL;

namespace PA1;

public unsafe class Rasterizer(WindowRenderer windowRenderer)
{
    private readonly Sdl _sdl = windowRenderer.Sdl;
    private readonly Renderer* _renderer = windowRenderer.Renderer;
    private readonly Dictionary<int, Vertex[]> bufferVertexes = [];
    private readonly Dictionary<int, int[]> bufferIndices = [];

    private int x;
    private int y;
    private int width;
    private int height;
    private Matrix4x4d viewport;
    private Point[]? frameBuffer;
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
            if (x > width)
            {
                throw new ArgumentException("x must be less than or equal to width.");
            }

            if (y > height)
            {
                throw new ArgumentException("y must be less than or equal to height.");
            }

            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            viewport = Matrix4x4d.CreateViewport(x, y, width, height, 0, 1);
            frameBuffer = new Point[(width - x) * (height - y)];
        }
    }

    public void Clear()
    {
        if (frameBuffer is null)
        {
            return;
        }

        Array.Fill(frameBuffer, new Point(-1, -1));
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

            if (CCW)
            {
                triangles[i / 3] = new Triangle(a, b, c);
            }
            else
            {
                triangles[i / 3] = new Triangle(c, b, a);
            }
        }

        transform = viewport * Projection * View * Model;

        _sdl.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        _sdl.RenderClear(_renderer);

        _sdl.SetRenderDrawColor(_renderer, 255, 255, 255, 255);

        Parallel.For(x, width, i =>
        {
            for (int j = y; j < height; j++)
            {
                foreach (Triangle triangle in triangles)
                {
                    if (IsPointInTriangle(triangle, i, j))
                    {
                        int index = j * width + i;
                        int indexX = i;
                        int indexY = j;

                        if (FlipY)
                        {
                            indexY = height - j - 1;
                        }

                        frameBuffer[index] = new Point(indexX, indexY);

                        break;
                    }
                }
            }
        });

        _sdl.RenderDrawPoints(_renderer, frameBuffer, frameBuffer.Length);
    }

    private bool IsPointInTriangle(Triangle triangle, int x, int y)
    {
        Vector2d center = new(x + 0.5, y + 0.5);

        Vector2d a = (transform * triangle.A.Position).XY();
        Vector2d b = (transform * triangle.B.Position).XY();
        Vector2d c = (transform * triangle.C.Position).XY();

        Vector2d ab = b - a;
        Vector2d bc = c - b;
        Vector2d ca = a - c;

        Vector2d ap = center - a;
        Vector2d bp = center - b;
        Vector2d cp = center - c;

        double abp = Vector2d.Cross(ab, ap);
        double bcp = Vector2d.Cross(bc, bp);
        double cap = Vector2d.Cross(ca, cp);

        return CCW ? abp >= 0 && bcp >= 0 && cap >= 0 : abp <= 0 && bcp <= 0 && cap <= 0;
    }
}
