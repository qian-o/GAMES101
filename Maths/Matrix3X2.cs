using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Matrix3X2(Vector2D row1, Vector2D row2, Vector2D row3)
{
    public double M11 = row1.X;

    public double M12 = row1.Y;

    public double M21 = row2.X;

    public double M22 = row2.Y;

    public double M31 = row3.X;

    public double M32 = row3.Y;

    public readonly unsafe double this[int row, int column]
    {
        get
        {
            fixed (double* ptr = &M11)
            {
                return *(ptr + (row * 2) + column);
            }
        }
    }

    public readonly Vector2D Row1 => new(M11, M12);

    public readonly Vector2D Row2 => new(M21, M22);

    public readonly Vector2D Row3 => new(M31, M32);

    public readonly Vector3D Column1 => new(M11, M21, M31);

    public readonly Vector3D Column2 => new(M12, M22, M32);

    public static Matrix3X2 operator +(Matrix3X2 left, Matrix3X2 right)
    {
        Vector2D row1 = left.Row1 + right.Row1;
        Vector2D row2 = left.Row2 + right.Row2;
        Vector2D row3 = left.Row3 + right.Row3;

        return new(row1, row2, row3);
    }

    public static Matrix3X2 operator -(Matrix3X2 left, Matrix3X2 right)
    {
        Vector2D row1 = left.Row1 - right.Row1;
        Vector2D row2 = left.Row2 - right.Row2;
        Vector2D row3 = left.Row3 - right.Row3;

        return new(row1, row2, row3);
    }

    public static Matrix3X2 operator *(Matrix3X2 left, Matrix3X2 right)
    {
        Vector2D row1 = new(Vector2D.Dot(left.Row1, new Vector2D(right.M11, right.M21)), Vector2D.Dot(left.Row1, new Vector2D(right.M12, right.M22)));
        Vector2D row2 = new(Vector2D.Dot(left.Row2, new Vector2D(right.M11, right.M21)), Vector2D.Dot(left.Row2, new Vector2D(right.M12, right.M22)));
        Vector2D row3 = new(Vector2D.Dot(left.Row3, new Vector2D(right.M11, right.M21)), Vector2D.Dot(left.Row3, new Vector2D(right.M12, right.M22)));
        row3 += right.Row3;

        return new(row1, row2, row3);
    }

    public static Matrix3X2 operator *(Matrix3X2 matrix, double scalar)
    {
        Vector2D row1 = matrix.Row1 * scalar;
        Vector2D row2 = matrix.Row2 * scalar;
        Vector2D row3 = matrix.Row3 * scalar;

        return new(row1, row2, row3);
    }

    public static Matrix3X2 operator *(double scalar, Matrix3X2 matrix)
    {
        return matrix * scalar;
    }

    public static Vector2D operator *(Matrix3X2 matrix, Vector2D vector)
    {
        return new Vector2D(Vector2D.Dot(matrix.Row1, vector), Vector2D.Dot(matrix.Row2, vector)) + matrix.Row3;
    }

    public static Matrix3X2 operator /(Matrix3X2 matrix, double scalar)
    {
        Vector2D row1 = matrix.Row1 / scalar;
        Vector2D row2 = matrix.Row2 / scalar;
        Vector2D row3 = matrix.Row3 / scalar;

        return new(row1, row2, row3);
    }

    public static Matrix3X2 CreateRotation(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new Vector2D(cos, sin), new Vector2D(-sin, cos), new Vector2D(0, 0));
    }

    public static Matrix3X2 CreateScale(Vector2D scale)
    {
        return new(scale, new Vector2D(0, 0), new Vector2D(0, 0));
    }

    public static Matrix3X2 CreateTranslation(Vector2D translation)
    {
        return new(new Vector2D(1, 0), new Vector2D(0, 1), translation);
    }
}
