using System.Numerics;
using System.Reflection;

namespace ET;

public static class Init
{
    public static void Main(string[] args)
    {
        Start();

        #region Build Objs
        Scene.Instance.ActiveScene = new GameObject();
        Scene.Instance.ActiveScene.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(Vector3.Zero, Quaternion.Identity, Vector3.One);
        Scene.Instance.ActiveScene.AddComponent<PhysicsSceneComponent, float>(-10);
        
        GameObject light = Scene.Instance.ActiveScene.AddChild();
        light.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 10, 10), new Vector3(90, 0, 0).ToQuaternion(), Vector3.One);
        light.AddComponent<DirectionLightComponent, float, Color>(0.8f, Color.White);
        
        GameObject camera = Scene.Instance.ActiveScene.AddChild();
        camera.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 0, -2.5f), Quaternion.Identity, Vector3.One);
        camera.AddComponent<PerspectiveCameraComponent, float, float, float, float>(60, 4f / 3f, 0.1f, 100f);
        
        GameObject ball = Scene.Instance.ActiveScene.AddChild();
        ball.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 0, 10), Quaternion.Identity, Vector3.One);
        ball.AddComponent<SphereCollisionComponent, float>(0.5f);
        ball.AddComponent<RigidBodyComponent, float>(1);
        ball.AddComponent<TestComponent>();
        
        GameObject plane = Scene.Instance.ActiveScene.AddChild();
        plane.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, -3, 10), new Vector3(20, 0, 0).ToQuaternion(), new Vector3(3, 0.1f, 3));
        plane.AddComponent<BoxCollisionComponent, Vector3>(new Vector3(3, 0.1f, 3));
        plane.AddComponent<RigidBodyComponent, float>(0);
        #endregion
        
        if (Options.IsEditor)
        {
            int count = 300;
            while (Window.Instance.Exist)
            {
                Time.Instance.Update();
                Input.Instance.Update(Window.Instance.InputSnapshot);
                EntitySystem.Instance.Update();
                EntitySystem.Instance.LateUpdate();
                RenderSystem.Instance.Update();
                
                if (--count < 0) break;
            }
        }
        else
        {
            // Start
            foreach (GameObject obj in Scene.Instance.ActiveScene.Foreach())
            {
                foreach (Component component in obj.Components)
                {
                    EntitySystem.Instance.Start(component);
                }
            }
            
            while (Window.Instance.Exist)
            {
                Time.Instance.Update();
                Input.Instance.Update(Window.Instance.InputSnapshot);
                EntitySystem.Instance.Update();
                EntitySystem.Instance.LateUpdate();
                RenderSystem.Instance.Update();
            }
        }
        
        Scene.Instance.Dispose();
        World.Instance.Dispose();
        
        //AssetFileSystem.Instance.Save(Path.Combine("Scene", "Scene0.prefab"), Scene.Instance.ActiveScene);
        //Scene.Instance.ActiveScene = AssetFileSystem.Instance.Load<GameObject>(Path.Combine("Scene", "Scene0.prefab"));
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
        World.Instance.AddSingleton<Scene>()/*.ActiveScene = AssetFileSystem.Instance.Load<GameObject>(Path.Combine("Scene", "Scene0.prefab"))*/;
        World.Instance.AddSingleton<Input>();
        World.Instance.AddSingleton<Window>();
    }
}