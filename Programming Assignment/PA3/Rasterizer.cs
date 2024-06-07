﻿using Maths;
using PA.Graphics;
using Silk.NET.SDL;
using Color = PA.Graphics.Color;
using Vertex = PA.Graphics.Vertex;

namespace PA3;

public unsafe class Rasterizer(WindowRenderer windowRenderer, SampleCount sampleCount = SampleCount.SampleCount1)
{
    private readonly Sdl _sdl = windowRenderer.Sdl;
    private readonly Renderer* _renderer = windowRenderer.Renderer;
    private readonly SampleCount _sampleCount = sampleCount;
    private readonly Dictionary<int, Vertex[]> bufferVertexes = [];
    private readonly Dictionary<int, uint[]> bufferIndices = [];

    private int x;
    private int y;
    private int width;
    private int height;
    private Matrix4x4d viewport;
    private FrameBuffer? frameBuffer;

    public bool FlipY { get; set; } = true;

    public bool CCW { get; set; } = true;

    public Matrix4x4d Model { get; set; }

    public Matrix4x4d View { get; set; }

    public Matrix4x4d Projection { get; set; }

    public Func<Vertex, Color>? Frag { get; set; }

    public int CreateVertexBuffer(Vertex[] vertexes)
    {
        int id = bufferVertexes.Count;
        bufferVertexes.Add(id, vertexes);

        return id;
    }

    public int CreateIndexBuffer(uint[] indices)
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
            frameBuffer = new FrameBuffer(_sdl, _renderer, width, height, _sampleCount);
        }
    }

    public void Clear()
    {
        if (frameBuffer is null)
        {
            return;
        }

        frameBuffer.Clear(Colors.Black);
    }

    public void Render(int vertexBufferId, int indexBufferId)
    {
        if (frameBuffer is null
            || !bufferVertexes.TryGetValue(vertexBufferId, out Vertex[]? vertexes)
            || !bufferIndices.TryGetValue(indexBufferId, out uint[]? indices))
        {
            return;
        }

        Triangle[] triangles = new Triangle[indices.Length / 3];

        for (int i = 0; i < indices.Length; i += 3)
        {
            Vertex a = vertexes[indices[i]];
            a.Color = Color.FromRgb(148, 121, 92);

            Vertex b = vertexes[indices[i + 1]];
            b.Color = Color.FromRgb(148, 121, 92);

            Vertex c = vertexes[indices[i + 2]];
            c.Color = Color.FromRgb(148, 121, 92);

            triangles[i / 3] = new Triangle(a, b, c);
        }

        for (int i = 0; i < triangles.Length; i++)
        {
            Triangle triangle = triangles[i];

            Matrix4x4d mv = View * Model;
            Matrix4x4d mvpv = viewport * Projection * mv;
            Matrix4x4d.Invert(mv, out Matrix4x4d invTrans);
            invTrans = Matrix4x4d.Transpose(invTrans);

            Triangle viewTriangle = triangles[i];
            viewTriangle.A.Position = mv * viewTriangle.A.Position;
            viewTriangle.B.Position = mv * viewTriangle.B.Position;
            viewTriangle.C.Position = mv * viewTriangle.C.Position;
            viewTriangle.A.Normal = invTrans * viewTriangle.A.Normal;
            viewTriangle.B.Normal = invTrans * viewTriangle.B.Normal;
            viewTriangle.C.Normal = invTrans * viewTriangle.C.Normal;

            Triangle viewportTriangle = triangles[i];
            viewportTriangle.A.Position = mvpv * viewportTriangle.A.Position;
            viewportTriangle.B.Position = mvpv * viewportTriangle.B.Position;
            viewportTriangle.C.Position = mvpv * viewportTriangle.C.Position;

            Box2d box = Box2d.FromPoints(viewportTriangle.A.Position.XY(), viewportTriangle.B.Position.XY(), viewportTriangle.C.Position.XY());

            frameBuffer.ProcessingPixelsByBox(box, (pixel) =>
            {
                RasterizeTriangle(pixel, viewTriangle, viewportTriangle);
            });
        }

        frameBuffer.Present(x, y, FlipY);
    }

    private void RasterizeTriangle(Pixel pixel, Triangle viewTriangle, Triangle viewportTriangle)
    {
        Vector2d[] pattern = frameBuffer!.Pattern;

        Vector2d a = viewportTriangle.A.Position.XY();
        Vector2d b = viewportTriangle.B.Position.XY();
        Vector2d c = viewportTriangle.C.Position.XY();

        for (int index = 0; index < pattern.Length; index++)
        {
            Vector2d offset = pattern[index];

            double x = pixel.X + offset.X;
            double y = pixel.Y + offset.Y;

            if (IsPointInTriangle([a, b, c], x, y, out Vector3d abg))
            {
                Vertex vertex = Vertex.Interpolate(viewTriangle.A, viewTriangle.B, viewTriangle.C, abg);

                double depth = vertex.Position.Z;

                if (depth >= frameBuffer.GetDepth(pixel, index))
                {
                    Color color = Frag?.Invoke(vertex) ?? Colors.White;

                    frameBuffer.SetColor(pixel, index, color);
                    frameBuffer.SetDepth(pixel, index, depth);
                }
            }
        }
    }

    private bool IsPointInTriangle(Vector2d[] vectors, double x, double y, out Vector3d abg)
    {
        Vector2d center = new(x, y);

        Vector2d ab = vectors[1] - vectors[0];
        Vector2d bc = vectors[2] - vectors[1];
        Vector2d ca = vectors[0] - vectors[2];

        Vector2d ap = center - vectors[0];
        Vector2d bp = center - vectors[1];
        Vector2d cp = center - vectors[2];

        double abp = Vector2d.Cross(ab, ap);
        double bcp = Vector2d.Cross(bc, bp);
        double cap = Vector2d.Cross(ca, cp);

        double area = Math.Abs(Vector2d.Cross(ab, ca));

        bool isHit = CCW ? abp >= 0 && bcp >= 0 && cap >= 0 : abp <= 0 && bcp <= 0 && cap <= 0;

        abg = isHit ? new(bcp / area, cap / area, abp / area) : default;

        return isHit;
    }
}