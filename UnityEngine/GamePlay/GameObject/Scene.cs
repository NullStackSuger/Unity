using System.Numerics;

namespace UnityEngine;

public class Scene
{
    public Scene(string name, bool isActive = true)
    {
        root = TreeNode<SceneObjectInfo>.Root(new SceneObjectInfo(new GameObject(name)));
        if (isActive) ActiveScene = this;
        
        GameObject camera = new Camera(true, "Main Camera", true);
        camera.transform.position = new Vector3(0, 0, -2.5f);

        GameObject light = new Light("Main Light", true);
        light.transform.position = new Vector3(0, 10, 10f);
        light.transform.rotation = new Vector3(90, 0, 0).ToQuaternion();
        
        GameObject cube = new GameObject("Cube");
        MeshComponent meshComponent = cube.AddComponent<MeshComponent>();
        meshComponent.ObjPath = FileSystem.GetLikeFiles(".obj")?.First();
        meshComponent.shadowShader = new DefaultShadowShader();
        meshComponent.objectShader = new DefaultObjectShader();
        cube.transform.position = new Vector3(0, 0, 10);
        cube.transform.rotation = new Vector3(45, 0, 45).ToQuaternion();
        
        GameObject plane = new GameObject("Plane");
        meshComponent = plane.AddComponent<MeshComponent>();
        meshComponent.ObjPath = FileSystem.GetLikeFiles(".obj")?.First();
        meshComponent.shadowShader = new DefaultShadowShader();
        meshComponent.objectShader = new DefaultObjectShader();
        plane.transform.position = new Vector3(0, -3, 10);
        plane.transform.rotation = new Vector3(-20, 0, 0).ToQuaternion();
        plane.transform.scale = new Vector3(3, 0.1f, 3);

        Build();
    }

    public void Build()
    {
        root.Clear();
        BuildNode(root);
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

    public GameObject Find(Func<GameObject, bool> func)
    {
        GameObject obj = this;
        return obj.Find(func);
    }
    public GameObject Find(string name)
    {
        return Find(obj => obj.name == name);
    }
    public IEnumerable<GameObject> Find()
    {
        GameObject obj = this;
        return obj.Find();
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
}