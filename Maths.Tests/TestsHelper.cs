using System.Numerics;

namespace Maths.Tests;

public class TestsHelper
{
    public static void AssertEqual(Vector2D expected, Vector2 actual, double delta = 0.0001)
    {
        Assert.AreEqual(expected.X, actual.X, delta);
        Assert.AreEqual(expected.Y, actual.Y, delta);
    }

    public static void AssertEqual(Vector3D expected, Vector3 actual, double delta = 0.0001)
    {
        Assert.AreEqual(expected.X, actual.X, delta);
        Assert.AreEqual(expected.Y, actual.Y, delta);
        Assert.AreEqual(expected.Z, actual.Z, delta);
    }

    public static void AssertEqual(Vector4D expected, Vector4 actual, double delta = 0.0001)
    {
        Assert.AreEqual(expected.X, actual.X, delta);
        Assert.AreEqual(expected.Y, actual.Y, delta);
        Assert.AreEqual(expected.Z, actual.Z, delta);
        Assert.AreEqual(expected.W, actual.W, delta);
    }

    public static void AssertEqual(Matrix3X3 expected, Matrix3X3 actual, double delta = 0.0001)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Assert.AreEqual(expected[i, j], actual[i, j], delta);
            }
        }
    }

    public static void AssertEqual(Matrix4X4 expected, Matrix4X4 actual, double delta = 0.0001)
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
