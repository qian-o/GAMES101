using Maths;
using PA.Graphics;
using Silk.NET.OpenGL;

namespace PA6;

internal class Scene(GL gl, int width, int height) : IDisposable
{
    private readonly GL _gl = gl;

    private BVHAccel? accel;
    private int currentWidth = -1;
    private int currentHeight = -1;
    private SampleCount currentSampleCount = SampleCount.SampleCount1;
    private FrameBuffer? frameBuffer = null;

    public int Width { get; set; } = width;

    public int Height { get; set; } = height;

    public Camera Camera { get; set; } = new(Vector3d.Zero, Angle.FromDegrees(45.0f));

    public Vector3d BackgroundColor { get; set; } = new(0.2f, 0.7f, 0.8f);

    public float Epsilon { get; set; } = 0.00001f;

    public int MaxDepth { get; set; } = 5;

    public SampleCount SampleCount { get; set; } = SampleCount.SampleCount1;

    public List<Geometry> Geometries { get; } = [];

    public List<Light> Lights { get; } = [];

    public FrameBuffer FrameBuffer => TryGetFrameBuffer();

    public void BuildBVH(SplitMethod splitMethod = SplitMethod.NAIVE)
    {
        accel?.Dispose();

        accel = new([.. Geometries], splitMethod);
    }

    public void Dispose()
    {
        accel?.Dispose();
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
