namespace UnityEngine;

public class GameObject
{
    public GameObject() : this("Game Object")
    {
    }

    public GameObject(string name)
    {
        this.Parent = Scene.ActiveScene; // 如果Scene还没创建(GameObject就是Scene), 那 Parent==null
        this.name = name;
        
        transform = AddComponent<TransformComponent>();
    }
    
    public override string ToString()
    {
        return name;
    }
    public string name;

    #region Component
    // TODO 调用Awake, Destroy
    
    public T AddComponent<T>() where T : MonoBehaviour, new()
    {
        T component = new T
        {
            gameObject = this
        };
        components.Add(typeof(T), component);
        return component;
    }

    public void RemoveComponent<T>() where T : MonoBehaviour, new()
    {
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