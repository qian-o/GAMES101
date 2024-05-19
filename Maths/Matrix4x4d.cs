using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Matrix4x4d(Vector4d row1, Vector4d row2, Vector4d row3, Vector4d row4) : IEquatable<Matrix4x4d>
{
    public static Matrix4x4d Identity => new(new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), new(0, 0, 0, 1));

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

    public readonly Vector4d Row1 => new(M11, M12, M13, M14);

    public readonly Vector4d Row2 => new(M21, M22, M23, M24);

    public readonly Vector4d Row3 => new(M31, M32, M33, M34);

    public readonly Vector4d Row4 => new(M41, M42, M43, M44);

    public readonly Vector4d Column1 => new(M11, M21, M31, M41);

    public readonly Vector4d Column2 => new(M12, M22, M32, M42);

    public readonly Vector4d Column3 => new(M13, M23, M33, M43);

    public readonly Vector4d Column4 => new(M14, M24, M34, M44);

    public readonly bool Equals(Matrix4x4d other)
    {
        return GetHashCode() == other.GetHashCode();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Matrix4x4d d && Equals(d);
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

    public static Matrix4x4d operator +(Matrix4x4d left, Matrix4x4d right)
    {
        Vector4d row1 = left.Row1 + right.Row1;
        Vector4d row2 = left.Row2 + right.Row2;
        Vector4d row3 = left.Row3 + right.Row3;
        Vector4d row4 = left.Row4 + right.Row4;

        return new(row1, row2, row3, row4);
    }

    public static Matrix4x4d operator -(Matrix4x4d left, Matrix4x4d right)
    {
        Vector4d row1 = left.Row1 - right.Row1;
        Vector4d row2 = left.Row2 - right.Row2;
        Vector4d row3 = left.Row3 - right.Row3;
        Vector4d row4 = left.Row4 - right.Row4;

        return new(row1, row2, row3, row4);
    }

    public static Vector3d operator *(Matrix4x4d matrix, Vector3d vector)
    {
        double x = Vector4d.Dot(matrix.Row1, new(vector.X, vector.Y, vector.Z, 1));
        double y = Vector4d.Dot(matrix.Row2, new(vector.X, vector.Y, vector.Z, 1));
        double z = Vector4d.Dot(matrix.Row3, new(vector.X, vector.Y, vector.Z, 1));
        double w = Vector4d.Dot(matrix.Row4, new(vector.X, vector.Y, vector.Z, 1));

        return new Vector3d(x, y, z) / w;
    }

    public static Vector4d operator *(Matrix4x4d matrix, Vector4d vector)
    {
        double x = Vector4d.Dot(matrix.Row1, vector);
        double y = Vector4d.Dot(matrix.Row2, vector);
        double z = Vector4d.Dot(matrix.Row3, vector);
        double w = Vector4d.Dot(matrix.Row4, vector);

        return new(x, y, z, w);
    }

    public static Matrix4x4d operator *(Matrix4x4d left, Matrix4x4d right)
    {
        double m11 = Vector4d.Dot(left.Row1, right.Column1);
        double m12 = Vector4d.Dot(left.Row1, right.Column2);
        double m13 = Vector4d.Dot(left.Row1, right.Column3);
        double m14 = Vector4d.Dot(left.Row1, right.Column4);

        double m21 = Vector4d.Dot(left.Row2, right.Column1);
        double m22 = Vector4d.Dot(left.Row2, right.Column2);
        double m23 = Vector4d.Dot(left.Row2, right.Column3);
        double m24 = Vector4d.Dot(left.Row2, right.Column4);

        double m31 = Vector4d.Dot(left.Row3, right.Column1);
        double m32 = Vector4d.Dot(left.Row3, right.Column2);
        double m33 = Vector4d.Dot(left.Row3, right.Column3);
        double m34 = Vector4d.Dot(left.Row3, right.Column4);

        double m41 = Vector4d.Dot(left.Row4, right.Column1);
        double m42 = Vector4d.Dot(left.Row4, right.Column2);
        double m43 = Vector4d.Dot(left.Row4, right.Column3);
        double m44 = Vector4d.Dot(left.Row4, right.Column4);

        return new(new(m11, m12, m13, m14),
                   new(m21, m22, m23, m24),
                   new(m31, m32, m33, m34),
                   new(m41, m42, m43, m44));
    }

    public static bool operator ==(Matrix4x4d left, Matrix4x4d right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Matrix4x4d left, Matrix4x4d right)
    {
        return !(left == right);
    }

    public static Matrix4x4d Transpose(Matrix4x4d matrix)
    {
        return new(matrix.Column1, matrix.Column2, matrix.Column3, matrix.Column4);
    }

    public static Matrix4x4d CreateRotationX(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new(1, 0, 0, 0),
                   new(0, cos, -sin, 0),
                   new(0, sin, cos, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4x4d CreateRotationY(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new(cos, 0, sin, 0),
                   new(0, 1, 0, 0),
                   new(-sin, 0, cos, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4x4d CreateRotationZ(Angle angle)
    {
        double cos = Math.Cos(angle.Radians);
        double sin = Math.Sin(angle.Radians);

        return new(new(cos, -sin, 0, 0),
                   new(sin, cos, 0, 0),
                   new(0, 0, 1, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4x4d CreateScale(Vector3d scale)
    {
        return new(new(scale.X, 0, 0, 0),
                   new(0, scale.Y, 0, 0),
                   new(0, 0, scale.Z, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4x4d CreateTranslation(Vector3d translation)
    {
        return new(new(1, 0, 0, translation.X),
                   new(0, 1, 0, translation.Y),
                   new(0, 0, 1, translation.Z),
                   new(0, 0, 0, 1));
    }

    public static Matrix4x4d CreateRotationX(Angle angle, Vector3d centerPoint)
    {
        return CreateTranslation(centerPoint) * CreateRotationX(angle) * CreateTranslation(-centerPoint);
    }

    public static Matrix4x4d CreateRotationY(Angle angle, Vector3d centerPoint)
    {
        return CreateTranslation(centerPoint) * CreateRotationY(angle) * CreateTranslation(-centerPoint);
    }

    public static Matrix4x4d CreateRotationZ(Angle angle, Vector3d centerPoint)
    {
        return CreateTranslation(centerPoint) * CreateRotationZ(angle) * CreateTranslation(-centerPoint);
    }

    public static Matrix4x4d CreateLookAt(Vector3d cameraPosition, Vector3d cameraTarget, Vector3d cameraUpVector)
    {
        Vector3d e = cameraPosition;
        Vector3d zAxis = Vector3d.Normalize(cameraTarget - cameraPosition);
        Vector3d xAxis = Vector3d.Normalize(Vector3d.Cross(zAxis, cameraUpVector));
        Vector3d yAxis = Vector3d.Cross(xAxis, zAxis);

        Matrix4x4d rViewInverse = new(new(xAxis.X, yAxis.X, -zAxis.X, 0),
                                     new(xAxis.Y, yAxis.Y, -zAxis.Y, 0),
                                     new(xAxis.Z, yAxis.Z, -zAxis.Z, 0),
                                     new(0, 0, 0, 1));

        Matrix4x4d tView = CreateTranslation(-e);
        Matrix4x4d rView = Transpose(rViewInverse);

        return rView * tView;
    }

    public static Matrix4x4d CreateOrthographic(double width, double height, double zNearPlane, double zFarPlane)
    {
        return CreateOrthographicOffCenter(-width / 2.0, width / 2.0, -height / 2.0, height / 2.0, zNearPlane, zFarPlane);
    }

    public static Matrix4x4d CreateOrthographicOffCenter(double left, double right, double bottom, double top, double zNearPlane, double zFarPlane)
    {
        double xTranslation = (right + left) / 2.0;
        double yTranslation = (top + bottom) / 2.0;
        double zTranslation = (zNearPlane + zFarPlane) / 2.0;

        double xScale = 2.0 / (right - left);
        double yScale = 2.0 / (top - bottom);
        double zScale = 2.0 / (zNearPlane - zFarPlane);

        Matrix4x4d t = CreateTranslation(new Vector3d(-xTranslation, -yTranslation, -zTranslation));
        Matrix4x4d s = CreateScale(new Vector3d(xScale, yScale, zScale));

        return s * t;
    }

    public static Matrix4x4d CreatePerspective(double width, double height, double nearPlaneDistance, double farPlaneDistance)
    {
        return CreatePerspectiveOffCenter(-width / 2.0, width / 2.0, -height / 2.0, height / 2.0, nearPlaneDistance, farPlaneDistance);
    }

    public static Matrix4x4d CreatePerspectiveFieldOfView(Angle fieldOfView, double aspectRatio, double nearPlaneDistance, double farPlaneDistance)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fieldOfView.Radians, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fieldOfView.Radians, Math.PI);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(aspectRatio, 0.0);

        double top = nearPlaneDistance * Math.Tan(fieldOfView.Radians / 2.0);
        double right = top * aspectRatio;
        double bottom = -top;
        double left = -right;

        return CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance);
    }

    public static Matrix4x4d CreatePerspectiveOffCenter(double left, double right, double bottom, double top, double nearPlaneDistance, double farPlaneDistance)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(nearPlaneDistance, 0.0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(farPlaneDistance, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nearPlaneDistance, farPlaneDistance);

        Matrix4x4d perspToOrtho = new(new Vector4d(nearPlaneDistance, 0, 0, 0),
                                     new Vector4d(0, nearPlaneDistance, 0, 0),
                                     new Vector4d(0, 0, nearPlaneDistance + farPlaneDistance, -nearPlaneDistance * farPlaneDistance),
                                     new Vector4d(0, 0, 1, 0));

        Matrix4x4d ortho = CreateOrthographicOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance);

        return ortho * perspToOrtho;
    }

    public static Matrix4x4d CreateViewport(double x, double y, double width, double height, double minDepth, double maxDepth)
    {
        return new(new(width / 2.0, 0, 0, x + (width / 2.0)),
                   new(0, height / 2.0, 0, y + (height / 2.0)),
                   new(0, 0, maxDepth - minDepth, minDepth),
                   new(0, 0, 0, 1));
    }
}
