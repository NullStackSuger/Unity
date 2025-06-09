namespace ET;

public interface ICollisionStay : ICollision
{
    public void Run(Entity self, Entity other);
}

[CollisionHandler]
public abstract class CollisionStaySystem<T> : SystemObject, ICollisionStay where T : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionStay);
    }

    public Type[] Types()
    {
        return [typeof(T)];
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionStay((T)self);
    }

    protected abstract void CollisionStay(T self);
}

[CollisionHandler]
public abstract class CollisionStaySystem<T, A> : SystemObject, ICollisionStay where T : Entity where A : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionStay);
    }
    
    public Type[] Types()
    {
        return [typeof(T), typeof(A)];
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionStay((T)self, (A)other);
    }
    
    protected abstract void CollisionStay(T self, A other);
}