namespace UnityEngine;

public class FileSystem
{
    public FileSystem(string path)
    {
        root = TreeNode<FileInfo>.Root(new FileInfo(path));
        
        // TODO Test
        var fold = root.AddChild(new FileInfo($"{root.value.Path}/TestFlod"));
        fold.AddChild(new FileInfo($"{fold.value.Path}/FileInFlod.aa"));
        root.AddChild(new FileInfo($"{root.value.Path}/TestFile.txt"));
    }
    
    // 这里我直接定期Build了, 因为文件系统比Scene复杂的多, 这样出错可能最小
    public void Build()
    {
        
    }
    
    private readonly TreeNode<FileInfo> root;

    public static implicit operator TreeNode<FileInfo>(FileSystem fileSystem)
    {
        return fileSystem.root;
    }
        
    public class FileInfo
    {
        public readonly string Path;
        public string Name => System.IO.Path.GetFileName(Path);
        public bool IsDirectory => Directory.Exists(Path);
        public bool IsFile => File.Exists(Path);

        public FileInfo(string path)
        {
            this.Path = path;
        }
    }
}