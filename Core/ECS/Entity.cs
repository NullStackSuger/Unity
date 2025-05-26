using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public abstract class Entity : DisposeObject
{
    [BsonId]
    [BsonElement]
    public long Id { get; protected set; }
    
    [BsonIgnore]
    public long InstanceId { get; protected set; }
    
    [BsonIgnore]
    public bool IsDisposed => this.InstanceId == 0;

    protected Entity()
    {
        this.InstanceId = IdGenerator.Instance.GenerateInstanceId();
    }
    
    public override void Dispose()
    {
        if (this.IsDisposed) return;
        
        // 触发Destroy事件
        if (this is IDestroy)
        {
            EntitySystem.Instance.Destroy(this);
        }

        this.Id = 0;
        this.InstanceId = 0;
    }

    public override void BeginInit()
    {
        // 触发Serialize事件
        if (this is ISerialize)
        {
            EntitySystem.Instance.Serialize(this);
        }
    }

    public override void EndInit()
    {
        // 触发DeSerialize事件
        if (this is IDeserialize)
        {
            EntitySystem.Instance.Deserialize(this);
        }
    }
}