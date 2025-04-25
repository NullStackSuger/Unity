using System.Numerics;
using System.Runtime.InteropServices;

namespace UnityEngine;

public static partial class Helper
{
    /// <summary>
    /// Vk中把对象转为bytes的方法, 考虑了字段布局等,
    /// 如果用其他序列化会出问题
    /// </summary>
    internal static byte[] VkToBytes<T>(T data)
    {
        int size = Marshal.SizeOf<T>();
        var bytes = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(data, ptr, false);
        Marshal.Copy(ptr, bytes, 0, size);
        Marshal.FreeHGlobal(ptr);
        return bytes;
    }

    public static float ToRadians(float degrees)
    {
        return (float)Math.PI * degrees / 180.0f;
    }

    public static float ToDegrees(float radians)
    {
        return radians * 180.0f / (float)Math.PI;
    }

    internal static Matrix4x4 Model(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        float c3 = MathF.Cos(rotation.Z);
        float s3 = MathF.Sin(rotation.Z);
        float c2 = MathF.Cos(rotation.X);
        float s2 = MathF.Sin(rotation.X);
        float c1 = MathF.Cos(rotation.Y);
        float s1 = MathF.Sin(rotation.Y);
        return new Matrix4x4
        (
            scale.X * (c1 * c3 + s1 * s2 * s3), scale.X * (c2 * s3), scale.X * (c1 * s2 * s3 - c3 * s1), 0.0f,
            scale.Y * (c3 * s1 * s2 - c1 * s3), scale.Y * (c2 * c3), scale.Y * (c1 * c3 * s2 + s1 * s3), 0.0f,
            scale.Z * (c2 * s1), scale.Z * -s2, scale.Z * (c1 * c2), 0.0f,
            position.X, position.Y, position.Z, 1.0f
        );
    }
    internal static Matrix4x4 Model(Vector3 position)
    {
        return Model(position, Vector3.Zero, Vector3.One);
    }

    internal static Matrix4x4 View(Vector3 position, Vector3 rotation)
    {
        float c3 = MathF.Cos(rotation.Z);
        float s3 = MathF.Sin(rotation.Z);
        float c2 = MathF.Cos(rotation.X);
        float s2 = MathF.Sin(rotation.X);
        float c1 = MathF.Cos(rotation.Y);
        float s1 = MathF.Sin(rotation.Y);
        Vector3 u = new Vector3((c1 * c3 + s1 * s2 * s3), (c2 * s3), (c1 * s2 * s3 - c3 * s1));
        Vector3 v = new Vector3((c3 * s1 * s2 - c1 * s3), (c2 * c3), (c1 * c3 * s2 + s1 * s3));
        Vector3 w = new Vector3((c2 * s1), (-s2), (c1 * c2));
        Matrix4x4 mat = Matrix4x4.Identity;
        mat[0, 0] = u.X;
        mat[1, 0] = u.Y;
        mat[2, 0] = u.Z;
        mat[0, 1] = v.X;
        mat[1, 1] = v.Y;
        mat[2, 1] = v.Z;
        mat[0, 2] = w.X;
        mat[1, 2] = w.Y;
        mat[2, 2] = w.Z;
        mat[3, 0] = -Vector3.Dot(u, position);
        mat[3, 1] = -Vector3.Dot(v, position);
        mat[3, 2] = -Vector3.Dot(w, position);
        return mat;
    }
    internal static Matrix4x4 View(Vector3 position)
    {
        return View(position, Vector3.Zero);   
    }

    internal static Matrix4x4 Orthographic(float left, float right, float bottom, float top, float near, float far)
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        mat[0, 0] = 2.0f / (right - left);
        mat[1, 1] = 2.0f / (bottom - top);
        mat[2, 2] = 1.0f / (far - near);
        mat[3, 0] = -(right + left) / (right - left);
        mat[3, 1] = -(bottom + top) / (bottom - top);
        mat[3, 2] = -near / (far - near);
        return mat;
    }
    internal static Matrix4x4 Perspective(float fovY, float aspect, float near, float far)
    {
        Debug.Assert(aspect - float.Epsilon <= 0.0f, $"({aspect}), ({float.Epsilon})");

        float tanHalfFovY = MathF.Tan(fovY / 2.0f);
        
        Matrix4x4 mat = new Matrix4x4();
        mat[0, 0] = 1.0f / (aspect * tanHalfFovY);
        mat[1, 1] = 1.0f / tanHalfFovY;
        mat[2, 2] = far / (far - near);
        mat[2, 3] = 1.0f;
        mat[3, 2] = -(far * near) / (far - near);
        return mat;
    }
}