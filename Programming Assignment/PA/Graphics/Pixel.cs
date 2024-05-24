using System.Runtime.InteropServices;

namespace PA.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct Pixel(int x, int y)
{
    public int X = x;

    public int Y = y;
}
