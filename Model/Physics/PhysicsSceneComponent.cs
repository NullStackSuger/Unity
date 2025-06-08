using BulletSharp;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class PhysicsSceneComponent : Component, IAwake<float>, IDestroy, IDeserialize, IUpdate
{
    public float g;

    [BsonIgnore] 
    public DiscreteDynamicsWorld world;
    [BsonIgnore]
    public CollisionConfiguration config;
    [BsonIgnore]
    public Dispatcher dispatcher;
    [BsonIgnore]
    public DbvtBroadphase broadphase;
    [BsonIgnore]
    public ConstraintSolver solver;
    [BsonIgnore]
    public HashSet<(CollisionObject, CollisionObject)> lastCollision = new();
    [BsonIgnore]
    public HashSet<(CollisionObject, CollisionObject)> nowCollision = new();
}