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

    public static LoadResult LoadObj(string path)
    {
        var loader = objLoaderFactory.Create();
        var fs = new FileStream(path, FileMode.Open);
        return loader.Load(fs);
    }
    public static bool LoadObj(string path, out ushort[] indices, out Vector3[] positions, out Vector2[] uvs, out Vector3[] normals)
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

        return true;
    }

    private static readonly ObjLoaderFactory objLoaderFactory = new();
}