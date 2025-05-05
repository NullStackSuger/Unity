using UnityEngine.Events;

namespace UnityEngine;

public class FileSystem
{
    public FileSystem(string path)
    {
        EventSystem.Add(new FileRebuildEventHandler());
        
        root = TreeNode<FileInfo>.Root(new FileInfo(path));
        EventSystem.PublishAsync(new FileRebuildEvent(){ node = root });
    }
    
    static void BuildNode(TreeNode<FileInfo> node)
    {
        FileInfo info = node;
        bool isDirectory = info.IsDirectory;
        if (!isDirectory) return;

        foreach (var entry in Directory.GetFileSystemEntries(info.Path))
        {
            var child = node.AddChild(new FileInfo($"{Path.GetFullPath(entry)}"));
            BuildNode(child);
        }
    }
    
    private readonly TreeNode<FileInfo> root;

    public static implicit operator TreeNode<FileInfo>(FileSystem fileSystem)
    {
        return fileSystem.root;
    }
        
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
    }
    
    private class FileRebuildEventHandler : AEvent<FileRebuildEvent>
    {
        protected override async Task Run(FileRebuildEvent a)
        {
            a.node.Clear();
            BuildNode(a.node);
            await Task.CompletedTask;
        }
    }
}