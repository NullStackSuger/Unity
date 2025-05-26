using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class OrthographicCameraComponent : CameraComponent, IAwake<Vector2, float, float>
{
    public float left;
    public float right;
    public float bottom;
    public float top;
    public float near;
    public float far;

    [BsonIgnore]
    public override Matrix4x4 Projection
    {
        get
        {
            Matrix4x4 mat = Matrix4x4.Identity;
            mat.M11 = 2.0f / (this.right - this.left);
            mat.M22 = 2.0f / (this.top - this.bottom);
            mat.M33 = 1.0f / (this.far - this.near);
            mat.M41 = -(this.right + this.left) / (this.right - this.left);
            mat.M42 = -(this.top + this.bottom) / (this.top - this.bottom);
            mat.M43 = -this.near / (this.far - this.near);
            return mat;
        }
    }
}