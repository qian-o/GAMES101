namespace Maths;

public struct Matrix4X4(Vector4D row1, Vector4D row2, Vector4D row3, Vector4D row4)
{
    public double M11 = row1.X;

    public double M12 = row1.Y;

    public double M13 = row1.Z;

    public double M14 = row1.W;

    public double M21 = row2.X;

    public double M22 = row2.Y;

    public double M23 = row2.Z;

    public double M24 = row2.W;

    public double M31 = row3.X;

    public double M32 = row3.Y;

    public double M33 = row3.Z;

    public double M34 = row3.W;

    public double M41 = row4.X;

    public double M42 = row4.Y;

    public double M43 = row4.Z;

    public double M44 = row4.W;

    public readonly Vector4D Row1 => new(M11, M12, M13, M14);

    public readonly Vector4D Row2 => new(M21, M22, M23, M24);

    public readonly Vector4D Row3 => new(M31, M32, M33, M34);

    public readonly Vector4D Row4 => new(M41, M42, M43, M44);

    public readonly Vector4D Column1 => new(M11, M21, M31, M41);

    public readonly Vector4D Column2 => new(M12, M22, M32, M42);

    public readonly Vector4D Column3 => new(M13, M23, M33, M43);

    public readonly Vector4D Column4 => new(M14, M24, M34, M44);

    public static Matrix4X4 operator +(Matrix4X4 left, Matrix4X4 right)
    {
        Vector4D row1 = left.Row1 + right.Row1;
        Vector4D row2 = left.Row2 + right.Row2;
        Vector4D row3 = left.Row3 + right.Row3;
        Vector4D row4 = left.Row4 + right.Row4;

        return new(row1, row2, row3, row4);
    }

    public static Matrix4X4 operator -(Matrix4X4 left, Matrix4X4 right)
    {
        Vector4D row1 = left.Row1 - right.Row1;
        Vector4D row2 = left.Row2 - right.Row2;
        Vector4D row3 = left.Row3 - right.Row3;
        Vector4D row4 = left.Row4 - right.Row4;

        return new(row1, row2, row3, row4);
    }

    public static Matrix4X4 operator *(Matrix4X4 left, Matrix4X4 right)
    {
        Vector4D row1 = new(Vector4D.Dot(left.Row1, right.Column1), Vector4D.Dot(left.Row1, right.Column2), Vector4D.Dot(left.Row1, right.Column3), Vector4D.Dot(left.Row1, right.Column4));
        Vector4D row2 = new(Vector4D.Dot(left.Row2, right.Column1), Vector4D.Dot(left.Row2, right.Column2), Vector4D.Dot(left.Row2, right.Column3), Vector4D.Dot(left.Row2, right.Column4));
        Vector4D row3 = new(Vector4D.Dot(left.Row3, right.Column1), Vector4D.Dot(left.Row3, right.Column2), Vector4D.Dot(left.Row3, right.Column3), Vector4D.Dot(left.Row3, right.Column4));
        Vector4D row4 = new(Vector4D.Dot(left.Row4, right.Column1), Vector4D.Dot(left.Row4, right.Column2), Vector4D.Dot(left.Row4, right.Column3), Vector4D.Dot(left.Row4, right.Column4));

        return new(row1, row2, row3, row4);
    }

    public static Matrix4X4 operator *(Matrix4X4 matrix, double scalar)
    {
        Vector4D row1 = matrix.Row1 * scalar;
        Vector4D row2 = matrix.Row2 * scalar;
        Vector4D row3 = matrix.Row3 * scalar;
        Vector4D row4 = matrix.Row4 * scalar;

        return new(row1, row2, row3, row4);
    }

    public static Matrix4X4 operator *(double scalar, Matrix4X4 matrix)
    {
        return matrix * scalar;
    }

    public static Matrix4X4 operator /(Matrix4X4 matrix, double scalar)
    {
        Vector4D row1 = matrix.Row1 / scalar;
        Vector4D row2 = matrix.Row2 / scalar;
        Vector4D row3 = matrix.Row3 / scalar;
        Vector4D row4 = matrix.Row4 / scalar;

        return new(row1, row2, row3, row4);
    }
}
