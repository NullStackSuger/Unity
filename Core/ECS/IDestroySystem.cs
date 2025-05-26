namespace ET;

public interface IDestroy
{
}
	
public interface IDestroySystem: ISystem
{
	void Run(Entity o);
}

[EntitySystem]
public abstract class DestroySystem<T> : SystemObject, IDestroySystem where T: Entity, IDestroy
{
	void IDestroySystem.Run(Entity o)
	{
		this.Destroy((T)o);
	}

	Type ISystem.SystemType()
	{
		return typeof(IDestroySystem);
	}

	int ISystem.GetInstanceQueueIndex()
	{
		return InstanceQueueIndex.None;
	}

	Type ISystem.Type()
	{
		return typeof(T);
	}

	protected abstract void Destroy(T self);
}