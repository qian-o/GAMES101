using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Matrix3x3d(Vector3d row1, Vector3d row2, Vector3d row3) : IEquatable<Matrix3x3d>
{
    public static Matrix3x3d Identity => new(new(1, 0, 0), new(0, 1, 0), new(0, 0, 1));

    public double M11 = row1.X;

    public double M12 = row1.Y;

    public double M13 = row1.Z;

    public double M21 = row2.X;

    public double M22 = row2.Y;

    public double M23 = row2.Z;

    public double M31 = row3.X;

    public double M32 = row3.Y;

    public double M33 = row3.Z;

    public unsafe double this[int row, int column]
    {
        get
        {
            fixed (double* ptr = &M11)
            {
                return *(ptr + (row * 4) + column);
            }
        }
    }

    public readonly bool IsIdentity => this == Identity;

    public readonly Vector3d Row1 => new(M11, M12, M13);

    public readonly Vector3d Row2 => new(M21, M22, M23);

    public readonly Vector3d Row3 => new(M31, M32, M33);

    public readonly Vector3d Column1 => new(M11, M21, M31);

    public readonly Vector3d Column2 => new(M12, M22, M32);

    public readonly Vector3d Column3 => new(M13, M23, M33);

    public readonly bool Equals(Matrix3x3d other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Matrix3x3d d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(M11);
        hash.Add(M12);
        hash.Add(M13);

        hash.Add(M21);
        hash.Add(M22);
        hash.Add(M23);

        hash.Add(M31);
        hash.Add(M32);
        hash.Add(M33);

        return hash.ToHashCode();
    }

    public override readonly string ToString()
    {
        return $"M11: {M11}, M12: {M12}, M13: {M13}\n" +
               $"M21: {M21}, M22: {M22}, M23: {M23}\n" +
               $"M31: {M31}, M32: {M32}, M33: {M33}";
    }

    public static Matrix3x3d operator +(Matrix3x3d left, Matrix3x3d right)
    {
        Vector3d row1 = left.Row1 + right.Row1;
        Vector3d row2 = left.Row2 + right.Row2;
        Vector3d row3 = left.Row3 + right.Row3;

        return new(row1, row2, row3);
    }

    public static Matrix3x3d operator -(Matrix3x3d left, Matrix3x3d right)
    {
        Vector3d row1 = left.Row1 - right.Row1;
        Vector3d row2 = left.Row2 - right.Row2;
        Vector3d row3 = left.Row3 - right.Row3;

        return new(row1, row2, row3);
    }

    public static Vector2d operator *(Matrix3x3d matrix, Vector2d vector)
    {
        double x = Vector3d.Dot(matrix.Row1, new(vector.X, vector.Y, 1));
        double y = Vector3d.Dot(matrix.Row2, new(vector.X, vector.Y, 1));
        double w = Vector3d.Dot(matrix.Row3, new(vector.X, vector.Y, 1));

        return new Vector2d(x, y) / w;
    }

    public static Vector3d operator *(Matrix3x3d matrix, Vector3d vector)
    {
        double x = Vector3d.Dot(matrix.Row1, vector);
        double y = Vector3d.Dot(matrix.Row2, vector);
        double z = Vector3d.Dot(matrix.Row3, vector);

        return new(x, y, z);
    }

    public static Matrix3x3d operator *(Matrix3x3d left, Matrix3x3d right)
    {
        double m11 = Vector3d.Dot(left.Row1, right.Column1);
        double m12 = Vector3d.Dot(left.Row1, right.Column2);
        double m13 = Vector3d.Dot(left.Row1, right.Column3);

        double m21 = Vector3d.Dot(left.Row2, right.Column1);
        double m22 = Vector3d.Dot(left.Row2, right.Column2);
        double m23 = Vector3d.Dot(left.Row2, right.Column3);

        double m31 = Vector3d.Dot(left.Row3, right.Column1);
        double m32 = Vector3d.Dot(left.Row3, right.Column2);
        double m33 = Vector3d.Dot(left.Row3, right.Column3);

        return new(new(m11, m12, m13),
                   new(m21, m22, m23),
                   new(m31, m32, m33));
    }

    public static bool operator ==(Matrix3x3d left, Matrix3x3d right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Matrix3x3d left, Matrix3x3d right)
    {
        return !(left == right);
    }

    public static Matrix3x3d Transpose(Matrix3x3d matrix)
    {
        return new(matrix.Column1, matrix.Column2, matrix.Column3);
    }

    public static Matrix3x3d CreateRotation(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new(cos, -sin, 0),
                   new(sin, cos, 0),
                   new(0, 0, 1));
    }

    public static Matrix3x3d CreateScale(Vector2d scale)
    {
        return new(new(scale.X, 0, 0),
                   new(0, scale.Y, 0),
                   new(0, 0, 1));
    }

    public static Matrix3x3d CreateTranslation(Vector2d translation)
    {
        return new(new(1, 0, translation.X),
                   new(0, 1, translation.Y),
                   new(0, 0, 1));
    }
}
