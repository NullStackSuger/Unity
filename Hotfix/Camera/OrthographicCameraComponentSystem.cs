using System.Numerics;

namespace ET;

public static partial class OrthographicCameraComponentSystem
{
    [EntitySystem]
    private class ET_OrthographicCameraComponent_AwakeSystem : AwakeSystem<ET.OrthographicCameraComponent, Vector2, float, float>
    {
        protected override void Awake(OrthographicCameraComponent self, Vector2 size, float near, float far)
        {
            self.left = -size.X;
            self.right = size.X;
            self.bottom = -size.Y;
            self.top = size.Y;
            self.near = near;
            self.far = far;
        }
    }
}