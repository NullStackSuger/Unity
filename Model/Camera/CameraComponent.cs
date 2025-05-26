using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public abstract class CameraComponent : Component
{
    [BsonIgnore]
    public virtual Matrix4x4 View
    {
        get
        {
            if (this.GameObject.GetComponent(out TransformComponent transform))
            {
                return Matrix4x4.Identity;
            }
            
            Vector3 zAxis = Vector3.Normalize(transform.Forward);
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(transform.Up, zAxis));
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);
            
            return new Matrix4x4
            (
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, transform.position), -Vector3.Dot(yAxis, transform.position), -Vector3.Dot(zAxis, transform.position), 1
            );
        }
    }
    
    [BsonIgnore]
    public abstract Matrix4x4 Projection { get; }
}