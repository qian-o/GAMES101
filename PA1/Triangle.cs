namespace PA1;

/// <summary>
/// CCW vertex assembly triangle.
/// </summary>
/// <param name="a">a</param>
/// <param name="b">b</param>
/// <param name="c">c</param>
public struct Triangle(Vertex a, Vertex b, Vertex c)
{
    public Vertex A = a;

    public Vertex B = b;

    public Vertex C = c;
}
