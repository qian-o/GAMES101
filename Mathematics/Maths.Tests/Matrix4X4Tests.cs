using System.Numerics;

namespace Maths.Tests;

[TestClass]
public class Matrix4X4Tests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(90)]
    [DataRow(180)]
    [DataRow(360)]
    public void GenerateRotationXMatrix(float degrees)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3d point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4x4d matrix1 = Matrix4x4d.CreateRotationX(angle);
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
    public void GenerateRotationYMatrix(float degrees)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3d point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4x4d matrix1 = Matrix4x4d.CreateRotationY(angle);
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
    public void GenerateRotationZMatrix(float degrees)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3d point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4x4d matrix1 = Matrix4x4d.CreateRotationZ(angle);
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
    public void GenerateScaleMatrix(float x, float y, float z)
    {
        Vector3d point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4x4d matrix1 = Matrix4x4d.CreateScale(new Vector3d(x, y, z));
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
    public void GenerateTranslationMatrix(float x, float y, float z)
    {
        Vector3d point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4x4d matrix1 = Matrix4x4d.CreateTranslation(new Vector3d(x, y, z));
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
    public void GenerateMultiplesMatrix(float degrees, float x, float y, float z)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector3d point1 = new(1, 1, 1);
        Vector3 point2 = new(1, 1, 1);

        Matrix4x4d rx1 = Matrix4x4d.CreateRotationX(angle);
        Matrix4x4d ry1 = Matrix4x4d.CreateRotationY(angle);
        Matrix4x4d rz1 = Matrix4x4d.CreateRotationZ(angle);
        Matrix4x4d s1 = Matrix4x4d.CreateScale(new Vector3d(x, y, z));
        Matrix4x4d t1 = Matrix4x4d.CreateTranslation(new Vector3d(x, y, z));

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
