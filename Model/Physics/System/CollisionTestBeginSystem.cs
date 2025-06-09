namespace ET;

public interface ICollisionTestBegin : ICollision
{
    public void Run(Entity self);
}

[CollisionHandler]
public abstract class CollisionTestBeginSystem<T> : SystemObject, ICollisionTestBegin where T : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionTestBegin);
    }
    
    public Type[] Types()
    {
        return [typeof(T)];
    }
    
    public void Run(Entity self)
    {
        this.CollisionTestBegin((T)self);
    }

    protected abstract void CollisionTestBegin(T self);
}