namespace ET;

public static partial class PerspectiveCameraComponentSystem
{
    [EntitySystem]
    private class ET_PerspectiveCameraComponent_AwakeSystem : AwakeSystem<ET.PerspectiveCameraComponent, float, float, float, float>
    {
        protected override void Awake(PerspectiveCameraComponent self, float fovY, float aspect, float near, float far)
        {
            self.fovY = fovY;
            self.aspect = aspect;
            self.near = near;
            self.far = far;
        }
    }
}