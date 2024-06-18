using ImGuiNET;
using Maths;
using PA.Graphics;

internal unsafe class Program
{
    private static WindowRenderer _windowRenderer = null!;
    private static List<Vector2d> _points = null!;

    private static void Main(string[] _)
    {
        _windowRenderer = new("PA 4");
        _windowRenderer.Render += WindowRenderer_Render;

        _points = [];

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Render(float delta)
    {
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _points.Add(ImGui.GetMousePos().ToMaths());
        }
        else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            _points.Clear();
        }

        ImDrawListPtr drawListPtr = ImGui.GetForegroundDrawList();

        foreach (Vector2d point in _points)
        {
            drawListPtr.AddCircleFilled(point.ToSystem(), 2.5f, 0xFFFFFFFF);
        }

        NaiveBezier(drawListPtr);
        Bezier(drawListPtr);
    }

    private static void NaiveBezier(ImDrawListPtr drawListPtr)
    {
        if (_points.Count < 4)
        {
            return;
        }

        Vector2d p0 = _points[0];
        Vector2d p1 = _points[1];
        Vector2d p2 = _points[2];
        Vector2d p3 = _points[3];

        for (float t = 0; t <= 1.0; t += 0.0001f)
        {
            Vector2d point = (p0 * MathF.Pow(1 - t, 3))
                             + (p1 * 3 * MathF.Pow(1 - t, 2) * t)
                             + (p2 * 3 * (1 - t) * MathF.Pow(t, 2))
                             + (p3 * MathF.Pow(t, 3));

            drawListPtr.AddCircleFilled(point.ToSystem(), 1.0f, 0xFF0000FF);
        }
    }

    private static Vector2d RecursiveBezier(Vector2d[] controlPoints, float t)
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

    private static void Bezier(ImDrawListPtr drawListPtr)
    {
        if (_points.Count == 0)
        {
            return;
        }

        for (float t = 0; t <= 1.0; t += 0.0001f)
        {
            Vector2d point = RecursiveBezier([.. _points], t);

            drawListPtr.AddCircleFilled(point.ToSystem(), 1.0f, 0xFF00FF00);
        }
    }
}