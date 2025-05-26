using System.Numerics;

namespace ET;

public static partial class DirectionLightComponentSystem
{
    [EntitySystem]
    private class ET_DirectionLightComponent_AwakeSystem : AwakeSystem<DirectionLightComponent, float, Color>
    {
        protected override void Awake(DirectionLightComponent self, float intensity, Color color)
        {
            self.intensity = intensity;
            self.color = color;

            self.GameObject.AddComponent<OrthographicCameraComponent, Vector2, float, float>(new Vector2(5, 5), 0.1f, 100f);
        }
    }
}