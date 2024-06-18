using System.Runtime.InteropServices;

namespace Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Matrix4x4d(Vector4d row1, Vector4d row2, Vector4d row3, Vector4d row4) : IEquatable<Matrix4x4d>
{
    public static Matrix4x4d Identity => new(new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), new(0, 0, 0, 1));

    public float M11 = row1.X;

    public float M12 = row1.Y;

    public float M13 = row1.Z;

    public float M14 = row1.W;

    public float M21 = row2.X;

    public float M22 = row2.Y;

    public float M23 = row2.Z;

    public float M24 = row2.W;

    public float M31 = row3.X;

    public float M32 = row3.Y;

    public float M33 = row3.Z;

    public float M34 = row3.W;

    public float M41 = row4.X;

    public float M42 = row4.Y;

    public float M43 = row4.Z;

    public float M44 = row4.W;

    public unsafe float this[int row, int column]
    {
        get
        {
            fixed (float* ptr = &M11)
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
        float x = Vector4d.Dot(matrix.Row1, new(vector.X, vector.Y, vector.Z, 1));
        float y = Vector4d.Dot(matrix.Row2, new(vector.X, vector.Y, vector.Z, 1));
        float z = Vector4d.Dot(matrix.Row3, new(vector.X, vector.Y, vector.Z, 1));
        float w = Vector4d.Dot(matrix.Row4, new(vector.X, vector.Y, vector.Z, 1));

        return new Vector3d(x, y, z) / w;
    }

    public static Vector4d operator *(Matrix4x4d matrix, Vector4d vector)
    {
        float x = Vector4d.Dot(matrix.Row1, vector);
        float y = Vector4d.Dot(matrix.Row2, vector);
        float z = Vector4d.Dot(matrix.Row3, vector);
        float w = Vector4d.Dot(matrix.Row4, vector);

        return new(x, y, z, w);
    }

    public static Matrix4x4d operator *(Matrix4x4d left, Matrix4x4d right)
    {
        float m11 = Vector4d.Dot(left.Row1, right.Column1);
        float m12 = Vector4d.Dot(left.Row1, right.Column2);
        float m13 = Vector4d.Dot(left.Row1, right.Column3);
        float m14 = Vector4d.Dot(left.Row1, right.Column4);

        float m21 = Vector4d.Dot(left.Row2, right.Column1);
        float m22 = Vector4d.Dot(left.Row2, right.Column2);
        float m23 = Vector4d.Dot(left.Row2, right.Column3);
        float m24 = Vector4d.Dot(left.Row2, right.Column4);

        float m31 = Vector4d.Dot(left.Row3, right.Column1);
        float m32 = Vector4d.Dot(left.Row3, right.Column2);
        float m33 = Vector4d.Dot(left.Row3, right.Column3);
        float m34 = Vector4d.Dot(left.Row3, right.Column4);

        float m41 = Vector4d.Dot(left.Row4, right.Column1);
        float m42 = Vector4d.Dot(left.Row4, right.Column2);
        float m43 = Vector4d.Dot(left.Row4, right.Column3);
        float m44 = Vector4d.Dot(left.Row4, right.Column4);

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

    public static bool Invert(Matrix4x4d matrix, out Matrix4x4d result)
    {
        //                                       -1
        // If you have matrix M, inverse Matrix M   can compute
        //
        //     -1       1      
        //    M   = --------- A
        //            det(M)
        //
        // A is adjugate (adjoint) of M, where,
        //
        //      T
        // A = C
        //
        // C is Cofactor matrix of M, where,
        //           i + j
        // C   = (-1)      * det(M  )
        //  ij                    ij
        //
        //     [ a b c d ]
        // M = [ e f g h ]
        //     [ i j k l ]
        //     [ m n o p ]
        //
        // First Row
        //           2 | f g h |
        // C   = (-1)  | j k l | = + ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
        //  11         | n o p |
        //
        //           3 | e g h |
        // C   = (-1)  | i k l | = - ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
        //  12         | m o p |
        //
        //           4 | e f h |
        // C   = (-1)  | i j l | = + ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
        //  13         | m n p |
        //
        //           5 | e f g |
        // C   = (-1)  | i j k | = - ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
        //  14         | m n o |
        //
        // Second Row
        //           3 | b c d |
        // C   = (-1)  | j k l | = - ( b ( kp - lo ) - c ( jp - ln ) + d ( jo - kn ) )
        //  21         | n o p |
        //
        //           4 | a c d |
        // C   = (-1)  | i k l | = + ( a ( kp - lo ) - c ( ip - lm ) + d ( io - km ) )
        //  22         | m o p |
        //
        //           5 | a b d |
        // C   = (-1)  | i j l | = - ( a ( jp - ln ) - b ( ip - lm ) + d ( in - jm ) )
        //  23         | m n p |
        //
        //           6 | a b c |
        // C   = (-1)  | i j k | = + ( a ( jo - kn ) - b ( io - km ) + c ( in - jm ) )
        //  24         | m n o |
        //
        // Third Row
        //           4 | b c d |
        // C   = (-1)  | f g h | = + ( b ( gp - ho ) - c ( fp - hn ) + d ( fo - gn ) )
        //  31         | n o p |
        //
        //           5 | a c d |
        // C   = (-1)  | e g h | = - ( a ( gp - ho ) - c ( ep - hm ) + d ( eo - gm ) )
        //  32         | m o p |
        //
        //           6 | a b d |
        // C   = (-1)  | e f h | = + ( a ( fp - hn ) - b ( ep - hm ) + d ( en - fm ) )
        //  33         | m n p |
        //
        //           7 | a b c |
        // C   = (-1)  | e f g | = - ( a ( fo - gn ) - b ( eo - gm ) + c ( en - fm ) )
        //  34         | m n o |
        //
        // Fourth Row
        //           5 | b c d |
        // C   = (-1)  | f g h | = - ( b ( gl - hk ) - c ( fl - hj ) + d ( fk - gj ) )
        //  41         | j k l |
        //
        //           6 | a c d |
        // C   = (-1)  | e g h | = + ( a ( gl - hk ) - c ( el - hi ) + d ( ek - gi ) )
        //  42         | i k l |
        //
        //           7 | a b d |
        // C   = (-1)  | e f h | = - ( a ( fl - hj ) - b ( el - hi ) + d ( ej - fi ) )
        //  43         | i j l |
        //
        //           8 | a b c |
        // C   = (-1)  | e f g | = + ( a ( fk - gj ) - b ( ek - gi ) + c ( ej - fi ) )
        //  44         | i j k |
        //
        // Cost of operation
        // 53 adds, 104 muls, and 1 div.
        float a = matrix.M11, b = matrix.M12, c = matrix.M13, d = matrix.M14;
        float e = matrix.M21, f = matrix.M22, g = matrix.M23, h = matrix.M24;
        float i = matrix.M31, j = matrix.M32, k = matrix.M33, l = matrix.M34;
        float m = matrix.M41, n = matrix.M42, o = matrix.M43, p = matrix.M44;

        float kp_lo = k * p - l * o;
        float jp_ln = j * p - l * n;
        float jo_kn = j * o - k * n;
        float ip_lm = i * p - l * m;
        float io_km = i * o - k * m;
        float in_jm = i * n - j * m;

        float a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
        float a12 = -(e * kp_lo - g * ip_lm + h * io_km);
        float a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
        float a14 = -(e * jo_kn - f * io_km + g * in_jm);

        float det = a * a11 + b * a12 + c * a13 + d * a14;

        if (Math.Abs(det) < float.Epsilon)
        {
            result = new Matrix4x4d(new(float.NaN, float.NaN, float.NaN, float.NaN),
                                    new(float.NaN, float.NaN, float.NaN, float.NaN),
                                    new(float.NaN, float.NaN, float.NaN, float.NaN),
                                    new(float.NaN, float.NaN, float.NaN, float.NaN));
            return false;
        }

        float invDet = 1.0f / det;

        result.M11 = a11 * invDet;
        result.M21 = a12 * invDet;
        result.M31 = a13 * invDet;
        result.M41 = a14 * invDet;

        result.M12 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
        result.M22 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
        result.M32 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
        result.M42 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

        float gp_ho = g * p - h * o;
        float fp_hn = f * p - h * n;
        float fo_gn = f * o - g * n;
        float ep_hm = e * p - h * m;
        float eo_gm = e * o - g * m;
        float en_fm = e * n - f * m;

        result.M13 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
        result.M23 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
        result.M33 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
        result.M43 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

        float gl_hk = g * l - h * k;
        float fl_hj = f * l - h * j;
        float fk_gj = f * k - g * j;
        float el_hi = e * l - h * i;
        float ek_gi = e * k - g * i;
        float ej_fi = e * j - f * i;

        result.M14 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
        result.M24 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
        result.M34 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
        result.M44 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

        return true;
    }

    public static Matrix4x4d Transpose(Matrix4x4d matrix)
    {
        return new(matrix.Column1, matrix.Column2, matrix.Column3, matrix.Column4);
    }

    public static Matrix4x4d CreateRotationX(Angle angle)
    {
        float cos = MathF.Cos(angle.Radians);
        float sin = MathF.Sin(angle.Radians);

        return new(new(1, 0, 0, 0),
                   new(0, cos, -sin, 0),
                   new(0, sin, cos, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4x4d CreateRotationY(Angle angle)
    {
        float cos = MathF.Cos(angle.Radians);
        float sin = MathF.Sin(angle.Radians);

        return new(new(cos, 0, sin, 0),
                   new(0, 1, 0, 0),
                   new(-sin, 0, cos, 0),
                   new(0, 0, 0, 1));
    }

    public static Matrix4x4d CreateRotationZ(Angle angle)
    {
        float cos = MathF.Cos(angle.Radians);
        float sin = MathF.Sin(angle.Radians);

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

    public static Matrix4x4d CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
    {
        return CreateOrthographicOffCenter(-width / 2.0f, width / 2.0f, -height / 2.0f, height / 2.0f, zNearPlane, zFarPlane);
    }

    public static Matrix4x4d CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
    {
        float xTranslation = (right + left) / 2.0f;
        float yTranslation = (top + bottom) / 2.0f;
        float zTranslation = (zNearPlane + zFarPlane) / 2.0f;

        float xScale = 2.0f / (right - left);
        float yScale = 2.0f / (top - bottom);
        float zScale = 2.0f / (zNearPlane - zFarPlane);

        Matrix4x4d t = CreateTranslation(new Vector3d(-xTranslation, -yTranslation, -zTranslation));
        Matrix4x4d s = CreateScale(new Vector3d(xScale, yScale, zScale));

        return s * t;
    }

    public static Matrix4x4d CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance)
    {
        return CreatePerspectiveOffCenter(-width / 2.0f, width / 2.0f, -height / 2.0f, height / 2.0f, nearPlaneDistance, farPlaneDistance);
    }

    public static Matrix4x4d CreatePerspectiveFieldOfView(Angle fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fieldOfView.Radians, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fieldOfView.Radians, Math.PI);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(aspectRatio, 0.0);

        float top = nearPlaneDistance * MathF.Tan(fieldOfView.Radians / 2.0f);
        float right = top * aspectRatio;
        float bottom = -top;
        float left = -right;

        return CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance);
    }

    public static Matrix4x4d CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(nearPlaneDistance, 0.0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(farPlaneDistance, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nearPlaneDistance, farPlaneDistance);

        // https://games-cn.org/forums/topic/%e4%bd%9c%e4%b8%9a%e4%b8%89%e7%9a%84%e7%89%9b%e5%80%92%e8%bf%87%e6%9d%a5%e4%ba%86/
        Matrix4x4d scale = CreateScale(new Vector3d(1, 1, -1));

        Matrix4x4d perspToOrtho = new(new Vector4d(nearPlaneDistance, 0, 0, 0),
                                      new Vector4d(0, nearPlaneDistance, 0, 0),
                                      new Vector4d(0, 0, nearPlaneDistance + farPlaneDistance, -nearPlaneDistance * farPlaneDistance),
                                      new Vector4d(0, 0, 1, 0));

        Matrix4x4d ortho = CreateOrthographicOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance);

        return ortho * perspToOrtho * scale;
    }

    public static Matrix4x4d CreateViewport(float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        return new(new(width / 2.0f, 0, 0, x + (width / 2.0f)),
                   new(0, height / 2.0f, 0, y + (height / 2.0f)),
                   new(0, 0, maxDepth - minDepth, minDepth),
                   new(0, 0, 0, 1));
    }
}
