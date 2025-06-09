namespace ET;

public class CollisionSystem : Singleton<CollisionSystem>, ISingletonAwake
{
    // 泛型类型, System类型
    private readonly Dictionary<Type[], UnOrderMultiMap<Type, SystemObject>> allCollisionSystems = new(new TypeArrayComparer());

    private UnOrderMultiMap<Type, SystemObject> GetOrCreateOneTypeSystems(Type[] argsTypes)
    {
        this.allCollisionSystems.TryGetValue(argsTypes, out UnOrderMultiMap<Type, SystemObject> systems);
        if (systems != null)
        {
            return systems;
        }
        
        systems = new UnOrderMultiMap<Type, SystemObject>();
        this.allCollisionSystems.Add(argsTypes, systems);
        return systems;
    }

    private List<SystemObject> GetSystems(Type[] argsTypes, Type systemType)
    {
        if (!this.allCollisionSystems.TryGetValue(argsTypes, out UnOrderMultiMap<Type, SystemObject> oneTypeSystems))
        {
            return null;
        }
        
        return oneTypeSystems.GetValueOrDefault(systemType);
    }
    
    public void Awake()
    {
        foreach (Type type in CodeTypes.Instance.GetTypes(typeof(CollisionHandlerAttribute)))
        {
            SystemObject obj = (SystemObject)Activator.CreateInstance(type);

            if (obj is ICollision iCollision)
            {
                UnOrderMultiMap<Type, SystemObject> oneTypeSystems = this.GetOrCreateOneTypeSystems(iCollision.Types());
                oneTypeSystems.Add(iCollision.SystemType(), obj);
            }
        }
    }

    public void CollisionEnter(Entity self)
    {
        List<SystemObject> iCollisionEnters = this.GetSystems([self.GetType()], typeof(ICollisionEnter));
        if (iCollisionEnters == null) return;

        foreach (ICollisionEnter collisionEnter in iCollisionEnters)
        {
            if (collisionEnter == null) continue;

            try
            {
                collisionEnter.Run(self, null);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void CollisionEnter(Entity self, Entity other)
    {
        List<SystemObject> iCollisionEnterHandlers = this.GetSystems([self.GetType(), other.GetType()], typeof(ICollisionEnter));
        if (iCollisionEnterHandlers == null) return;

        foreach (ICollisionEnter collisionEnterHandler in iCollisionEnterHandlers)
        {
            if (collisionEnterHandler == null) continue;

            try
            {
                collisionEnterHandler.Run(self, other);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void CollisionStay(Entity self)
    {
        List<SystemObject> iCollisionStays = this.GetSystems([self.GetType()], typeof(ICollisionStay));
        if (iCollisionStays == null) return;

        foreach (ICollisionStay collisionStay in iCollisionStays)
        {
            if (collisionStay == null) continue;

            try
            {
                collisionStay.Run(self, null);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void CollisionStay(Entity self, Entity other)
    {
        List<SystemObject> iCollisionStayHandlers = this.GetSystems([self.GetType(), other.GetType()], typeof(ICollisionStay));
        if (iCollisionStayHandlers == null) return;

        foreach (ICollisionStay collisionStayHandler in iCollisionStayHandlers)
        {
            if (collisionStayHandler == null) continue;

            try
            {
                collisionStayHandler.Run(self, other);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void CollisionExit(Entity self)
    {
        List<SystemObject> iCollisionExits = this.GetSystems([self.GetType()], typeof(ICollisionExit));
        if (iCollisionExits == null) return;

        foreach (ICollisionExit collisionExit in iCollisionExits)
        {
            if (collisionExit == null) continue;

            try
            {
                collisionExit.Run(self, null);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void CollisionExit(Entity self, Entity other)
    {
        List<SystemObject> iCollisionExitHandlers = this.GetSystems([self.GetType(), other.GetType()], typeof(ICollisionExit));
        if (iCollisionExitHandlers == null) return;

        foreach (ICollisionExit collisionExitHandler in iCollisionExitHandlers)
        {
            if (collisionExitHandler == null) continue;

            try
            {
                collisionExitHandler.Run(self, other);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public void CollisionTestBegin(Entity self)
    {
        List<SystemObject> iCollisionTestBegins = this.GetSystems([self.GetType()], typeof(ICollisionTestBegin));
        if (iCollisionTestBegins == null) return;

        foreach (ICollisionTestBegin collisionTestBegin in iCollisionTestBegins)
        {
            if (collisionTestBegin == null) continue;

            try
            {
                collisionTestBegin.Run(self);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
    
    public void CollisionTestEnd(Entity self)
    {
        List<SystemObject> iCollisionTestEnds = this.GetSystems([self.GetType()], typeof(ICollisionTestEnd));
        if (iCollisionTestEnds == null) return;

        foreach (ICollisionTestEnd collisionTestEnd in iCollisionTestEnds)
        {
            if (collisionTestEnd == null) continue;

            try
            {
                collisionTestEnd.Run(self);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}