using Maths;
using PA.Graphics;
using Silk.NET.OpenGL;

namespace PA7;

internal class Scene(GL gl, int width, int height) : IDisposable
{
    private readonly GL _gl = gl;

    private BVHAccel? bvh;
    private int currentWidth = -1;
    private int currentHeight = -1;
    private SampleCount currentSampleCount = SampleCount.SampleCount1;
    private FrameBuffer? frameBuffer = null;

    public int Width { get; set; } = width;

    public int Height { get; set; } = height;

    public Camera Camera { get; set; } = new(new Vector3d(278.0f, 273.0f, -800.0f), Angle.FromDegrees(45.0f));

    public Vector3d BackgroundColor { get; set; } = new(0.2f, 0.7f, 0.8f);

    public float Epsilon { get; set; } = 0.00001f;

    public int MaxDepth { get; set; } = 5;

    public SampleCount SampleCount { get; set; } = SampleCount.SampleCount1;

    public List<Shape> Shapes { get; } = [];

    public FrameBuffer FrameBuffer => TryGetFrameBuffer();

    public void BuildBVH(SplitMethod splitMethod = SplitMethod.NAIVE)
    {
        bvh?.Dispose();

        bvh = new([.. Shapes], splitMethod);
    }

    public Intersection GetIntersection(Ray ray)
    {
        if (bvh is null)
        {
            throw new InvalidOperationException("BVH acceleration structure is not built.");
        }

        return bvh.Intersect(ray);
    }

    public void Dispose()
    {
        bvh?.Dispose();
    }

    private FrameBuffer TryGetFrameBuffer()
    {
        if (currentWidth != Width || currentHeight != Height || currentSampleCount != SampleCount)
        {
            currentWidth = Width;
            currentHeight = Height;
            currentSampleCount = SampleCount;

            frameBuffer?.Dispose();
            frameBuffer = new FrameBuffer(_gl, Width, Height, SampleCount);
        }

        return frameBuffer!;
    }
}