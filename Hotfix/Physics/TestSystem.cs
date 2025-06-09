namespace ET;

public class TestSystem : CollisionEnterSystem<TransformComponent, PhysicsSceneComponent>
{
    protected override void CollisionEnter(TransformComponent self, PhysicsSceneComponent other)
    {
        Log.Warning("000");
    }
}

public class TestSystem1 : CollisionEnterSystem<TransformComponent, TransformComponent>
{
    protected override void CollisionEnter(TransformComponent self, TransformComponent other)
    {
        Log.Warning("111");
    }
}

public class TestSystem2 : CollisionEnterSystem<TransformComponent>
{
    protected override void CollisionEnter(TransformComponent self)
    {
        Log.Warning("222");
    }
}

public class TestSystem3 : CollisionTestBeginSystem<TransformComponent>
{
    protected override void CollisionTestBegin(TransformComponent self)
    {
        Log.Warning("333");
    }
}