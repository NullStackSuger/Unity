namespace ET;

public class World: IDisposable
{
    private static World instance;
    
    public static World Instance
    {
        get
        {
            return instance ??= new World();
        }
    }
        
    private readonly Dictionary<Type, ASingleton> singletons = new();
        
    private World()
    {
    }
        
    public void Dispose()
    {
        instance = null;
            
        lock (this)
        {
            // dispose剩下的singleton，主要为了把instance置空
            foreach (var kv in this.singletons)
            {
                kv.Value.Dispose();
            }
        }
    }

    public T AddSingleton<T>() where T : ASingleton, ISingletonAwake, new()
    {
        T singleton = new();
        AddSingleton(singleton);
        singleton.Awake();
        return singleton;
    }
        
    public T AddSingleton<T, A>(A a) where T : ASingleton, ISingletonAwake<A>, new()
    {
        T singleton = new();
        AddSingleton(singleton);
        singleton.Awake(a);
        return singleton;
    }
        
    public T AddSingleton<T, A, B>(A a, B b) where T : ASingleton, ISingletonAwake<A, B>, new()
    {
        T singleton = new();
        AddSingleton(singleton);
        singleton.Awake(a, b);
        return singleton;
    }
        
    public T AddSingleton<T, A, B, C>(A a, B b, C c) where T : ASingleton, ISingletonAwake<A, B, C>, new()
    {
        T singleton = new();
        AddSingleton(singleton);
        singleton.Awake(a, b, c);
        return singleton;
    }

    private void AddSingleton(ASingleton singleton)
    {
        lock (this)
        {
            Type type = singleton.GetType();
            singletons[type] = singleton;
        }

        singleton.Register();
    }
}