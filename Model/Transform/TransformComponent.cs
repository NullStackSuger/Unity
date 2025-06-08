using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class TransformComponent : Component, IAwake<Vector3, Quaternion, Vector3>
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    [BsonIgnore] 
    public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Quaternion.Normalize(rotation));
    [BsonIgnore]
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Quaternion.Normalize(rotation));

    [BsonIgnore]
    public Vector3 WorldPosition
    {
        get => Vector3.Transform(Vector3.Zero, this.Model);
        set
        {
            Matrix4x4 world = this.World;
            if (!Matrix4x4.Invert(world, out Matrix4x4 invWorld))
            {
                Log.Error(new InvalidOperationException("无法求父对象的逆矩阵 可能是缩放为0或矩阵不可逆"));
            }
            Vector3 localPos = Vector3.Transform(value, invWorld);
            this.position = localPos;
        }
    }
  
    [BsonIgnore]
    public Matrix4x4 Local => Matrix4x4.CreateScale(this.scale) * Matrix4x4.CreateFromQuaternion(this.rotation) * Matrix4x4.CreateTranslation(this.position);
    [BsonIgnore]
    public Matrix4x4 World
    {
        get
        {
            Matrix4x4 matrix = Matrix4x4.Identity;
            GameObject current = this.GameObject.Parent;
            while (current != null)
            {
                if (current.GetComponent(out TransformComponent transform))
                {
                    matrix *= transform.Local;
                }
                current = current.Parent;
            }
            return matrix;
        }
    }
    [BsonIgnore] 
    public Matrix4x4 Model => this.Local * this.World;
}