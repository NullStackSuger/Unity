namespace ET;

public interface ICollisionExit : ICollision
{
    public void Run(Entity self, Entity other);
}

[CollisionHandler]
public abstract class CollisionExitSystem<T> : SystemObject, ICollisionExit where T : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionExit);
    }

    public Type[] Types()
    {
        return [typeof(T)];
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionExit((T)self);
    }

    protected abstract void CollisionExit(T self);
}

[CollisionHandler]
public abstract class CollisionExitSystem<T, A> : SystemObject, ICollisionExit where T : Entity where A : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionExit);
    }
    
    public Type[] Types()
    {
        return [typeof(T), typeof(A)];
    }
    
    public void Run(Entity self, Entity other)
    {
        this.CollisionExit((T)self, (A)other);
    }
    
    protected abstract void CollisionExit(T self, A other);
}