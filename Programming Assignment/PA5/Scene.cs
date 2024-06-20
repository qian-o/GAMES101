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

    #region Properties
    private FrameBuffer? frameBuffer = null;
    private SceneProperties properties = new(width, height);

    public FrameBuffer FrameBuffer => TryGetFrameBuffer();

    public SceneProperties SceneProperties => properties;

    public ref int Width => ref properties.Width;

    public ref int Height => ref properties.Height;

    public ref Camera Camera => ref properties.Camera;

    public ref Vector3d BackgroundColor => ref properties.BackgroundColor;

    public ref float Epsilon => ref properties.Epsilon;

    public List<Material> Materials { get; } = [];

    public List<Geometry> Objects { get; } = [];

    public List<Light> Lights { get; } = [];
    #endregion

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

internal struct SceneProperties(int width, int height)
{
    public int Width = width;

    public int Height = height;

    public Camera Camera = new(Vector3d.Zero, Angle.FromDegrees(45.0f));

    public Vector3d BackgroundColor = new(0.2f, 0.7f, 0.8f);

    public float Epsilon = 0.00001f;
}
