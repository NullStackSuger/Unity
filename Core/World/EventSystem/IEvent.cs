namespace ET;

public interface IEvent
{
	Type Type { get; }
}
	
public abstract class AEvent<A>: IEvent where A: struct
{
	public Type Type => typeof (A);

	protected abstract ETTask Run(A a);

	public async ETTask Handle(A a)
	{
		try
		{
			await Run(a);
		}
		catch (Exception e)
		{
			Log.Error(e);
		}
	}
}