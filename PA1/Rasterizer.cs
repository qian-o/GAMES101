using Maths;
using Silk.NET.SDL;

namespace PA1;

public unsafe class Rasterizer(WindowRenderer windowRenderer)
{
    private readonly Sdl _sdl = windowRenderer.Sdl;
    private readonly Renderer* _renderer = windowRenderer.Renderer;

    private int x;
    private int y;
    private int width;
    private int height;
    private Matrix4x4d viewport;
    private Point[]? points;
    private Triangle[]? triangles;
    private Matrix4x4d transform;

    public bool FlipY { get; set; } = true;

    public Matrix4x4d Model { get; set; }

    public Matrix4x4d View { get; set; }

    public Matrix4x4d Projection { get; set; }

    public void SetTriangles(Triangle[] triangles)
    {
        this.triangles = triangles;
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
            points = new Point[(width - x) * (height - y)];
        }
    }

    public void Render()
    {
        if (points is null || triangles is null)
        {
            return;
        }

        Array.Fill(points, new Point(-1, -1));

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

                        points[index] = new Point(indexX, indexY);

                        break;
                    }
                }
            }
        });

        _sdl.RenderDrawPoints(_renderer, points, points.Length);
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

        return abp >= 0 && bcp >= 0 && cap >= 0;
    }
}
