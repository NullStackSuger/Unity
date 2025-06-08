using System.Numerics;

namespace ET;

public class BoxCollisionComponent : CollisionComponent, IAwake<Vector3>, IDeserialize
{
    public Vector3 size;
}