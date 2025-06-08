namespace ET;

public static partial class TestComponentSystem
{
    [EntitySystem]
    private class ET_TestComponent_AwakeSystem : AwakeSystem<TestComponent>
    {
        protected override void Awake(TestComponent self)
        {
            Log.Debug("Awake");
        }
    }
    
    [EntitySystem]
    private class ET_TestComponent_CollisionEnterSystem1 : CollisionEnterSystem<TestComponent>
    {
        protected override void CollisionEnter(TestComponent self)
        {
            Log.Warning("Co 1");
        }
    }
    
    [EntitySystem]
    private class ET_TestComponent_CollisionEnterSystem : CollisionEnterSystem<TestComponent, TransformComponent>
    {
        protected override void CollisionEnter(TestComponent self, TransformComponent transform)
        {
            Log.Warning("Collision Enter");
        }
    }
    
    [EntitySystem]
    private class ET_TestComponent_CollisionEnterSystem2 : CollisionEnterSystem<TestComponent, PhysicsSceneComponent>
    {
        protected override void CollisionEnter(TestComponent self, PhysicsSceneComponent other)
        {
            Log.Warning("Co 2");
        }
    }
}