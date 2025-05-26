namespace ET;

public interface IStart
{
}
	
public interface IStartSystem: ISystem
{
	void Run(Entity o);
}

[EntitySystem]
public abstract class StartSystem<T> : SystemObject, IStartSystem where T: Entity, IStart
{
	void IStartSystem.Run(Entity o)
	{
		this.Start((T)o);
	}

	Type ISystem.Type()
	{
		return typeof(T);
	}

	Type ISystem.SystemType()
	{
		return typeof(IStartSystem);
	}

	int ISystem.GetInstanceQueueIndex()
	{
		return InstanceQueueIndex.Update;
	}

	protected abstract void Start(T self);
}