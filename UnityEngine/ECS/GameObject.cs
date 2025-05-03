namespace UnityEngine;

public class GameObject : IDisposable
{
    public GameObject()
    {
        
    }
    public GameObject(GameObject parent)
    {
        this.parent = parent;
        parent.AddChild(this);
    }
    
    public void Tick()
    {
        foreach (IComponentUpdate update in updateComponents.Values)
        {
            update.OnUpdate();
        }

        foreach (GameObject child in children)
        {
            child.Tick();
        }
    }
    
    public void Dispose()
    {
        foreach (Type componentType in components.Keys)
        {
            RemoveComponent(componentType);
        }

        foreach (GameObject child in children)
        {
            child.Dispose();
        }
        
        parent?.RemoveChild(this);
    }

    private void AddChild(GameObject child)
    {
        this.children.Add(child);
    }
    private void RemoveChild(GameObject child)
    {
        this.children.Remove(child);
    }
    
    public bool AddComponent<T>() where T : IComponent, new()
    {
        return AddComponent(typeof(T));
    }
    public bool AddComponent(Type componentType)
    {
        if (!components.TryAdd(componentType, Activator.CreateInstance(componentType) as IComponent)) return false;
        
        if (components[componentType] is IComponentAwake awake)
        {
            awake.OnAwake();
        }
        
        if (components[componentType] is IComponentUpdate update)
        {
            updateComponents[componentType] = update;
        }
        
        return true;
    }
    
    public void RemoveComponent<T>() where T : IComponent
    {
        RemoveComponent(typeof(T));
    }
    public void RemoveComponent(Type componentType)
    {
        updateComponents.Remove(componentType);

        if (components[componentType] is IComponentDestroy destroy)
        {
            destroy.OnDestroy();
        }
        
        components.Remove(componentType);
    }
    
    private readonly GameObject parent;
    private readonly List<GameObject> children = new();
    private readonly Dictionary<Type, IComponent> components = new();
    private readonly Dictionary<Type, IComponentUpdate> updateComponents = new();
}