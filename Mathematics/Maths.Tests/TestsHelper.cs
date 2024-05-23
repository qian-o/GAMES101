using System.Numerics;

namespace Maths.Tests;

public class TestsHelper
{
    public static void AssertEqual(Vector2d expected, Vector2 actual, double delta = 0.0001)
    {
        Assert.AreEqual(expected.X, actual.X, delta);
        Assert.AreEqual(expected.Y, actual.Y, delta);
    }

    public static void AssertEqual(Vector3d expected, Vector3 actual, double delta = 0.0001)
    {
        Assert.AreEqual(expected.X, actual.X, delta);
        Assert.AreEqual(expected.Y, actual.Y, delta);
        Assert.AreEqual(expected.Z, actual.Z, delta);
    }

    public static void AssertEqual(Vector4d expected, Vector4 actual, double delta = 0.0001)
    {
        Assert.AreEqual(expected.X, actual.X, delta);
        Assert.AreEqual(expected.Y, actual.Y, delta);
        Assert.AreEqual(expected.Z, actual.Z, delta);
        Assert.AreEqual(expected.W, actual.W, delta);
    }

    public static void AssertEqual(Matrix3x3d expected, Matrix3x3d actual, double delta = 0.0001)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Assert.AreEqual(expected[i, j], actual[i, j], delta);
            }
        }
    }

    public static void AssertEqual(Matrix4x4d expected, Matrix4x4d actual, double delta = 0.0001)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.AreEqual(expected[i, j], actual[i, j], delta);
            }
        }
    }
}
