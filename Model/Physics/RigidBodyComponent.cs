using BulletSharp;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class RigidBodyComponent : Component, IAwake<float>, IDeserialize, IUpdate
{
    public float mass;
    
    [BsonIgnore]
    public RigidBody rigidBody;
}