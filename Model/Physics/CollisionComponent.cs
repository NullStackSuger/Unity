using BulletSharp;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public abstract class CollisionComponent : Component
{
    [BsonIgnore]
    public CollisionShape shape;
}