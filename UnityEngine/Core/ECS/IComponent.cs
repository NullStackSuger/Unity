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

    ~MonoBehaviour()
    {
        this.gameObject = null;
    }

    public GameObject gameObject { get; internal set; }
}

public interface IComponent
{
    
}

public interface IComponentAwake
{
    void OnAwake();
}

public interface IComponentUpdate
{
    void OnUpdate();
}

public interface IComponentDestroy
{
    void OnDestroy();
}