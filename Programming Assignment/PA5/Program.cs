using PA.Graphics;

namespace PA5;

internal class Program
{
    private static WindowRenderer _windowRenderer = null!;

    static void Main(string[] _)
    {
        _windowRenderer = new("PA 5");

        _windowRenderer.Run();

        _windowRenderer.Dispose();
    }
}
