using UnityEngine.Events;

namespace UnityEngine;

public class Scene
{
    public Scene(string name, bool isActive = true)
    {
        EventSystem.Add(new SceneRebuildEventHandler());
        
        root = TreeNode<SceneObjectInfo>.Root(new SceneObjectInfo(new GameObject(name)));
        if (isActive) ActiveScene = this;
        
        GameObject camera = new Camera(true, "Main Camera", true);

        EventSystem.PublishAsync(new SceneRebuildEvent(){ node = root });
    }
    
    private static void BuildNode(TreeNode<SceneObjectInfo> node)
    {
        SceneObjectInfo info = node;

        foreach (GameObject obj in info.gameObject.Children)
        {
            if (obj == null || obj.IsDispose) continue;
            var child = node.AddChild(new SceneObjectInfo(obj));
            BuildNode(child);
        }
    }

    public void Tick()
    {
        GameObject gameObject = this;
        gameObject.Tick();
    }

    private readonly TreeNode<SceneObjectInfo> root;

    public static Scene ActiveScene { get; private set; }

    public static implicit operator TreeNode<SceneObjectInfo>(Scene scene)
    {
        return scene?.root;
    }
    public static implicit operator SceneObjectInfo(Scene scene)
    {
        return scene?.root?.Value;
    }
    public static implicit operator GameObject(Scene scene)
    {
        return scene?.root?.Value?.gameObject;
    }

    public class SceneObjectInfo
    {
        public readonly GameObject gameObject;

        public SceneObjectInfo(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public static implicit operator GameObject(SceneObjectInfo info)
        {
            return info.gameObject;
        }
    }
    
    private class SceneRebuildEventHandler : AEvent<SceneRebuildEvent>
    {
        protected override async Task Run(SceneRebuildEvent a)
        {
            a.node.Clear();
            BuildNode(a.node);
            await Task.CompletedTask;
        }
    }
}