using ImGuiNET;
using PA.Graphics;

namespace PA6;


internal class Program
{
    private static WindowRenderer _windowRenderer = null!;

    static void Main(string[] _)
    {
        _windowRenderer = new("PA 6");
        _windowRenderer.Render += WindowRenderer_Render;

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }

    private static void WindowRenderer_Render(float obj)
    {
        ImGui.Begin("PA 6");
        {
        }
        ImGui.End();
    }
}
