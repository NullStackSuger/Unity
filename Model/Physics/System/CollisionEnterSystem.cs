namespace ET;

public interface ICollisionEnter : ICollision
{
    public void Run(Entity self, Entity other);
}

[CollisionHandler]
public abstract class CollisionEnterSystem<T> : SystemObject, ICollisionEnter where T : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionEnter);
    }

    public Type[] Types()
    {
        return [typeof(T)];
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionEnter((T)self);
    }

    protected abstract void CollisionEnter(T self);
}

[CollisionHandler]
public abstract class CollisionEnterSystem<T, A> : SystemObject, ICollisionEnter where T : Entity where A : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionEnter);
    }
    
    public Type[] Types()
    {
        return [typeof(T), typeof(A)];
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionEnter((T)self, (A)other);
    }
    
    protected abstract void CollisionEnter(T self, A other);
}