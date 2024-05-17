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
}
