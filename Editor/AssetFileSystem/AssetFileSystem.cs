namespace ET;

public class AssetFileSystem : Singleton<AssetFileSystem>, ISingletonAwake
{
    public static readonly string AssetPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Asset"));
    
    private readonly string[] followFile = [".prefab"];
    private readonly string[] ignoreFile = [".meta"];
    // (后缀名, paths)
    private readonly Dictionary<string, List<string>> files = new();
    private AssetData root;

    public void Awake()
    {
        
    }

    public void Rebuild()
    {
        this.files.Clear();
        this.root = Build(AssetPath);
        
        AssetData Build(string path)
        {
            var data = new AssetData()
            {
                path = path,
            };
            
            if (Directory.Exists(path))
            {
                // 子文件夹
                foreach (string subPath in Directory.GetDirectories(path))
                {
                    data.children.Add(Build(subPath));
                }   
                
                // 子文件
                foreach (string subPath in Directory.GetFiles(path))
                {
                    string extension = Path.GetExtension(subPath);
                    if (this.ignoreFile.Contains(extension)) continue;
                    if (this.followFile.Contains(extension))
                    {
                        this.files.TryAdd(extension, new List<string>());
                        this.files[extension].Add(subPath);
                    }
                    
                    data.children.Add(new AssetData()
                    {
                        path = subPath,
                    });
                }
            }
            
            return data;
        }
    }

    public IEnumerable<string> Foreach()
    {
        return Foreach(this.root);
    }
    private static IEnumerable<string> Foreach(AssetData data)
    {
        yield return data.path;

        foreach (AssetData child in data.children)
        {
            foreach (string sub in Foreach(child))
            {
                yield return sub;
            }
        }
    }
    public void Foreach(Action<string> begin, Action<string> end, Func<string, int, bool> dirBegin, Action<string, int> dirEnd, Action<string> file, Action error)
    {
        Foreach(this.root, begin, end, dirBegin, dirEnd, file, error);
    }
    private void Foreach(AssetData data, Action<string> begin, Action<string> end, Func<string, int, bool> dirBegin, Action<string, int> dirEnd, Action<string> file, Action error)
    {
        begin?.Invoke(data.path);

        if (File.Exists(data.path))
        {
            file?.Invoke(data.path);
        }
        else if (Directory.Exists(data.path) && (dirBegin?.Invoke(data.path, data.children.Count) ?? true))
        {
            foreach (AssetData child in data.children)
            {
                Foreach(child, begin, end, dirBegin, dirEnd, file, error);
            }
            
            dirEnd?.Invoke(data.path, data.children.Count);
        }
        else
        {
            // 文件被删除了
            error?.Invoke();
        }
        
        end?.Invoke(data.path);
    }

    public T Load<T>(string path)
    {
        byte[] res = File.ReadAllBytes(Path.Combine(AssetPath, path));
        T t = MongoHelper.Deserialize<T>(res);
        return t;
    }

    public void Save(string path, object t)
    {
        byte[] res = MongoHelper.Serialize(t);
        File.WriteAllBytes(Path.Combine(AssetPath, path), res);
    }
    
    private class AssetData
    {
        public string path;
        public readonly List<AssetData> children = new();
    }
}