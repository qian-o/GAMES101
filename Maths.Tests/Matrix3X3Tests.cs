using System.Numerics;

namespace Maths.Tests;

[TestClass]
public class Matrix3X3Tests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(90)]
    [DataRow(180)]
    [DataRow(270)]
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
}
