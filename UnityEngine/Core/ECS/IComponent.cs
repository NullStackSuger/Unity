namespace UnityEngine;

public abstract class MonoBehaviour : IComponent, IComponentAwake, IComponentUpdate, IComponentDestroy, IComponentDrawSetting
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
    
    public virtual void DrawSetting()
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

/// <summary>
/// 在Setting面板如何显示
/// </summary>
public interface IComponentDrawSetting
{
    void DrawSetting();
}