namespace UnityEngine;

public class Scene
{
    public Scene(string name)
    {
        root = TreeNode<SceneObjectInfo>.Root(new SceneObjectInfo(new GameObject(name)));
        ActiveScene = this;
    }

    public void Tick()
    {
        
    }

    private readonly TreeNode<SceneObjectInfo> root;

    public static Scene ActiveScene { get; private set; }

    public static implicit operator TreeNode<SceneObjectInfo>(Scene scene)
    {
        return scene?.root;
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
    }
}