using Maths;
using PA.Graphics;
using Silk.NET.OpenGL;
using Vertex = PA.Graphics.Vertex;

namespace PA2;

public unsafe class Rasterizer(Window window, SampleCount sampleCount = SampleCount.SampleCount1)
{
    #region Structs
    private struct TriangleInfo
    {
        public Triangle Triangle;

        public Vector2d A;

        public Vector2d B;

        public Vector2d C;

        public Box2d Box;
    }
    #endregion

    private readonly GL _gl = window.GL;
    private readonly SampleCount _sampleCount = sampleCount;
    private readonly Dictionary<int, Vertex[]> bufferVertexes = [];
    private readonly Dictionary<int, int[]> bufferIndices = [];

    private int x;
    private int y;
    private int width;
    private int height;
    private Matrix4x4d viewport;
    private FrameBuffer? frameBuffer;
    private Matrix4x4d transform;

    public bool FlipY { get; set; } = true;

    public bool CCW { get; set; } = true;

    public Matrix4x4d Model { get; set; }

    public Matrix4x4d View { get; set; }

    public Matrix4x4d Projection { get; set; }

    public FrameBuffer? FrameBuffer => frameBuffer;

    public int CreateVertexBuffer(Vertex[] vertexes)
    {
        int id = bufferVertexes.Count;
        bufferVertexes.Add(id, vertexes);

        return id;
    }

    public int CreateIndexBuffer(int[] indices)
    {
        int id = bufferIndices.Count;
        bufferIndices.Add(id, indices);

        return id;
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        if (this.x != x || this.y != y || this.width != width || this.height != height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            viewport = Matrix4x4d.CreateViewport(x, y, width, height, 1, -1);

            frameBuffer?.Dispose();
            frameBuffer = new FrameBuffer(_gl, width, height, _sampleCount);
        }
    }

    public void Clear()
    {
        if (frameBuffer is null)
        {
            return;
        }

        frameBuffer.Clear();
    }

    public void Render(int vertexBufferId, int indexBufferId)
    {
        if (frameBuffer is null
            || !bufferVertexes.TryGetValue(vertexBufferId, out Vertex[]? vertexes)
            || !bufferIndices.TryGetValue(indexBufferId, out int[]? indices))
        {
            return;
        }

        Triangle[] triangles = new Triangle[indices.Length / 3];

        for (int i = 0; i < indices.Length; i += 3)
        {
            Vertex a = vertexes[indices[i]];
            Vertex b = vertexes[indices[i + 1]];
            Vertex c = vertexes[indices[i + 2]];

            triangles[i / 3] = new Triangle(a, b, c);
        }

        transform = viewport * Projection * View * Model;

        TriangleInfo[] triangleInfos = new TriangleInfo[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            Triangle triangle = triangles[i];

            Vector2d a = (transform * triangle.A.Position).XY();
            Vector2d b = (transform * triangle.B.Position).XY();
            Vector2d c = (transform * triangle.C.Position).XY();

            Box2d box = Box2d.FromPoints(a, b, c);

            triangleInfos[i] = new TriangleInfo
            {
                Triangle = triangle,
                A = a,
                B = b,
                C = c,
                Box = box
            };
        }

        ParallelHelper.Foreach(frameBuffer.Pixels, (pixel) =>
        {
            foreach (TriangleInfo triangleInfo in triangleInfos)
            {
                RasterizeTriangle(pixel, triangleInfo);
            }
        });

        frameBuffer.Present();
    }

    private void RasterizeTriangle(Pixel pixel, TriangleInfo triangleInfo)
    {
        if (!triangleInfo.Box.Contains(pixel.X, pixel.Y))
        {
            return;
        }

        Triangle triangle = triangleInfo.Triangle;
        Vector2d a = triangleInfo.A;
        Vector2d b = triangleInfo.B;
        Vector2d c = triangleInfo.C;

        for (int sample = 0; sample < frameBuffer!.Samples; sample++)
        {
            Vector2d offset = frameBuffer.Patterns[sample];

            float x = pixel.X + offset.X;
            float y = pixel.Y + offset.Y;

            if (IsPointInTriangle([a, b, c], x, y, out Vector3d abg))
            {
                Vertex vertex = Vertex.Interpolate(triangle.A, triangle.B, triangle.C, abg);

                float depth = vertex.Position.Z;

                if (depth > frameBuffer[pixel, sample].Depth)
                {
                    frameBuffer[pixel, sample] = new Fragment(vertex.Color, depth);
                }
            }
        }
    }

    private bool IsPointInTriangle(Vector2d[] vectors, float x, float y, out Vector3d abg)
    {
        Vector2d center = new(x, y);

        Vector2d ab = vectors[1] - vectors[0];
        Vector2d bc = vectors[2] - vectors[1];
        Vector2d ca = vectors[0] - vectors[2];

        Vector2d ap = center - vectors[0];
        Vector2d bp = center - vectors[1];
        Vector2d cp = center - vectors[2];

        float abp = Vector2d.Cross(ab, ap);
        float bcp = Vector2d.Cross(bc, bp);
        float cap = Vector2d.Cross(ca, cp);

        float area = Math.Abs(Vector2d.Cross(ab, ca));

        bool isHit = CCW ? abp >= 0 && bcp >= 0 && cap >= 0 : abp <= 0 && bcp <= 0 && cap <= 0;

        abg = isHit ? new(bcp / area, cap / area, abp / area) : default;

        return isHit;
    }
}
