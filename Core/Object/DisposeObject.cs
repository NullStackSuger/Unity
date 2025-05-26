using System.ComponentModel;

namespace ET;

public abstract class DisposeObject: Object, IDisposable
{
    public virtual void Dispose()
    {
    }
    
    /// <summary>
    /// 序列化之前调用
    /// </summary>
    public virtual void BeginInit()
    {
    }
        
    /// <summary>
    /// 序列化之后调用
    /// </summary>
    public virtual void EndInit()
    {
    }
}