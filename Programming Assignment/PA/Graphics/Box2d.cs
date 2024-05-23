﻿using Maths;

namespace PA.Graphics;

public struct Box2d(double minX, double minY, double maxX, double maxY)
{
    public double MinX = minX;

    public double MinY = minY;

    public double MaxX = maxX;

    public double MaxY = maxY;

    public static Box2d FromPoints(params Vector2d[] points)
    {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

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

        minX = Math.Floor(minX);
        minY = Math.Floor(minY);
        maxX = Math.Ceiling(maxX);
        maxY = Math.Ceiling(maxY);

        return new Box2d(minX, minY, maxX, maxY);
    }
}