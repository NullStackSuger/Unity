using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public sealed class GameObject : Entity
{
    [BsonIgnore]
    private GameObject parent;
    [BsonIgnore]
    public GameObject Parent
    {
        get => this.parent;
        set
        {
            if (value == null) Log.Error($"cant set parent null: {this.GetType().FullName}");
            if (value == this) Log.Error($"cant set parent self: {this.GetType().FullName}");

            if (this.parent != null) // 之前有parent
            {
                // parent相同，不设置
                if (this.parent == value) Log.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                
                // 这里只是更换Parent, 不涉及Dispose
                this.parent.children.Remove(this.Id);
            }
            
            this.parent = value;
        }
    }
    
    [BsonIgnore]
    private SortedDictionary<long, GameObject> children = new();
    [BsonIgnore]
    public IReadOnlyCollection<GameObject> Children => children.Values;
    [BsonElement]
    [BsonIgnoreIfNull]
    private List<GameObject> childrenDB;
    
    [BsonIgnore]
    private SortedDictionary<long, Component> components = new();
    [BsonIgnore]
    public IReadOnlyCollection<Component> Components => components.Values;
    [BsonElement]
    [BsonIgnoreIfNull]
    private List<Component> componentsDB;

    public GameObject()
    {
        this.Id = IdGenerator.Instance.GenerateId();
    }

    // 用于Bson反序列化使用
    [BsonConstructor]
    private GameObject(long id)
    {
        this.Id = id;
    }

    public override void Dispose()
    {
        if (this.IsDisposed) return;
        
        // 清理Children
        GameObject[] children = this.children.Values.ToArray();
        for (int i = children.Length - 1; i >= 0; i--)
        {
            children[i].Dispose();
        }
        this.children.Clear();
        this.children = null;
        
        // 清理Component
        Component[] components = this.components.Values.ToArray();
        for (int i = components.Length - 1; i >= 0; i--)
        {
            components[i].Dispose();
        }
        this.components.Clear();
        this.components = null;
        
        // 清理Parent
        if (this.parent != null && !this.parent.IsDisposed)
        {
            this.parent.children.Remove(this.Id);
        }
        this.parent = null;
        
        this.childrenDB?.Clear();
        this.childrenDB = null;
        this.componentsDB?.Clear();
        this.componentsDB = null;
        
        base.Dispose();
    }
    
    public override void OnSerialize()
    {
        base.OnSerialize();

        if (this.components != null)
        {
            this.componentsDB ??= new List<Component>();
            foreach (Component component in this.components.Values)
            {
                this.componentsDB.Add(component);
                component.OnSerialize();
            }
        }

        if (this.children != null)
        {
            this.childrenDB ??= new List<GameObject>();
            foreach (GameObject child in this.children.Values)
            {
                this.childrenDB.Add(child);
                child.OnSerialize();
            }
        }
    }

    public override void OnDeserialize()
    {
        base.OnDeserialize();

        if (this.componentsDB != null)
        {
            this.components ??= new SortedDictionary<long, Component>();
            foreach (Component component in this.componentsDB)
            {
                this.components.Add(component.GetType().GetLongHashCode(), component);
                component.GameObject = this;
                component.OnDeserialize();
            }
            this.componentsDB.Clear();
            this.componentsDB = null;
        }

        if (this.childrenDB != null)
        {
            this.children ??= new SortedDictionary<long, GameObject>();
            foreach (GameObject child in this.childrenDB)
            {
                this.children.Add(child.Id, child);
                child.OnDeserialize();
            }
            this.childrenDB.Clear();
            this.childrenDB = null;
        }
    }

    public IEnumerable<GameObject> Foreach()
    {
        yield return this;
        foreach (GameObject child in this.Children)
        {
            foreach (GameObject sub in child.Foreach())
            {
                yield return sub;
            }
        }
    }
    public void Foreach(Func<GameObject, bool> begin, Action<GameObject> end)
    {
        if (begin?.Invoke(this) ?? true)
        {
            foreach (GameObject child in this.Children)
            {
                child.Foreach(begin, end);
            }
        }
        end?.Invoke(this);
    }

    public GameObject GetChild(long id)
    {
        if (this.IsDisposed) return null;

        return this.children[id];
    }
    public bool GetChild(long id, out GameObject child)
    {
        if (this.IsDisposed)
        {
            child = null;
            return false;
        }
        
        return this.children.TryGetValue(id, out child);
    }
    public void RemoveChild(long id)
    {
        if (this.IsDisposed) return;
        
        if (!this.children.Remove(id, out GameObject child))
        {
            return;
        }
        
        child.Dispose();
    }
    public void RemoveChild(GameObject child)
    {
        if (this.IsDisposed) return;

        if (!this.children.Remove(child.Id)) return;

        child.Dispose();
    }
    public GameObject AddChild()
    {
        if (this.IsDisposed) return null;

        GameObject child = new();
        child.Parent = this;
        this.children.Add(child.Id, child);
        
        return child;
    }

    public T GetComponent<T>() where T : Component
    {
        if (this.IsDisposed) return null;

        return this.components[typeof(T).GetLongHashCode()] as T;
    }
    public bool GetComponent<T>(out T component) where T : Component
    {
        if (this.IsDisposed)
        {
            component = null;
            return false;
        }

        if (this.components.TryGetValue(typeof(T).GetLongHashCode(), out var componentEntity))
        {
            component = componentEntity as T;
            return true;
        }
        else
        {
            component = null;
            return false;
        }
    }
    public Component GetComponent(Type type)
    {
        if (this.IsDisposed) return null;
        if (!typeof(Component).IsAssignableFrom(type)) return null;

        return this.components[type.GetLongHashCode()];
    }
    /// <summary>
    /// 遍历寻找Component, 比较费性能
    /// </summary>
    public T GetComponentHard<T>() where T : Component
    {
        foreach (Component component in this.components.Values)
        {
            if (component.GetType().IsAbstract) continue;
            if (typeof(T).IsAssignableFrom(component.GetType())) return (T)component;
        }
        return null;
    }
    public void RemoveComponent<T>() where T : Component
    {
        if (this.IsDisposed) return;
        
        if (!this.components.Remove(typeof(T).GetLongHashCode(), out Component component))
        {
            return;
        }
        
        component.Dispose();
    }
    public void RemoveComponent(Component component)
    {
        if (this.IsDisposed) return;

        if (!this.components.Remove(component.GetType().GetLongHashCode()))
        {
            return;
        }
        
        component.Dispose();
    }
    public T AddComponent<T>(bool isRunning = false) where T : Component, IAwake, new()
    {
        if (this.IsDisposed) return null;
        
        if (this.components.ContainsKey(typeof(T).GetLongHashCode()))
        {
            Log.Error($"entity already has component: {typeof(T).FullName}");
        }
        
        T component = new T();
        component.GameObject = this;
        this.components.Add(typeof(T).GetLongHashCode(), component);
        EntitySystem.Instance.RegisterSystem(component);
        EntitySystem.Instance.Awake(component);
        if (isRunning) EntitySystem.Instance.Start(component);
        
        return component;
    }
    public T AddComponent<T, A>(A a, bool isRunning = false) where T : Component, IAwake<A>, new()
    {
        if (this.IsDisposed) return null;
        
        if (this.components.ContainsKey(typeof(T).GetLongHashCode()))
        {
            Log.Error($"entity already has component: {typeof(T).FullName}");
        }
        
        T component = new T();
        component.GameObject = this;
        this.components.Add(typeof(T).GetLongHashCode(), component);
        EntitySystem.Instance.RegisterSystem(component);
        EntitySystem.Instance.Awake(component, a);
        if (isRunning) EntitySystem.Instance.Start(component);
        
        return component;
    }
    public T AddComponent<T, A, B>(A a, B b, bool isRunning = false) where T : Component, IAwake<A, B>, new()
    {
        if (this.IsDisposed) return null;
        
        if (this.components.ContainsKey(typeof(T).GetLongHashCode()))
        {
            Log.Error($"entity already has component: {typeof(T).FullName}");
        }
        
        T component = new T();
        component.GameObject = this;
        this.components.Add(typeof(T).GetLongHashCode(), component);
        EntitySystem.Instance.RegisterSystem(component);
        EntitySystem.Instance.Awake(component, a, b);
        if (isRunning) EntitySystem.Instance.Start(component);
        
        return component;
    }
    public T AddComponent<T, A, B, C>(A a, B b, C c, bool isRunning = false) where T : Component, IAwake<A, B, C>, new()
    {
        if (this.IsDisposed) return null;
        
        if (this.components.ContainsKey(typeof(T).GetLongHashCode()))
        {
            Log.Error($"entity already has component: {typeof(T).FullName}");
        }
        
        T component = new T();
        component.GameObject = this;
        this.components.Add(typeof(T).GetLongHashCode(), component);
        EntitySystem.Instance.RegisterSystem(component);
        EntitySystem.Instance.Awake(component, a, b, c);
        if (isRunning) EntitySystem.Instance.Start(component);
        
        return component;
    }
    public T AddComponent<T, A, B, C, D>(A a, B b, C c, D d, bool isRunning = false) where T : Component, IAwake<A, B, C, D>, new()
    {
        if (this.IsDisposed) return null;
        
        if (this.components.ContainsKey(typeof(T).GetLongHashCode()))
        {
            Log.Error($"entity already has component: {typeof(T).FullName}");
        }
        
        T component = new T();
        component.GameObject = this;
        this.components.Add(typeof(T).GetLongHashCode(), component);
        EntitySystem.Instance.RegisterSystem(component);
        EntitySystem.Instance.Awake(component, a, b, c, d);
        if (isRunning) EntitySystem.Instance.Start(component);
        
        return component;
    }
    public T AddComponent<T, A, B, C, D, E>(A a, B b, C c, D d, E e, bool isRunning = false) where T : Component, IAwake<A, B, C, D, E>, new()
    {
        if (this.IsDisposed) return null;
        
        if (this.components.ContainsKey(typeof(T).GetLongHashCode()))
        {
            Log.Error($"entity already has component: {typeof(T).FullName}");
        }
        
        T component = new T();
        component.GameObject = this;
        this.components.Add(typeof(T).GetLongHashCode(), component);
        EntitySystem.Instance.RegisterSystem(component);
        EntitySystem.Instance.Awake(component, a, b, c, d, e);
        if (isRunning) EntitySystem.Instance.Start(component);
        
        return component;
    }
}