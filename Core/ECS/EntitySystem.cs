namespace ET;

public class EntitySystem : Singleton<EntitySystem>, ISingletonAwake
{
    private readonly Queue<EntityRef<Entity>>[] queues = new Queue<EntityRef<Entity>>[InstanceQueueIndex.Max];
    private readonly TypeSystems typeSystems = new TypeSystems(InstanceQueueIndex.Max);
    
    public void Awake()
    {
        for (int i = 0; i < this.queues.Length; i++)
        {
            this.queues[i] = new Queue<EntityRef<Entity>>();
        }
        
        foreach (Type systemType in CodeTypes.Instance.GetTypes(typeof(EntitySystemAttribute)))
        {
            if (!typeof(ISystem).IsAssignableFrom(systemType)) continue;
            
            SystemObject obj = (SystemObject)Activator.CreateInstance(systemType);
            ISystem iSystem = obj as ISystem;
            TypeSystems.OneTypeSystems oneTypeSystems = this.typeSystems.GetOrCreateOneTypeSystems(iSystem!.Type()); // 这里iSystem.Type()就是Component的类型
            oneTypeSystems.Map.Add(iSystem.SystemType(), obj);
            int index = iSystem.GetInstanceQueueIndex();
            if (InstanceQueueIndex.None < index && index < InstanceQueueIndex.Max)
            {
                oneTypeSystems.QueueFlag[index] = true;
            }
        }
    }

    public void RegisterSystem(Entity entity)
    {
        TypeSystems.OneTypeSystems oneTypeSystems = this.typeSystems.GetOneTypeSystems(entity.GetType());
        if (oneTypeSystems == null) return;

        for (int i = 0; i < oneTypeSystems.QueueFlag.Length; ++i)
        {
            if (!oneTypeSystems.QueueFlag[i]) continue;
            this.queues[i].Enqueue(entity);
        }
    }
    
    public void Update()
    {
        Queue<EntityRef<Entity>> queue = this.queues[InstanceQueueIndex.Update];
        int count = queue.Count;
        while (count-- > 0)
        {
            Entity component = queue.Dequeue();
            if (component == null)
            {
                continue;
            }

            if (component.IsDisposed)
            {
                continue;
            }
                
            if (component is not IUpdate)
            {
                continue;
            }

            List<SystemObject> iUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IUpdateSystem));
            if (iUpdateSystems == null)
            {
                continue;
            }

            queue.Enqueue(component);

            foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
            {
                try
                {
                    iUpdateSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

        }
    }

    public void LateUpdate()
    {
        Queue<EntityRef<Entity>> queue = this.queues[InstanceQueueIndex.LateUpdate];
        int count = queue.Count;
        while (count-- > 0)
        {
            Entity component = queue.Dequeue();
            if (component == null)
            {
                continue;
            }

            if (component.IsDisposed)
            {
                continue;
            }
                
            if (component is not ILateUpdate)
            {
                continue;
            }

            List<SystemObject> iLateUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILateUpdateSystem));
            if (iLateUpdateSystems == null)
            {
                continue;
            }

            queue.Enqueue(component);

            foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
            {
                try
                {
                    iLateUpdateSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
    
    public void Serialize(Entity entity)
    {
        List<SystemObject> iSerializeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (ISerializeSystem));
        if (iSerializeSystems == null) return;

        foreach (ISerializeSystem serializeSystem in iSerializeSystems)
        {
            try
            {
                serializeSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public void Deserialize(Entity entity)
    {
        List<SystemObject> iDeserializeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IDeserializeSystem));
        if (iDeserializeSystems == null) return;

        foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
        {
            try
            {
                deserializeSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public void Start(Entity entity)
    {
        List<SystemObject> iStartSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IStartSystem));
        if (iStartSystems == null) return;

        foreach (IStartSystem startSystem in iStartSystems)
        {
            try
            {
                startSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public void Awake(Entity entity)
    {
        List<SystemObject> iAwakeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IAwakeSystem));
        if (iAwakeSystems == null) return;

        foreach (IAwakeSystem awakeSystem in iAwakeSystems)
        {
            try
            {
                awakeSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void Awake<A>(Entity entity, A a)
    {
        List<SystemObject> iAwakeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IAwakeSystem<A>));
        if (iAwakeSystems == null) return;

        foreach (IAwakeSystem<A> awakeSystem in iAwakeSystems)
        {
            try
            {
                awakeSystem.Run(entity, a);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void Awake<A, B>(Entity entity, A a, B b)
    {
        List<SystemObject> iAwakeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IAwakeSystem<A, B>));
        if (iAwakeSystems == null) return;

        foreach (IAwakeSystem<A, B> awakeSystem in iAwakeSystems)
        {
            try
            {
                awakeSystem.Run(entity, a, b);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void Awake<A, B, C>(Entity entity, A a, B b, C c)
    {
        List<SystemObject> iAwakeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IAwakeSystem<A, B, C>));
        if (iAwakeSystems == null) return;

        foreach (IAwakeSystem<A, B, C> awakeSystem in iAwakeSystems)
        {
            try
            {
                awakeSystem.Run(entity, a, b, c);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public void Awake<A, B, C, D>(Entity entity, A a, B b, C c, D d)
    {
        List<SystemObject> iAwakeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IAwakeSystem<A, B, C, D>));
        if (iAwakeSystems == null) return;

        foreach (IAwakeSystem<A, B, C, D> awakeSystem in iAwakeSystems)
        {
            try
            {
                awakeSystem.Run(entity, a, b, c, d);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void Awake<A, B, C, D, E>(Entity entity, A a, B b, C c, D d, E e)
    {
        List<SystemObject> iAwakeSystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IAwakeSystem<A, B, C, D, E>));
        if (iAwakeSystems == null) return;

        foreach (IAwakeSystem<A, B, C, D, E> awakeSystem in iAwakeSystems)
        {
            try
            {
                awakeSystem.Run(entity, a, b, c, d, e);
            }
            catch (Exception err)
            {
                Log.Error(err);
            }
        }
    }

    public void Destroy(Entity entity)
    {
        List<SystemObject> iDestroySystems = this.typeSystems.GetSystems(entity.GetType(), typeof (IDestroySystem));
        if (iDestroySystems == null) return;

        foreach (IDestroySystem destroySystem in iDestroySystems)
        {
            try
            {
                destroySystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}