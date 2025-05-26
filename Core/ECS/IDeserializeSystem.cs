namespace ET;

public interface IDeserialize
{
}
	
public interface IDeserializeSystem: ISystem
{
	void Run(Entity o);
}

/// <summary>
/// 反序列化后执行的System
/// </summary>
/// <typeparam name="T"></typeparam>
[EntitySystem]
public abstract class DeserializeSystem<T> : SystemObject, IDeserializeSystem where T: Entity, IDeserialize
{
	void IDeserializeSystem.Run(Entity o)
	{
		this.Deserialize((T)o);
	}

	Type ISystem.SystemType()
	{
		return typeof(IDeserializeSystem);
	}

	int ISystem.GetInstanceQueueIndex()
	{
		return InstanceQueueIndex.None;
	}

	Type ISystem.Type()
	{
		return typeof(T);
	}

	protected abstract void Deserialize(T self);
}