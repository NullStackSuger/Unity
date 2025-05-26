namespace ET;

public interface IUpdate
{
}
	
public interface IUpdateSystem: ISystem
{
	void Run(Entity o);
}

[EntitySystem]
public abstract class UpdateSystem<T> : SystemObject, IUpdateSystem where T: Entity, IUpdate
{
	void IUpdateSystem.Run(Entity o)
	{
		this.Update((T)o);
	}

	Type ISystem.Type()
	{
		return typeof(T);
	}

	Type ISystem.SystemType()
	{
		return typeof(IUpdateSystem);
	}

	int ISystem.GetInstanceQueueIndex()
	{
		return InstanceQueueIndex.Update;
	}

	protected abstract void Update(T self);
}