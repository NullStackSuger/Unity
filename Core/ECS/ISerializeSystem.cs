namespace ET;

public interface ISerialize
{
}
	
public interface ISerializeSystem: ISystem
{
	void Run(Entity o);
}

/// <summary>
/// 序列化前执行的System
/// </summary>
/// <typeparam name="T"></typeparam>
[EntitySystem]
public abstract class SerializeSystem<T> : SystemObject, ISerializeSystem where T: Entity, ISerialize
{
	void ISerializeSystem.Run(Entity o)
	{
		this.Serialize((T)o);
	}

	Type ISystem.SystemType()
	{
		return typeof(ISerializeSystem);
	}

	int ISystem.GetInstanceQueueIndex()
	{
		return InstanceQueueIndex.None;
	}

	Type ISystem.Type()
	{
		return typeof(T);
	}

	protected abstract void Serialize(T self);
}