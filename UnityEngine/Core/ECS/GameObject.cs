namespace UnityEngine;

public class GameObject : IDisposable
{
    public GameObject() : this("Game Object")
    {
    }

    public GameObject(string name)
    {
        this.name = name;
        this.Parent = Scene.ActiveScene; // 如果Scene还没创建(GameObject就是Scene), 那 Parent==null
        
        transform = AddComponent<TransformComponent>();
    }

    ~GameObject()
    {
        Dispose();
    }
    public void Dispose()
    {
        foreach (MonoBehaviour component in components.Values)
        {
            component.OnDestroy();
        }
        components.Clear();

        foreach (GameObject child in children)
        {
            GameObject.RemoveChild(this, child);
        }

        IsDispose = true;
    }
    public bool IsDispose { get; private set; }

    public override string ToString()
    {
        return name;
    }
    public string name;

    public void Tick()
    {
        foreach (MonoBehaviour component in components.Values)
        {
            component.OnUpdate();
        }

        foreach (GameObject child in children)
        {
            child.Tick();
        }
    }

    #region Component
    public T AddComponent<T>() where T : MonoBehaviour, new()
    {
        T component = new T
        {
            gameObject = this
        };
        components.Add(typeof(T), component);
        component.OnAwake();
        return component;
    }

    public void RemoveComponent<T>() where T : MonoBehaviour, new()
    {
        components[typeof(T)].OnDestroy();
        components.Remove(typeof(T));
    }

    public T GetComponent<T>() where T : MonoBehaviour, new()
    {
        return components[typeof(T)] as T;
    }

    public bool TryGetComponent<T>(out T component) where T : MonoBehaviour, new()
    {
        bool hasValue = components.TryGetValue(typeof(T), out var mono);
        component = mono as T;
        return hasValue;
    }
    
    private readonly Dictionary<Type, MonoBehaviour> components = new();
    public IReadOnlyDictionary<Type, MonoBehaviour> Components => components;
    #endregion

    #region Parent & Children
    /// <summary>
    /// 判断自己是否是other的父亲(不包括this==other)
    /// </summary>
    public bool IsParentOf(GameObject other)
    {
        GameObject current = other?.parent;
        while (current != null)
        {
            if (current == this)
                return true;
            current = current.parent;
        }
        return false;
    }
    private static void AddChild(GameObject parent, GameObject child)
    {
        parent.children.Add(child);
        child.parent = parent;
    }
    private static void RemoveChild(GameObject parent, GameObject child)
    {
        parent.children.Remove(child);
        child.parent = null;
    }
    /// <summary>
    /// 以自己为起点, 找到第一个符合条件的GameObject
    /// </summary>
    public GameObject Find(Func<GameObject, bool> func)
    {
        if (func(this)) return this;
        foreach (GameObject child in children)
        {
            GameObject result = child.Find(func);
            if (result != null)
                return result;
        }
        return null;
    }
    
    
    private readonly HashSet<GameObject> children = new();
    public IReadOnlySet<GameObject> Children => children;
    private GameObject parent;
    public GameObject Parent
    {
        get => parent;
        set
        {
            // 把Scene当作一个特殊的GameObject, 因此不能自己去设置为null, 
            if (value == null) return;
            if(value == this) return;
            if (this.IsParentOf(value)) return;
            
            if (parent != null)
            {
                GameObject.RemoveChild(parent, this);
            }
            
            GameObject.AddChild(value, this);
        }
    }
    #endregion

    public readonly TransformComponent transform;
}