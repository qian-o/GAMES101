namespace PA.Graphics;

public struct Pixel(Color color, double depth)
{
    public Color Color = color;

    public double Depth = depth;
}
