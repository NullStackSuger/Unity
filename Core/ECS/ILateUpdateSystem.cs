namespace ET;

public interface ILateUpdate
{
}
	
public interface ILateUpdateSystem: ISystem
{
	void Run(Entity o);
}

[EntitySystem]
public abstract class LateUpdateSystem<T> : SystemObject, ILateUpdateSystem where T: Entity, ILateUpdate
{
	void ILateUpdateSystem.Run(Entity o)
	{
		this.LateUpdate((T)o);
	}

	Type ISystem.Type()
	{
		return typeof(T);
	}

	Type ISystem.SystemType()
	{
		return typeof(ILateUpdateSystem);
	}

	int ISystem.GetInstanceQueueIndex()
	{
		return InstanceQueueIndex.LateUpdate;
	}

	protected abstract void LateUpdate(T self);
}