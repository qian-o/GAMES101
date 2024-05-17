using System.Numerics;

namespace Maths.Tests;

[TestClass]
public class Matrix3X3Tests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(90)]
    [DataRow(180)]
    [DataRow(360)]
    public void GenerateRotationMatrix(double degrees)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector2D point1 = new(1, 1);
        Vector2 point2 = new(1, 1);

        Matrix3X3 matrix1 = Matrix3X3.CreateRotation(angle);
        Matrix3x2 matrix2 = Matrix3x2.CreateRotation((float)angle.Radians);

        point1 = matrix1 * point1;
        point2 = Vector2.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(0.5, 0.5)]
    [DataRow(0.4, 0.3)]
    [DataRow(0, 0)]
    [DataRow(-0.4, -0.3)]
    [DataRow(-0.5, -0.5)]
    [DataRow(-1, -1)]
    public void GenerateScaleMatrix(double x, double y)
    {
        Vector2D point1 = new(1, 1);
        Vector2 point2 = new(1, 1);

        Matrix3X3 matrix1 = Matrix3X3.CreateScale(new Vector2D(x, y));
        Matrix3x2 matrix2 = Matrix3x2.CreateScale(new Vector2((float)x, (float)y));

        point1 = matrix1 * point1;
        point2 = Vector2.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(0.5, 0.5)]
    [DataRow(0.4, 0.3)]
    [DataRow(0, 0)]
    [DataRow(-0.4, -0.3)]
    [DataRow(-0.5, -0.5)]
    [DataRow(-1, -1)]
    public void GenerateTranslationMatrix(double x, double y)
    {
        Vector2D point1 = new(1, 1);
        Vector2 point2 = new(1, 1);

        Matrix3X3 matrix1 = Matrix3X3.CreateTranslation(new Vector2D(x, y));
        Matrix3x2 matrix2 = Matrix3x2.CreateTranslation(new Vector2((float)x, (float)y));

        point1 = matrix1 * point1;
        point2 = Vector2.Transform(point2, matrix2);

        TestsHelper.AssertEqual(point1, point2);
    }

    [TestMethod]
    [DataRow(0, 1, 1)]
    [DataRow(90, 0.5, 0.5)]
    [DataRow(180, 0.4, 0.3)]
    [DataRow(360, 0, 0)]
    [DataRow(180, -0.4, -0.3)]
    [DataRow(90, -0.5, -0.5)]
    [DataRow(0, -1, -1)]
    public void GenerateMultiplesMatrix(double degrees, double x, double y)
    {
        Angle angle = Angle.FromDegrees(degrees);

        Vector2D point1 = new(1, 1);
        Vector2 point2 = new(1, 1);

        Matrix3X3 r1 = Matrix3X3.CreateRotation(angle);
        Matrix3X3 s1 = Matrix3X3.CreateScale(new Vector2D(x, y));
        Matrix3X3 t1 = Matrix3X3.CreateTranslation(new Vector2D(x, y));
        Matrix3x2 r2 = Matrix3x2.CreateRotation((float)angle.Radians);
        Matrix3x2 s2 = Matrix3x2.CreateScale(new Vector2((float)x, (float)y));
        Matrix3x2 t2 = Matrix3x2.CreateTranslation(new Vector2((float)x, (float)y));

        point1 = t1 * s1 * r1 * point1;
        point2 = Vector2.Transform(point2, r2 * s2 * t2);

        TestsHelper.AssertEqual(point1, point2);
    }
}
