using System.Numerics;

namespace ET;

public static partial class TransformComponentSystem
{
    [EntitySystem]
    private class ET_TransformComponent_AwakeSystem : AwakeSystem<TransformComponent, Vector3, Quaternion, Vector3>
    {
        protected override void Awake(TransformComponent self, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            self.position = position;
            self.rotation = rotation;
            self.scale = scale;
        }
    }
}