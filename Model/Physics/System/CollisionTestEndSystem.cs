namespace ET;

public interface ICollisionTestEnd : ICollision
{
    public void Run(Entity self);
}

[CollisionHandler]
public abstract class CollisionTestEndSystem<T> : SystemObject, ICollisionTestEnd where T : Entity
{
    public Type SystemType()
    {
        return typeof(ICollisionTestEnd);
    }
    
    public Type[] Types()
    {
        return [typeof(T)];
    }
    
    public void Run(Entity self)
    {
        this.CollisionTestEnd((T)self);
    }

    protected abstract void CollisionTestEnd(T self);
}