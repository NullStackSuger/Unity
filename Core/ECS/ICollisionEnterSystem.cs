namespace ET;

public interface ICollisionEnter
{
}

public interface ICollisionEnter<A>
{
}

public interface ICollisionEnterSystem : ISystem
{
    void Run(Entity self);
}

public interface ICollisionEnterSystem<A> : ISystem
{
    void Run(Entity self, Entity other);
}

[EntitySystem]
public abstract class CollisionEnterSystem<T> : SystemObject, ICollisionEnterSystem where T : Entity, ICollisionEnter
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionEnterSystem);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    public void Run(Entity self)
    {
        this.CollisionEnter((T)self);
    }

    protected abstract void CollisionEnter(T self);
}

[EntitySystem]
public abstract class CollisionEnterSystem<T, A> : SystemObject, ICollisionEnterSystem<A> where T : Entity, ICollisionEnter<A> where A : Entity
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionEnterSystem<>);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionEnter((T)self, (A)other);
    }

    protected abstract void CollisionEnter(T self, A other);
}