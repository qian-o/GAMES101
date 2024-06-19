using Maths;
using PA.Graphics;
using Silk.NET.OpenGL;

namespace PA5;

internal class Scene(GL gl, int width, int height, SampleCount sampleCount = SampleCount.SampleCount1)
{
    private readonly GL _gl = gl;
    private readonly SampleCount _sampleCount = sampleCount;

    private int currentWidth = -1;
    private int currentHeight = -1;
    private FrameBuffer? frameBuffer = null;

    public int Width { get; set; } = width;

    public int Height { get; set; } = height;

    public Camera Camera { get; set; } = new Camera(Vector3d.Zero, Angle.FromDegrees(45.0f));

    public Vector3d BackgroundColor { get; set; } = new Vector3d(0.2f, 0.7f, 0.8f);

    public int MaxDepth { get; set; } = 5;

    public float Epsilon { get; set; } = 0.0001f;

    public List<Material> Materials { get; } = [];

    public List<Geometry> Objects { get; } = [];

    public List<Light> Lights { get; } = [];

    public FrameBuffer FrameBuffer => TryGetFrameBuffer();

    public SceneProperties GetProperties()
    {
        return new SceneProperties
        {
            Width = Width,
            Height = Height,
            Camera = Camera,
            BackgroundColor = BackgroundColor,
            MaxDepth = MaxDepth,
            Epsilon = Epsilon,
            MaterialsLength = Materials.Count,
            ObjectsLength = Objects.Count,
            LightsLength = Lights.Count
        };
    }

    private FrameBuffer TryGetFrameBuffer()
    {
        if (currentWidth != Width || currentHeight != Height)
        {
            currentWidth = Width;
            currentHeight = Height;

            frameBuffer?.Dispose();
            frameBuffer = new FrameBuffer(_gl, Width, Height, _sampleCount);
        }

        return frameBuffer!;
    }
}

internal struct SceneProperties
{
    public int Width;

    public int Height;

    public Camera Camera;

    public Vector3d BackgroundColor;

    public int MaxDepth;

    public float Epsilon;

    public int MaterialsLength;

    public int ObjectsLength;

    public int LightsLength;
}
