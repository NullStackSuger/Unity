namespace ET;

public interface ICollisionExit
{
}

public interface ICollisionExit<A>
{
}

public interface ICollisionExitSystem : ISystem
{
    void Run(Entity self);
}

public interface ICollisionExitSystem<A> : ISystem
{
    void Run(Entity self, Entity other);
}

[EntitySystem]
public abstract class CollisionExitSystem<T> : SystemObject, ICollisionExitSystem where T : Entity, ICollisionExit
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionExitSystem);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    public void Run(Entity self)
    {
        this.CollisionExit((T)self);
    }

    protected abstract void CollisionExit(T self);
}

[EntitySystem]
public abstract class CollisionExitSystem<T, A> : SystemObject, ICollisionExitSystem<A> where T : Entity, ICollisionExit<A> where A : Entity
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionExitSystem<>);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionExit((T)self, (A)other);
    }
    
    protected abstract void CollisionExit(T self, A a);
}