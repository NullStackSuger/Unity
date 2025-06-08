using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public class BlackBoard
{
    [BsonElement]
    [BsonIgnoreIfNull]
    private Dictionary<string, object> dict = new();

    public T Get<T>(string key)
    {
        if (!this.dict.TryGetValue(key, out object value))
        {
            throw new Exception($"BlackBoard not found key: {key} {typeof(T).FullName}");
        }
        
        // 这里值类型拆箱效率有问题, 用对象池优化
        return (T)value;
    }

    public void Add(string key, object value)
    {
        if (this.dict.TryGetValue(key, out object oldValue))
        {
            throw new Exception($"BlackBoard already exist key : {key} {oldValue.GetType().FullName}");
        }
        
        this.dict.Add(key, value);
    }

    // Set用于设置value会改变的
    public void Set(string key, object value)
    {
        this.dict[key] = value;
    }

    public bool Has(string key)
    {
        return this.dict.ContainsKey(key);
    }
}