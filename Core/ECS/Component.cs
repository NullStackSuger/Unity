using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public abstract class Component : Entity
{
    private GameObject gameObject;
    [BsonIgnore]
    public GameObject GameObject
    {
        get => this.gameObject;
        set
        {
            if (value == null) Log.Error($"cant set gameObject null: {this.GetType().FullName}");

            // 之前有parent
            if (this.gameObject != null) Log.Error($"cant change component gameObject {this.GetType().FullName} GameObject: {this.gameObject.GetType().FullName}");
            
            this.gameObject = value;
            // 这个set只会调用一次, 在这里设置id
            this.Id = this.gameObject!.Id;
        }
    }

    public override void OnDeserialize()
    {
        base.OnDeserialize();
        
        EntitySystem.Instance.RegisterSystem(this);
    }
}