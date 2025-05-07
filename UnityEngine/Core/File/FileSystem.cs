using UnityEngine.Events;

namespace UnityEngine;

public static class FileSystem
{
    static FileSystem()
    {
        root = TreeNode<FileInfo>.Root(new FileInfo(Define.AssetPath));

        Build();
    }

    public static void Build()
    {
        root.Clear();
        foreach (string expand in Define.LikeFileTypes)
        {
            likeFileTypes[expand] = new HashSet<string>();
        }
        BuildNode(root, ref likeFileTypes);
    }
    private static void BuildNode(TreeNode<FileInfo> node, ref Dictionary<string, HashSet<string>> likeFileTypes)
    {
        FileInfo info = node;

        foreach (var entry in Directory.GetFileSystemEntries(info.Path))
        {
            if (Directory.Exists(entry))
            {
                var child = node.AddChild(new FileInfo($"{Path.GetFullPath(entry)}"));
                BuildNode(child, ref likeFileTypes);  
            }
            else
            {
                string extension = Path.GetExtension(entry);
                
                if (Define.UnlikeFileTypes.Contains(extension))
                {
                    continue;
                }
                
                if (Define.LikeFileTypes.Contains(extension))
                {
                    likeFileTypes[extension].Add(entry);
                }
                
                node.AddChild(new FileInfo($"{Path.GetFullPath(entry)}"));
            }
        }
    }

    public static IReadOnlySet<string> GetLikeFiles(string extension)
    {
        return likeFileTypes[extension];
    }
    
    public static readonly TreeNode<FileInfo> root;
    /// <summary>
    /// 扩展名, 路径
    /// </summary>
    private static Dictionary<string, HashSet<string>> likeFileTypes = new();
        
    public class FileInfo
    {
        public string Path;
        public string Name => System.IO.Path.GetFileName(Path);
        public bool IsDirectory => Directory.Exists(Path);
        public bool IsFile => File.Exists(Path);

        public FileInfo(string path)
        {
            this.Path = path;
        }

        public static implicit operator string(FileInfo info)
        {
            return info.Path;
        }
    }
}