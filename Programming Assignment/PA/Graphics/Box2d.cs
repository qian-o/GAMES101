using Maths;

namespace PA.Graphics;

public struct Box2d(float minX, float minY, float maxX, float maxY)
{
    public float MinX = minX;

    public float MinY = minY;

    public float MaxX = maxX;

    public float MaxY = maxY;

    public static Box2d operator +(Box2d left, Box2d right)
    {
        float minX = Math.Min(left.MinX, right.MinX);
        float minY = Math.Min(left.MinY, right.MinY);
        float maxX = Math.Max(left.MaxX, right.MaxX);
        float maxY = Math.Max(left.MaxY, right.MaxY);

        return new Box2d(minX, minY, maxX, maxY);
    }

    public readonly bool Contains(float x, float y)
    {
        return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
    }

    public static Box2d FromPoints(params Vector2d[] points)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector2d point in points)
        {
            if (point.X < minX)
            {
                minX = point.X;
            }

            if (point.Y < minY)
            {
                minY = point.Y;
            }

            if (point.X > maxX)
            {
                maxX = point.X;
            }

            if (point.Y > maxY)
            {
                maxY = point.Y;
            }
        }

        minX = MathF.Floor(minX);
        minY = MathF.Floor(minY);
        maxX = MathF.Ceiling(maxX);
        maxY = MathF.Ceiling(maxY);

        return new Box2d(minX, minY, maxX, maxY);
    }
}
