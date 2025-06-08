namespace ET;

public class SphereCollisionComponent : CollisionComponent, IAwake<float>, IDeserialize
{
    public float radius;
}