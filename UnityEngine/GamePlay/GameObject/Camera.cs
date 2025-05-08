using System.Numerics;

namespace UnityEngine;

public class Camera
{
    public Camera(bool isPerspective = true, string name = "Main Camera", bool isMain = false)
    {
        gameObject = new GameObject(name);
        cameraComponent = isPerspective ? gameObject.AddComponent<PerspectiveCameraComponent>() : gameObject.AddComponent<OrthographicCameraComponent>();
        Frustum = new(Projection * View);
        
        if (isMain) Main = this;
    }
    
    private readonly GameObject gameObject;
    private readonly CameraComponent cameraComponent;
    public readonly Frustum Frustum;

    public Matrix4x4 View => cameraComponent.View();
    public Matrix4x4 Projection => cameraComponent.Projection();

    public static Camera Main;

    public static implicit operator GameObject(Camera camera)
    {
        return camera.gameObject;
    }
    public static implicit operator CameraComponent(Camera camera)
    {
        return camera.cameraComponent;
    }
}