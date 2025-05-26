namespace ET;

public interface IAwake
{
}

public interface IAwake<A>
{
}
	
public interface IAwake<A, B>
{
}
	
public interface IAwake<A, B, C>
{
}
	
public interface IAwake<A, B, C, D>
{
}

public interface IAwake<A, B, C, D, E>
{
}

// 被AddComponent/AddChild方式添加时触发
// 而反序列化来的通过IDeserializeSystem近似Awake
public interface IAwakeSystem: ISystem
{
    void Run(Entity o);
}
	
public interface IAwakeSystem<A>: ISystem
{
    void Run(Entity o, A a);
}
	
public interface IAwakeSystem<A, B>: ISystem
{
    void Run(Entity o, A a, B b);
}
	
public interface IAwakeSystem<A, B, C>: ISystem
{
    void Run(Entity o, A a, B b, C c);
}
	
public interface IAwakeSystem<A, B, C, D>: ISystem
{
    void Run(Entity o, A a, B b, C c, D d);
}

public interface IAwakeSystem<A, B, C, D, E>: ISystem
{
    void Run(Entity o, A a, B b, C c, D d, E e);
}

[EntitySystem]
public abstract class AwakeSystem<T> : SystemObject, IAwakeSystem where T: Entity, IAwake
{
    Type ISystem.Type()
    {
        return typeof(T);
    }

    Type ISystem.SystemType()
    {
        return typeof(IAwakeSystem);
    }

    int ISystem.GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    void IAwakeSystem.Run(Entity o)
    {
        this.Awake((T)o);
    }

    protected abstract void Awake(T self);
}
    
[EntitySystem]
public abstract class AwakeSystem<T, A> : SystemObject, IAwakeSystem<A> where T: Entity, IAwake<A>
{
    Type ISystem.Type()
    {
        return typeof(T);
    }

    Type ISystem.SystemType()
    {
        return typeof(IAwakeSystem<A>);
    }

    int ISystem.GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    void IAwakeSystem<A>.Run(Entity o, A a)
    {
        this.Awake((T)o, a);
    }

    protected abstract void Awake(T self, A a);
}

[EntitySystem]
public abstract class AwakeSystem<T, A, B> : SystemObject, IAwakeSystem<A, B> where T: Entity, IAwake<A, B>
{
    Type ISystem.Type()
    {
        return typeof(T);
    }

    Type ISystem.SystemType()
    {
        return typeof(IAwakeSystem<A, B>);
    }

    int ISystem.GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    void IAwakeSystem<A, B>.Run(Entity o, A a, B b)
    {
        this.Awake((T)o, a, b);
    }

    protected abstract void Awake(T self, A a, B b);
}

[EntitySystem]
public abstract class AwakeSystem<T, A, B, C> : SystemObject, IAwakeSystem<A, B, C> where T: Entity, IAwake<A, B, C>
{
    Type ISystem.Type()
    {
        return typeof(T);
    }

    Type ISystem.SystemType()
    {
        return typeof(IAwakeSystem<A, B, C>);
    }

    int ISystem.GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    void IAwakeSystem<A, B, C>.Run(Entity o, A a, B b, C c)
    {
        this.Awake((T)o, a, b, c);
    }

    protected abstract void Awake(T self, A a, B b, C c);
}
    
[EntitySystem]
public abstract class AwakeSystem<T, A, B, C, D> : SystemObject, IAwakeSystem<A, B, C, D> where T: Entity, IAwake<A, B, C, D>
{
    Type ISystem.Type()
    {
        return typeof(T);
    }

    Type ISystem.SystemType()
    {
        return typeof(IAwakeSystem<A, B, C, D>);
    }

    int ISystem.GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    void IAwakeSystem<A, B, C, D>.Run(Entity o, A a, B b, C c, D d)
    {
        this.Awake((T)o, a, b, c, d);
    }

    protected abstract void Awake(T self, A a, B b, C c, D d);
}

[EntitySystem]
public abstract class AwakeSystem<T, A, B, C, D, E> : SystemObject, IAwakeSystem<A, B, C, D, E> where T: Entity, IAwake<A, B, C, D, E>
{
    Type ISystem.Type()
    {
        return typeof(T);
    }

    Type ISystem.SystemType()
    {
        return typeof(IAwakeSystem<A, B, C, D, E>);
    }

    int ISystem.GetInstanceQueueIndex()
    {
        return InstanceQueueIndex.None;
    }

    void IAwakeSystem<A, B, C, D, E>.Run(Entity o, A a, B b, C c, D d, E e)
    {
        this.Awake((T)o, a, b, c, d, e);
    }

    protected abstract void Awake(T self, A a, B b, C c, D d, E e);
}