namespace UnityEngine;

public class Scene
{
    public Scene(string name)
    {
        // TODO 
        root = TreeNode<SceneObjectInfo>.Root(new SceneObjectInfo(new GameObject(){ name = name }));
    }

    public void Tick()
    {
        
    }

    private readonly TreeNode<SceneObjectInfo> root;

    public static implicit operator TreeNode<SceneObjectInfo>(Scene scene)
    {
        return scene.root;
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