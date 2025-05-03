namespace UnityEngine;

public abstract class MonoBehaviour : IComponent, IComponentAwake, IComponentUpdate, IComponentDestroy
{
    public virtual void OnAwake()
    {
        
    }

    public virtual void OnUpdate()
    {
        
    }

    public virtual void OnDestroy()
    {
        
    }
}

public interface IComponent
{
    
}

public interface IComponentUpdate
{
    void OnUpdate();
}

public interface IComponentAwake
{
    void OnAwake();
}

public interface IComponentDestroy
{
    void OnDestroy();
}