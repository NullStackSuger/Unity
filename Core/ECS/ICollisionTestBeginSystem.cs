/*namespace ET;

public interface ICollisionTestBegin
{
}

public interface ICollisionTestBeginSystem : ISystem
{
    void Run(Entity o);
}

[EntitySystem]
public abstract class CollisionTestBeginSystem<T> : SystemObject, ICollisionTestBeginSystem where T : Entity, ICollisionTestBegin
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionTestBeginSystem);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    public void Run(Entity o)
    {
        this.CollisionTestBegin((T)o);
    }

    protected abstract void CollisionTestBegin(T self);
}*/