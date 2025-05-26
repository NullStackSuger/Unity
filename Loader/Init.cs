using System.Numerics;
using System.Reflection;

namespace ET;

public static class Init
{
    public static void Main(string[] args)
    {
        Start();

        GameObject scene0 = new();
        scene0.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(Vector3.Zero, Quaternion.Identity, Vector3.One);
        GameObject light = scene0.AddChild();
        light.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 10, 10), new Vector3(90, 0, 0).ToQuaternion(), Vector3.One);
        light.AddComponent<DirectionLightComponent, float, Color>(0.8f, Color.White);
        GameObject camera = scene0.AddChild();
        camera.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 0, -2.5f), Quaternion.Identity, Vector3.One);
        camera.AddComponent<PerspectiveCameraComponent, float, float, float, float>(60, 4f / 3f, 0.1f, 100f);
        GameObject cube = scene0.AddChild();
        cube.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 0, 10), Quaternion.Identity, Vector3.One);
        GameObject plane = scene0.AddChild();
        plane.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, -3, 10), new Vector3(-20, 0, 0).ToQuaternion(), new Vector3(3, 0.1f, 3));
        
        Scene.Instance.ActiveScene = scene0;
        
        //AssetFileSystem.Instance.Save(Path.Combine("Scene", "Scene0.prefab"), scene0);

        //GameObject scene0 = Scene.Instance.ActiveScene;
        
        // Start 正常是要放在Game循环里, 而不是Editor
        foreach (GameObject obj in scene0.Foreach())
        {
            foreach (Component component in obj.Components)
            {
                EntitySystem.Instance.Start(component);
            }
        }

        while (Window.Instance.Exist)
        {
            Time.Instance.Update();
            // 正常是要放在Game循环里, 而不是Editor
            Input.Instance.Update(Window.Instance.InputSnapshot);
            EntitySystem.Instance.Update();
            EntitySystem.Instance.LateUpdate();
            RenderSystem.Instance.Update();
        }
        
        Scene.Instance.Dispose();
        World.Instance.Dispose();
    }

    private static void Start()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Error(e.ExceptionObject.ToString());
        };
        ETTask.ExceptionHandler += Log.Error;
        
        World.Instance.AddSingleton<CodeLoader>();
        World.Instance.AddSingleton<CodeTypes, Assembly[]>(CodeLoader.Instance.Assemblies.ToArray());
        World.Instance.AddSingleton<EventSystem>();
        World.Instance.AddSingleton<Time>();
        World.Instance.AddSingleton<IdGenerator>();
        World.Instance.AddSingleton<EntitySystem>();
        World.Instance.AddSingleton<AssetFileSystem>().Rebuild();
        World.Instance.AddSingleton<RenderSystem>();
        World.Instance.AddSingleton<Scene>();//.ActiveScene = AssetFileSystem.Instance.Load<GameObject>(Path.Combine("Scene", "Scene0.prefab"));
        World.Instance.AddSingleton<Input>();
        World.Instance.AddSingleton<Window>();
    }
}