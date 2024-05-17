using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Matrix4X4(Vector4D row1, Vector4D row2, Vector4D row3, Vector4D row4) : IEquatable<Matrix4X4>
{
    public static Matrix4X4 Identity => new(new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), new(0, 0, 0, 1));

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

    public readonly Vector4D Row1 => new(M11, M12, M13, M14);

    public readonly Vector4D Row2 => new(M21, M22, M23, M24);

    public readonly Vector4D Row3 => new(M31, M32, M33, M34);

    public readonly Vector4D Row4 => new(M41, M42, M43, M44);

    public readonly Vector4D Column1 => new(M11, M21, M31, M41);

    public readonly Vector4D Column2 => new(M12, M22, M32, M42);

    public readonly Vector4D Column3 => new(M13, M23, M33, M43);

    public readonly Vector4D Column4 => new(M14, M24, M34, M44);

    public readonly bool Equals(Matrix4X4 other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Matrix4X4 d && Equals(d);
    }

    public override readonly int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(M11);
        hash.Add(M12);
        hash.Add(M13);
        hash.Add(M14);

        hash.Add(M21);
        hash.Add(M22);
        hash.Add(M23);
        hash.Add(M24);

        hash.Add(M31);
        hash.Add(M32);
        hash.Add(M33);
        hash.Add(M34);

        hash.Add(M41);
        hash.Add(M42);
        hash.Add(M43);
        hash.Add(M44);

        return hash.ToHashCode();
    }

    public override readonly string ToString()
    {
        return $"M11: {M11}, M12: {M12}, M13: {M13}, M14: {M14}\n" +
               $"M21: {M21}, M22: {M22}, M23: {M23}, M24: {M24}\n" +
               $"M31: {M31}, M32: {M32}, M33: {M33}, M34: {M34}\n" +
               $"M41: {M41}, M42: {M42}, M43: {M43}, M44: {M44}";
    }

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

    public static Vector3D operator *(Matrix4X4 matrix, Vector3D vector)
    {
        double x = Vector4D.Dot(matrix.Row1, new(vector.X, vector.Y, vector.Z, 1));
        double y = Vector4D.Dot(matrix.Row2, new(vector.X, vector.Y, vector.Z, 1));
        double z = Vector4D.Dot(matrix.Row3, new(vector.X, vector.Y, vector.Z, 1));
        double w = Vector4D.Dot(matrix.Row4, new(vector.X, vector.Y, vector.Z, 1));

        return new Vector3D(x, y, z) / w;
    }

    public static Vector4D operator *(Matrix4X4 matrix, Vector4D vector)
    {
        double x = Vector4D.Dot(matrix.Row1, vector);
        double y = Vector4D.Dot(matrix.Row2, vector);
        double z = Vector4D.Dot(matrix.Row3, vector);
        double w = Vector4D.Dot(matrix.Row4, vector);

        return new(x, y, z, w);
    }

    public static Matrix4X4 operator *(Matrix4X4 left, Matrix4X4 right)
    {
        double m11 = Vector4D.Dot(left.Row1, right.Column1);
        double m12 = Vector4D.Dot(left.Row1, right.Column2);
        double m13 = Vector4D.Dot(left.Row1, right.Column3);
        double m14 = Vector4D.Dot(left.Row1, right.Column4);

        double m21 = Vector4D.Dot(left.Row2, right.Column1);
        double m22 = Vector4D.Dot(left.Row2, right.Column2);
        double m23 = Vector4D.Dot(left.Row2, right.Column3);
        double m24 = Vector4D.Dot(left.Row2, right.Column4);

        double m31 = Vector4D.Dot(left.Row3, right.Column1);
        double m32 = Vector4D.Dot(left.Row3, right.Column2);
        double m33 = Vector4D.Dot(left.Row3, right.Column3);
        double m34 = Vector4D.Dot(left.Row3, right.Column4);

        double m41 = Vector4D.Dot(left.Row4, right.Column1);
        double m42 = Vector4D.Dot(left.Row4, right.Column2);
        double m43 = Vector4D.Dot(left.Row4, right.Column3);
        double m44 = Vector4D.Dot(left.Row4, right.Column4);

        return new(new(m11, m12, m13, m14),
                   new(m21, m22, m23, m24),
                   new(m31, m32, m33, m34),
                   new(m41, m42, m43, m44));
    }

    public static bool operator ==(Matrix4X4 left, Matrix4X4 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Matrix4X4 left, Matrix4X4 right)
    {
        return !(left == right);
    }

    public static Matrix4X4 Transpose(Matrix4X4 matrix)
    {
        return new(matrix.Column1, matrix.Column2, matrix.Column3, matrix.Column4);
    }

    public static Matrix4X4 CreateRotationX(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new(1, 0, 0, 0),
                   new(0, cos, -sin, 0),
                   new(0, sin, cos, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4X4 CreateRotationY(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new(cos, 0, sin, 0),
                   new(0, 1, 0, 0),
                   new(-sin, 0, cos, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4X4 CreateRotationZ(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new(cos, -sin, 0, 0),
                   new(sin, cos, 0, 0),
                   new(0, 0, 1, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4X4 CreateScale(Vector3D scale)
    {
        return new(new(scale.X, 0, 0, 0),
                   new(0, scale.Y, 0, 0),
                   new(0, 0, scale.Z, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4X4 CreateTranslation(Vector3D translation)
    {
        return new(new(1, 0, 0, translation.X),
                   new(0, 1, 0, translation.Y),
                   new(0, 0, 1, translation.Z),
                   new(0, 0, 0, 1));
    }

    public static Matrix4X4 CreateLookAt(Vector3D cameraPosition, Vector3D cameraTarget, Vector3D cameraUpVector)
    {
        Vector3D e = cameraPosition;
        Vector3D zAxis = Vector3D.Normalize(cameraTarget - cameraPosition);
        Vector3D xAxis = Vector3D.Normalize(Vector3D.Cross(zAxis, cameraUpVector));
        Vector3D yAxis = Vector3D.Cross(xAxis, zAxis);

        Matrix4X4 rViewInverse = new(new(xAxis.X, yAxis.X, -zAxis.X, 0),
                                     new(xAxis.Y, yAxis.Y, -zAxis.Y, 0),
                                     new(xAxis.Z, yAxis.Z, -zAxis.Z, 0),
                                     new(0, 0, 0, 1));

        Matrix4X4 tView = CreateTranslation(-e);
        Matrix4X4 rView = Transpose(rViewInverse);

        return rView * tView;
    }
}
