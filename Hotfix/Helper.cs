using System.Numerics;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace ET;

public static class Helper
{
    public static BulletSharp.Math.Vector3 ToBullet(this Vector3 v)
    {
        return new BulletSharp.Math.Vector3(v.X, v.Y, v.Z);
    }

    public static BulletSharp.Math.Vector4 ToBullet(this Vector4 v)
    {
        return new BulletSharp.Math.Vector4(v.X, v.Y, v.Z, v.W);
    }

    public static BulletSharp.Math.Quaternion ToBullet(this Quaternion q)
    {
        return new BulletSharp.Math.Quaternion(q.X, q.Y, q.Z, q.W);
    }

    public static BulletSharp.Math.Matrix ToBullet(this Matrix4x4 m)
    {
        return new BulletSharp.Math.Matrix(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
    }

    public static Vector3 ToNumerics(this BulletSharp.Math.Vector3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static Vector4 ToNumerics(this BulletSharp.Math.Vector4 v)
    {
        return new Vector4(v.X, v.Y, v.Z, v.W);
    }

    public static Quaternion ToNumerics(this BulletSharp.Math.Quaternion q)
    {
        return new Quaternion(q.X, q.Y, q.Z, q.W);
    }

    public static Matrix4x4 ToNumerics(this BulletSharp.Math.Matrix m)
    {
        return new Matrix4x4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
    }

    public static BulletSharp.Math.Quaternion Quaternion(this BulletSharp.Math.Matrix m)
    {
        float trace = m.M11 + m.M22 + m.M33;
        float qw, qx, qy, qz;

        if (trace > 0.0f)
        {
            float s = (float)Math.Sqrt(trace + 1.0f) * 2; // S=4*qw
            qw = 0.25f * s;
            qx = (m.M32 - m.M23) / s;
            qy = (m.M13 - m.M31) / s;
            qz = (m.M21 - m.M12) / s;
        }
        else if ((m.M11 > m.M22) && (m.M11 > m.M33))
        {
            float s = (float)Math.Sqrt(1.0f + m.M11 - m.M22 - m.M33) * 2; // S=4*qx
            qw = (m.M32 - m.M23) / s;
            qx = 0.25f * s;
            qy = (m.M12 + m.M21) / s;
            qz = (m.M13 + m.M31) / s;
        }
        else if (m.M22 > m.M33)
        {
            float s = (float)Math.Sqrt(1.0f + m.M22 - m.M11 - m.M33) * 2; // S=4*qy
            qw = (m.M13 - m.M31) / s;
            qx = (m.M12 + m.M21) / s;
            qy = 0.25f * s;
            qz = (m.M23 + m.M32) / s;
        }
        else
        {
            float s = (float)Math.Sqrt(1.0f + m.M33 - m.M11 - m.M22) * 2; // S=4*qz
            qw = (m.M21 - m.M12) / s;
            qx = (m.M13 + m.M31) / s;
            qy = (m.M23 + m.M32) / s;
            qz = 0.25f * s;
        }

        return new BulletSharp.Math.Quaternion(qx, qy, qz, qw);
    }
}