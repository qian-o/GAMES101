using Maths;
using PA.Graphics;
using Silk.NET.Input;
using Silk.NET.SDL;

internal unsafe class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static List<Vector2d> _points = null!;

    private static void Main(string[] _)
    {
        _windowRenderer = new("PA 4");
        _windowRenderer.Render += WindowRenderer_Render;

        _windowRenderer.Mouse.Click += (_, button, position) =>
        {
            if (button == MouseButton.Left)
            {
                _points.Add(position.ToMaths());
            }
            else if (button == MouseButton.Right)
            {
                _points.Clear();
            }
        };

        _points = new(4);

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Render(double delta)
    {
        _windowRenderer.Sdl.SetRenderDrawColor(_windowRenderer.Renderer, 0, 0, 0, 255);
        _windowRenderer.Sdl.RenderClear(_windowRenderer.Renderer);

        _windowRenderer.Sdl.SetRenderDrawColor(_windowRenderer.Renderer, 255, 255, 255, 255);
        foreach (Vector2d point in _points)
        {
            FRect rect = new((float)point.X - 2.5f, (float)point.Y - 2.5f, 5, 5);

            _windowRenderer.Sdl.RenderFillRectF(_windowRenderer.Renderer, ref rect);
        }

        NaiveBezier(_windowRenderer.Sdl, _windowRenderer.Renderer);
        Bezier(_windowRenderer.Sdl, _windowRenderer.Renderer);
    }

    private static void NaiveBezier(Sdl sdl, Renderer* renderer)
    {
        if (_points.Count < 4)
        {
            return;
        }

        Vector2d p0 = _points[0];
        Vector2d p1 = _points[1];
        Vector2d p2 = _points[2];
        Vector2d p3 = _points[3];

        sdl.SetRenderDrawColor(renderer, 255, 0, 0, 255);

        for (double t = 0; t <= 1.0; t += 0.0001)
        {
            Vector2d point = (p0 * Math.Pow(1 - t, 3))
                             + (p1 * 3 * Math.Pow(1 - t, 2) * t)
                             + (p2 * 3 * (1 - t) * Math.Pow(t, 2))
                             + (p3 * Math.Pow(t, 3));

            sdl.RenderDrawPointF(renderer, (float)point.X, (float)point.Y);
        }
    }

    private static Vector2d RecursiveBezier(Vector2d[] controlPoints, double t)
    {
        if (controlPoints.Length == 1)
        {
            return controlPoints[0];
        }

        Vector2d[] newControlPoints = new Vector2d[controlPoints.Length - 1];
        for (int i = 0; i < newControlPoints.Length; i++)
        {
            newControlPoints[i] = MathsHelper.Lerp(controlPoints[i], controlPoints[i + 1], t);
        }

        return RecursiveBezier(newControlPoints, t);
    }

    private static void Bezier(Sdl sdl, Renderer* renderer)
    {
        if (_points.Count == 0)
        {
            return;
        }

        sdl.SetRenderDrawColor(renderer, 0, 255, 0, 255);

        for (double t = 0; t <= 1.0; t += 0.0001)
        {
            Vector2d point = RecursiveBezier([.. _points], t);

            sdl.RenderDrawPointF(renderer, (float)point.X, (float)point.Y);
        }
    }
}