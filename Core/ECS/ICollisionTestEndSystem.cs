/*namespace ET;

public interface ICollisionTestEnd
{
}

public interface ICollisionTestEndSystem : ISystem
{
    void Run(Entity o);
}

[EntitySystem]
public abstract class CollisionTestEndSystem<T> : SystemObject, ICollisionTestEndSystem where T : Entity, ICollisionTestEnd
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionTestEndSystem);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    public void Run(Entity o)
    {
        this.CollisionTestEnd((T)o);
    }

    protected abstract void CollisionTestEnd(T self);
}*/