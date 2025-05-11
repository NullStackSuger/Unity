using System.Numerics;

namespace UnityEngine;

public class Light
{
    public Light(string name = "Main Light", bool isMain = false)
    {
        gameObject = new GameObject(name);
        directionLightComponent = gameObject.AddComponent<DirectionLightComponent>();
        
        if (isMain) Main = this;
    }
    
    private readonly GameObject gameObject;
    private readonly DirectionLightComponent directionLightComponent;
    
    public Matrix4x4 View => directionLightComponent.View();
    public Matrix4x4 Projection => directionLightComponent.Projection();
    
    public static Light Main;
    
    public static implicit operator GameObject(Light camera)
    {
        return camera.gameObject;
    }
    public static implicit operator DirectionLightComponent(Light camera)
    {
        return camera.directionLightComponent;
    }
}