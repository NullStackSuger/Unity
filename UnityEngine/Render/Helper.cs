using System.Numerics;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Loaders;

namespace UnityEngine;

public static partial class Helper
{
    public const float degToRad = (float)Math.PI / 180.0f;
    public const float rad2Deg = 180f / MathF.PI;

    public static Quaternion ToQuaternion(this Vector3 angles)
    {
        Vector3 rad = angles * degToRad;
        Quaternion rotX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, rad.X);
        Quaternion rotY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rad.Y);
        Quaternion rotZ = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rad.Z);
        return Quaternion.Normalize(rotY * rotX * rotZ);
    }

    public static Vector3 ToVector3(this Quaternion q)
    {
        q = Quaternion.Normalize(q);

        float x, y, z;

        // sin(pitch)
        float sinp = 2f * (q.W * q.X - q.Z * q.Y);
        if (MathF.Abs(sinp) >= 1f)
            x = MathF.CopySign(MathF.PI / 2f, sinp); // 极值时为 ±90°
        else
            x = MathF.Asin(sinp); // pitch（X轴）

        // yaw (Y轴)
        float siny_cosp = 2f * (q.W * q.Y + q.Z * q.X);
        float cosy_cosp = 1f - 2f * (q.X * q.X + q.Y * q.Y);
        y = MathF.Atan2(siny_cosp, cosy_cosp);

        // roll (Z轴)
        float sinr_cosp = 2f * (q.W * q.Z + q.X * q.Y);
        float cosr_cosp = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
        z = MathF.Atan2(sinr_cosp, cosr_cosp);

        return new Vector3(x, y, z) * rad2Deg;
    }
    
    // TODO MVP应由GameObject,Camera,Window计算
    internal static Matrix4x4 Model(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
    }
    internal static Matrix4x4 Model(Vector3 position)
    {
        return Model(position, Quaternion.Identity, Vector3.One);
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
        
        Matrix4x4 mat = new Matrix4x4
        {
            [0, 0] = 1.0f / (aspect * tanHalfFovY),
            [1, 1] = 1.0f / tanHalfFovY,
            [2, 2] = far / (far - near),
            [2, 3] = 1.0f,
            [3, 2] = -(far * near) / (far - near)
        };
        return mat;
    }

    internal static LoadResult LoadObj(string path)
    {
        var loader = objLoaderFactory.Create();
        var fs = new FileStream($"{path}.obj", FileMode.Open);
        return loader.Load(fs);
    }
    internal static void LoadObj(string path, out ushort[] indices, out Vector3[] positions, out Vector2[] uvs, out Vector3[] normals)
    {
        var vertexDict = new Dictionary<FaceVertex, ushort>(); // 去重
        var positionList = new List<Vector3>();
        var uvList = new List<Vector2>();
        var normalList = new List<Vector3>();
        var indexList = new List<ushort>();
        
        var result = LoadObj(path);
        foreach (var group in result.Groups)
        {
            foreach (var face in group.Faces)
            {
                for(int i = 0; i < face.Count; ++i)
                {
                    var faceVertex = face[i];
                    
                    if (!vertexDict.TryGetValue(faceVertex, out ushort index))
                    {
                        // Position
                        var v = result.Vertices[faceVertex.VertexIndex - 1];
                        var position = new Vector3(v.X, v.Y, v.Z);

                        // UV
                        Vector2 uv = Vector2.Zero;
                        if (faceVertex.TextureIndex > 0 && faceVertex.TextureIndex <= result.Textures.Count)
                        {
                            var t = result.Textures[faceVertex.TextureIndex - 1];
                            uv = new Vector2(t.X, t.Y);
                        }
                        
                        // Normal
                        Vector3 normal = Vector3.UnitZ;
                        if (faceVertex.NormalIndex > 0 && faceVertex.NormalIndex <= result.Normals.Count)
                        {
                            var n = result.Normals[faceVertex.NormalIndex - 1];
                            normal = new Vector3(n.X, n.Y, n.Z);
                        }

                        positionList.Add(position);
                        uvList.Add(uv);
                        normalList.Add(normal);
                        index = (ushort)(positionList.Count - 1);
                        vertexDict[faceVertex] = index;
                    }

                    indexList.Add(index);
                }
            }
        }
        
        positions = positionList.ToArray();
        uvs = uvList.ToArray();
        normals = normalList.ToArray();
        indices = indexList.ToArray();
    }

    private static readonly ObjLoaderFactory objLoaderFactory = new();
}