namespace ET;

public class TypeSystems
{
    public class OneTypeSystems
    {
        public OneTypeSystems(int count)
        {
            this.QueueFlag = new bool[count];
        }
            
        public readonly UnOrderMultiMap<Type, SystemObject> Map = new();
        // 这里不用hash，数量比较少，直接for循环速度更快
        public readonly bool[] QueueFlag;
    }

    private readonly int count;

    public TypeSystems(int count)
    {
        this.count = count;
    }
        
    private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

    public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
    {
        this.typeSystemsMap.TryGetValue(type, out var systems);
        if (systems != null)
        {
            return systems;
        }

        systems = new OneTypeSystems(this.count);
        this.typeSystemsMap.Add(type, systems);
        return systems;
    }

    public OneTypeSystems GetOneTypeSystems(Type type)
    {
        this.typeSystemsMap.TryGetValue(type, out var systems);
        return systems;
    }

    public List<SystemObject> GetSystems(Type type, Type systemType)
    {
        if (!this.typeSystemsMap.TryGetValue(type, out var oneTypeSystems))
        {
            return null;
        }

        return oneTypeSystems.Map.GetValueOrDefault(systemType);
    }
}