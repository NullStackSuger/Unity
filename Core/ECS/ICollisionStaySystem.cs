namespace ET;

public interface ICollisionStay
{
}

public interface ICollisionStay<A>
{
}

public interface ICollisionStaySystem : ISystem
{
    void Run(Entity self);
}

public interface ICollisionStaySystem<A> : ISystem
{
    void Run(Entity self, Entity other);
}

[EntitySystem]
public abstract class CollisionStaySystem<T> : SystemObject, ICollisionStaySystem where T : Entity, ICollisionStay
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionStaySystem);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    public void Run(Entity self)
    {
        this.CollisionStay((T)self);
    }

    protected abstract void CollisionStay(T self);
}

[EntitySystem]
public abstract class CollisionStaySystem<T, A> : SystemObject, ICollisionStaySystem<A> where T : Entity, ICollisionStay<A> where A : Entity
{
    public Type Type()
    {
        return typeof(T);
    }

    public Type SystemType()
    {
        return typeof(ICollisionStaySystem<>);
    }

    public int GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    public void Run(Entity self, Entity other)
    {
        this.CollisionStay((T)self, (A)other);
    }

    protected abstract void CollisionStay(T self, A a);
}