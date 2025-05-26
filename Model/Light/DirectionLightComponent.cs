using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class DirectionLightComponent : Component, IAwake<float, Color>
{
    public float intensity;
    public Color color;

    [BsonIgnore]
    public Matrix4x4 View => this.GameObject.GetComponent<OrthographicCameraComponent>().View;
    [BsonIgnore]
    public Matrix4x4 Projection => this.GameObject.GetComponent<OrthographicCameraComponent>().Projection;
}