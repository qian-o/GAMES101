﻿using System.Numerics;

namespace Maths.Tests;

[TestClass]
public class Matrix4X4Tests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(90)]
    [DataRow(180)]
    [DataRow(360)]
    public void GenerateRotationXMatrix(double degrees)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3D point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4X4 matrix1 = Matrix4X4.CreateRotationX(angle);
        Matrix4x4 matrix2 = Matrix4x4.CreateRotationX((float)angle.Radians);

        point1 = matrix1 * point1;
        point2 = Vector3.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(90)]
    [DataRow(180)]
    [DataRow(360)]
    public void GenerateRotationYMatrix(double degrees)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3D point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4X4 matrix1 = Matrix4X4.CreateRotationY(angle);
        Matrix4x4 matrix2 = Matrix4x4.CreateRotationY((float)angle.Radians);

        point1 = matrix1 * point1;
        point2 = Vector3.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(90)]
    [DataRow(180)]
    [DataRow(360)]
    public void GenerateRotationZMatrix(double degrees)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3D point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4X4 matrix1 = Matrix4X4.CreateRotationZ(angle);
        Matrix4x4 matrix2 = Matrix4x4.CreateRotationZ((float)angle.Radians);

        point1 = matrix1 * point1;
        point2 = Vector3.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(1, 1, 1)]
    [DataRow(0.5, 0.5, 0.5)]
    [DataRow(0.4, 0.3, 0.2)]
    [DataRow(0, 0, 0)]
    [DataRow(-0.4, -0.3, -0.2)]
    [DataRow(-0.5, -0.5, -0.5)]
    [DataRow(-1, -1, -1)]
    public void GenerateScaleMatrix(double x, double y, double z)
    {
        Vector3D point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4X4 matrix1 = Matrix4X4.CreateScale(new Vector3D(x, y, z));
        Matrix4x4 matrix2 = Matrix4x4.CreateScale(new Vector3((float)x, (float)y, (float)z));

        point1 = matrix1 * point1;
        point2 = Vector3.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(1, 1, 1)]
    [DataRow(0.5, 0.5, 0.5)]
    [DataRow(0.4, 0.3, 0.2)]
    [DataRow(0, 0, 0)]
    [DataRow(-0.4, -0.3, -0.2)]
    [DataRow(-0.5, -0.5, -0.5)]
    [DataRow(-1, -1, -1)]
    public void GenerateTranslationMatrix(double x, double y, double z)
    {
        Vector3D point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4X4 matrix1 = Matrix4X4.CreateTranslation(new Vector3D(x, y, z));
        Matrix4x4 matrix2 = Matrix4x4.CreateTranslation(new Vector3((float)x, (float)y, (float)z));

        point1 = matrix1 * point1;
        point2 = Vector3.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(0, 1, 1, 1)]
    [DataRow(90, 0.5, 0.5, 0.5)]
    [DataRow(180, 0.4, 0.3, 0.2)]
    [DataRow(360, 0, 0, 0)]
    [DataRow(180, -0.4, -0.3, -0.2)]
    [DataRow(90, -0.5, -0.5, -0.5)]
    [DataRow(0, -1, -1, -1)]
    public void GenerateMultiplesMatrix(double degrees, double x, double y, double z)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3D point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4X4 rx1 = Matrix4X4.CreateRotationX(angle);
        Matrix4X4 ry1 = Matrix4X4.CreateRotationY(angle);
        Matrix4X4 rz1 = Matrix4X4.CreateRotationZ(angle);
        Matrix4X4 s1 = Matrix4X4.CreateScale(new Vector3D(x, y, z));
        Matrix4X4 t1 = Matrix4X4.CreateTranslation(new Vector3D(x, y, z));

        Matrix4x4 rx2 = Matrix4x4.CreateRotationX((float)angle.Radians);
        Matrix4x4 ry2 = Matrix4x4.CreateRotationY((float)angle.Radians);
        Matrix4x4 rz2 = Matrix4x4.CreateRotationZ((float)angle.Radians);
        Matrix4x4 s2 = Matrix4x4.CreateScale(new Vector3((float)x, (float)y, (float)z));
        Matrix4x4 t2 = Matrix4x4.CreateTranslation(new Vector3((float)x, (float)y, (float)z));

        point1 = t1 * s1 * rz1 * ry1 * rx1 * point1;
        point2 = Vector3.Transform(point2, rx2 * ry2 * rz2 * s2 * t2);

        TestsHelper.AssertEqual(point1, point2);
    }
}
