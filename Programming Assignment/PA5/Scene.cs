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
    private SceneProperties properties = new(width, height);

    public ref int Width => ref properties.Width;

    public ref int Height => ref properties.Height;

    public ref Angle Fov => ref properties.Fov;

    public ref float Near => ref properties.Near;

    public ref float Far => ref properties.Far;

    public ref Camera Camera => ref properties.Camera;

    public ref Vector3d BackgroundColor => ref properties.BackgroundColor;

    public ref int MaxDepth => ref properties.MaxDepth;

    public ref float Epsilon => ref properties.Epsilon;

    public List<Material> Materials { get; } = [];

    public List<Geometry> Objects { get; } = [];

    public List<Light> Lights { get; } = [];

    public FrameBuffer FrameBuffer => TryGetFrameBuffer();

    public SceneProperties SceneProperties => properties;

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

internal struct SceneProperties(int width,
                                int height)
{
    public int Width = width;

    public int Height = height;

    public Angle Fov = Angle.FromDegrees(45.0f);

    public float Near = 0.1f;

    public float Far = 100.0f;

    public Camera Camera = new();

    public Vector3d BackgroundColor = new(0.2f, 0.7f, 0.8f);

    public int MaxDepth = 5;

    public float Epsilon = 0.0001f;

    public readonly Matrix4x4d View => Matrix4x4d.CreateLookAt(Camera.Position, Camera.Target, Camera.Up);

    public readonly Matrix4x4d Projection => Matrix4x4d.CreatePerspectiveFieldOfView(Fov, Width / (float)Height, Near, Far);
}
