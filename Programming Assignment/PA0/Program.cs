﻿using System.Numerics;
using Maths;

namespace PA0;

internal class Program
{
    /// <summary>
    /// 给定一个点P=(2,1),将该点绕原点先逆时针旋转45◦，再平移(1,2),计算出变换后点的坐标（要求用齐次坐标进行计算）。
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] _)
    {
        // Maths
        {
            Vector2d p = new(2, 1);

            Matrix3x3d rotate = Matrix3x3d.CreateRotation(Angle.FromDegrees(45));
            Matrix3x3d translate = Matrix3x3d.CreateTranslation(new Vector2d(1, 2));

            p = translate * rotate * p;

            Console.WriteLine("Maths: " + p);
        }

        // System.Numerics
        {
            Vector2 p = new(2, 1);

            Matrix3x2 rotate = Matrix3x2.CreateRotation(Angle.FromDegrees(45).Radians);
            Matrix3x2 translate = Matrix3x2.CreateTranslation(new Vector2(1, 2));

            p = Vector2.Transform(p, rotate * translate);

            Console.WriteLine("System.Numerics: " + p);
        }
    }
}
