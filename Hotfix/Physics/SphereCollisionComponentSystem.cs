using BulletSharp;

namespace ET;

public static partial class SphereCollisionComponentSystem
{
    [EntitySystem]
    private class ET_SphereCollisionComponent_AwakeSystem : AwakeSystem<SphereCollisionComponent, float>
    {
        protected override void Awake(SphereCollisionComponent self, float radius)
        {
            self.radius = radius;
            self.shape = Init(radius);
        }
    }
    
    [EntitySystem]
    private class ET_SphereCollisionComponent_DeserializeSystem : DeserializeSystem<SphereCollisionComponent>
    {
        protected override void Deserialize(SphereCollisionComponent self)
        {
            self.shape = Init(self.radius);
        }
    }

    private static CollisionShape Init(float radius)
    {
        return new SphereShape(radius);
    }   
}