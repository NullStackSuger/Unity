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
    public Vector3 WorldPosition => Vector3.Transform(Vector3.Zero, this.World);
    [BsonIgnore]
    public Matrix4x4 Local => Matrix4x4.CreateScale(this.scale) * Matrix4x4.CreateFromQuaternion(this.rotation) * Matrix4x4.CreateTranslation(this.position);
    [BsonIgnore]
    public Matrix4x4 World
    {
        get
        {
            Matrix4x4 matrix = this.Local;
            GameObject current = this.GameObject;
            while (current != null)
            {
                if (current.GetComponent(out TransformComponent transform))
                {
                    matrix = transform.Local * matrix;
                }
                current = current.Parent;
            }
            return matrix;
        }
    }
}