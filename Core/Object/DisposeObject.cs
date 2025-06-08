namespace ET;

public abstract class DisposeObject: Object, IDisposable
{
    public virtual void Dispose()
    {
    }
    
    /// <summary>
    /// 序列化之前调用
    /// </summary>
    public virtual void OnSerialize()
    {
    }
        
    /// <summary>
    /// 反序列化之后调用
    /// </summary>
    public virtual void OnDeserialize()
    {
    }
}