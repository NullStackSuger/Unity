using BulletSharp;
using BulletSharp.Math;

namespace ET;

public static class RigidBodyComponentSystem
{
    [EntitySystem]
    private class ET_RigidBodyComponentSystem_AwakeSystem : AwakeSystem<RigidBodyComponent, float>
    {
        protected override void Awake(RigidBodyComponent self, float mass)
        {
            self.mass = mass;
            self.Init();

            PhysicsSceneComponent physicsSceneComponent = Scene.Instance.ActiveScene.GetComponent<PhysicsSceneComponent>();
            physicsSceneComponent.world.AddRigidBody(self.rigidBody);
        }
    }
    
    [EntitySystem]
    private class ET_RigidBodyComponentSystem_DeserializeSystem : DeserializeSystem<RigidBodyComponent>
    {
        protected override void Deserialize(RigidBodyComponent self)
        {
            self.Init();

            PhysicsSceneComponent physicsSceneComponent = Scene.Instance.ActiveScene.GetComponent<PhysicsSceneComponent>();
            physicsSceneComponent.world.AddRigidBody(self.rigidBody);
        }
    }
    
    [EntitySystem]
    private class ET_RigidBodyComponentSystem_UpdateSystem : UpdateSystem<RigidBodyComponent>
    {
        protected override void Update(RigidBodyComponent self)
        {
            TransformComponent transformComponent = self.GameObject.GetComponent<TransformComponent>();
            transformComponent.WorldPosition = self.rigidBody.WorldTransform.Origin.ToNumerics();
            transformComponent.rotation = self.rigidBody.WorldTransform.Quaternion().ToNumerics();
        }
    }

    private static void Init(this RigidBodyComponent self)
    {
        GameObject obj = self.GameObject;
        TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
        CollisionComponent collisionComponent = obj.GetComponentHard<CollisionComponent>();
        
        Vector3 inertia = self.mass == 0 ? Vector3.Zero : collisionComponent.shape.CalculateLocalInertia(self.mass);
        var motionState = new DefaultMotionState(Matrix.RotationQuaternion(transformComponent.rotation.ToBullet()) * Matrix.Translation(transformComponent.position.ToBullet()));
        self.rigidBody = new RigidBody(new RigidBodyConstructionInfo(self.mass, motionState, collisionComponent.shape, inertia));
        self.rigidBody.UserObject = self;
    }
}