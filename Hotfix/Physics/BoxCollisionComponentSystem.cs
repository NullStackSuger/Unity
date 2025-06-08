using System.Numerics;
using BulletSharp;

namespace ET;

public static partial class BoxCollisionComponentSystem
{
    [EntitySystem]
    private class ET_BoxCollisionComponent_AwakeSystem : AwakeSystem<BoxCollisionComponent, Vector3>
    {
        protected override void Awake(BoxCollisionComponent self, Vector3 size)
        {
            self.size = size;
            self.shape = Init(self.size);
        }
    }
    
    [EntitySystem]
    private class ET_BoxCollisionComponent_DeserializeSystem : DeserializeSystem<BoxCollisionComponent>
    {
        protected override void Deserialize(BoxCollisionComponent self)
        {
            self.shape = Init(self.size);
        }
    }

    private static CollisionShape Init(Vector3 size)
    {
        return new BoxShape(0.5f * size.ToBullet());
    }
}