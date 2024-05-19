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
    private Matrix4X4 viewport;
    private Point[]? points;
    private Triangle[]? triangles;
    private Matrix4X4 transform;

    public bool FlipY { get; set; } = true;

    public Matrix4X4 Model { get; set; }

    public Matrix4X4 View { get; set; }

    public Matrix4X4 Projection { get; set; }

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

            viewport = Matrix4X4.CreateViewport(x, y, width, height, 0, 1);
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
        Vector2D center = new(x + 0.5, y + 0.5);

        Vector2D a = (transform * triangle.A.Position).XY();
        Vector2D b = (transform * triangle.B.Position).XY();
        Vector2D c = (transform * triangle.C.Position).XY();

        Vector2D ab = b - a;
        Vector2D bc = c - b;
        Vector2D ca = a - c;

        Vector2D ap = center - a;
        Vector2D bp = center - b;
        Vector2D cp = center - c;

        double abp = Vector2D.Cross(ab, ap);
        double bcp = Vector2D.Cross(bc, bp);
        double cap = Vector2D.Cross(ca, cp);

        return abp >= 0 && bcp >= 0 && cap >= 0;
    }
}
