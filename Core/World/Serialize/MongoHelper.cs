using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace ET;

public static class MongoHelper
{
    private static readonly JsonWriterSettings defaultSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };

    static MongoHelper()
    {
        // 自动注册IgnoreExtraElements
        ConventionPack conventionPack = [new IgnoreExtraElementsConvention(true)];

        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

        // TODO 暂时让ObjectSerializer去序列化所有obj, 但是ET不是这样写的, 具体怎么写的我找不到了
        BsonSerializer.RegisterSerializer(typeof(object), new ObjectSerializer(type => true));

        Dictionary<string, Type> types = CodeTypes.Instance.GetTypes();
        foreach (Type type in types.Values)
        {
            if (!type.IsSubclassOf(typeof(Object)))
            {
                continue;
            }

            if (type.IsGenericType)
            {
                continue;
            }

            BsonClassMap.LookupClassMap(type);
        }
    }

    private static void RegisterStruct<T>() where T : struct
    {
        BsonSerializer.RegisterSerializer(typeof(T), new StructBsonSerialize<T>());
    }

    public static string ToJson(object obj)
    {
        if (obj is DisposeObject disposeObject)
        {
            disposeObject.OnSerialize();
        }
        return obj.ToJson(defaultSettings);
    }

    public static string ToJson(object obj, JsonWriterSettings settings)
    {
        if (obj is DisposeObject disposeObject)
        {
            disposeObject.OnSerialize();
        }
        return obj.ToJson(settings);
    }

    public static T FromJson<T>(string str)
    {
        try
        {
            T obj = BsonSerializer.Deserialize<T>(str);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"{str}\n{e}");
        }
    }

    public static object FromJson(Type type, string str)
    {
        object obj = BsonSerializer.Deserialize(str, type);
        if (obj is DisposeObject disposeObject)
        {
            disposeObject.OnDeserialize();
        }
        return obj;
    }

    public static byte[] Serialize(object obj)
    {
        if (obj is DisposeObject disposeObject)
        {
            disposeObject.OnSerialize();
        }
        return obj.ToBson();
    }

    public static void Serialize(object message, MemoryStream stream)
    {
        if (message is DisposeObject disposeObject)
        {
            disposeObject.OnSerialize();
        }
        
        using BsonBinaryWriter bsonWriter = new(stream, BsonBinaryWriterSettings.Defaults);

        BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
        BsonSerializationArgs args = default;
        args.NominalType = typeof(object);
        IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
        serializer.Serialize(context, args, message);
    }

    public static object Deserialize(Type type, byte[] bytes)
    {
        try
        {
            object obj = BsonSerializer.Deserialize(bytes, type);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {type.FullName} {bytes.Length}", e);
        }
    }

    public static object Deserialize(Type type, byte[] bytes, int index, int count)
    {
        try
        {
            using MemoryStream memoryStream = new(bytes, index, count);

            object obj = BsonSerializer.Deserialize(memoryStream, type);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {type.FullName} {bytes.Length} {index} {count}", e);
        }
    }

    public static object Deserialize(Type type, Stream stream)
    {
        try
        {
            object obj = BsonSerializer.Deserialize(stream, type);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {type.FullName} {stream.Position} {stream.Length}", e);
        }
    }

    public static T Deserialize<T>(byte[] bytes)
    {
        try
        {
            using MemoryStream memoryStream = new(bytes);

            T obj = (T)BsonSerializer.Deserialize(memoryStream, typeof(T));
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {typeof(T).FullName} {bytes.Length}", e);
        }
    }

    public static T Deserialize<T>(byte[] bytes, int index, int count)
    {
        return (T)Deserialize(typeof(T), bytes, index, count);
    }

    public static T Clone<T>(T t)
    {
        return Deserialize<T>(Serialize(t));
    }
}