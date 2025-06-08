using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class PerspectiveCameraComponent : CameraComponent, IAwake<float, float, float, float>
{
    public float fovY;
    public float aspect;
    public float near;
    public float far;

    [BsonIgnore]
    public override Matrix4x4 Projection
    {
        get
        {
            float tanHalfFovY = MathF.Tan(this.fovY * MathHelper.Deg2Rad);
        
            Matrix4x4 mat = new Matrix4x4
            {
                [0, 0] = 1.0f / (this.aspect * tanHalfFovY),
                [1, 1] = 1.0f / tanHalfFovY,
                [2, 2] = this.far / (this.far - this.near),
                [2, 3] = 1.0f,
                [3, 2] = -(this.far * this.near) / (this.far - this.near)
            };
            return mat;
        }
    }
}