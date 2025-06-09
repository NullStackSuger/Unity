using BulletSharp;
using BulletSharp.Math;
using ET.Event;

namespace ET;

public static partial class PhysicsSceneComponentSystem
{
    [EntitySystem]
    private class ET_PhysicsSceneComponent_AwakeSystem : AwakeSystem<PhysicsSceneComponent, float>
    {
        protected override void Awake(PhysicsSceneComponent self, float g)
        {
            self.g = g;
            self.Init();
        }
    }
    
    [EntitySystem]
    private class ET_PhysicsSceneComponent_DestroySystem : DestroySystem<PhysicsSceneComponent>
    {
        protected override void Destroy(PhysicsSceneComponent self)
        {
            self.world?.Dispose();
            self.solver?.Dispose();
            self.broadphase?.Dispose();
            self.dispatcher?.Dispose();
            self.config?.Dispose();
        }
    }

    [EntitySystem]
    private class ET_PhysicsSceneComponent_DeserializeSystem : DeserializeSystem<PhysicsSceneComponent>
    {
        protected override void Deserialize(PhysicsSceneComponent self)
        {
            self.Init();
        }
    }
    
    [EntitySystem]
    private class ET_PhysicsSceneComponent_UpdateSystem : UpdateSystem<PhysicsSceneComponent>
    {
        protected override void Update(PhysicsSceneComponent self)
        {
            self.nowCollision.Clear();
            
            // DeltaTime是ms, 要转成s
            // TODO 需要改成FixedUpdate
            self.world.StepSimulation(/*Time.Instance.DeltaTime / 100f*/1/60f);
            
            Dispatcher dispatcher = self.world.Dispatcher;
            int count = dispatcher.NumManifolds;
            for (int i = 0; i < count; i++)
            {
                PersistentManifold contactManifold = dispatcher.GetManifoldByIndexInternal(i);
                if (contactManifold.NumContacts <= 0) continue;
                
                CollisionObject a = contactManifold.Body0;
                CollisionObject b = contactManifold.Body1;
                
                RigidBodyComponent rigidBodyComponentA = a.UserObject as RigidBodyComponent;
                RigidBodyComponent rigidBodyComponentB = b.UserObject as RigidBodyComponent;
                if (rigidBodyComponentA == null || rigidBodyComponentB == null) continue;
                
                (CollisionObject, CollisionObject) pair = a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a);
                
                self.nowCollision.Add(pair);
                
                // Enter
                if (!self.lastCollision.Contains(pair))
                {
                    var collisionEvent = new OnCollisionEnter();
                    
                    collisionEvent.a = rigidBodyComponentA.GameObject;
                    collisionEvent.b = rigidBodyComponentB.GameObject;
                    EventSystem.Instance.Publish(collisionEvent);
                    
                    collisionEvent.a = rigidBodyComponentB.GameObject;
                    collisionEvent.b = rigidBodyComponentA.GameObject;
                    EventSystem.Instance.Publish(collisionEvent);
                }
                // Stay
                else
                {
                    var collisionEvent = new OnCollisionStay();
                    
                    collisionEvent.a = rigidBodyComponentA.GameObject;
                    collisionEvent.b = rigidBodyComponentB.GameObject;
                    EventSystem.Instance.Publish(collisionEvent);
                    
                    collisionEvent.a = rigidBodyComponentB.GameObject;
                    collisionEvent.b = rigidBodyComponentA.GameObject;
                    EventSystem.Instance.Publish(collisionEvent);
                }
            }
            foreach (var pair in self.lastCollision)
            {
                // Exit
                if (!self.nowCollision.Contains(pair))
                {
                    RigidBodyComponent rigidBodyComponentA = pair.Item1.UserObject as RigidBodyComponent;
                    RigidBodyComponent rigidBodyComponentB = pair.Item2.UserObject as RigidBodyComponent;
                    if (rigidBodyComponentA == null || rigidBodyComponentB == null) continue;
                    
                    var collisionEvent = new OnCollisionExit();
                    
                    collisionEvent.a = rigidBodyComponentA.GameObject;
                    collisionEvent.b = rigidBodyComponentB.GameObject;
                    EventSystem.Instance.Publish(collisionEvent);
                    
                    collisionEvent.a = rigidBodyComponentB.GameObject;
                    collisionEvent.b = rigidBodyComponentA.GameObject;
                    EventSystem.Instance.Publish(collisionEvent);
                }
            }
            
            (self.lastCollision, self.nowCollision) = (self.nowCollision, self.lastCollision);
        }
    }

    private static void Init(this PhysicsSceneComponent self)
    {
        self.config = new DefaultCollisionConfiguration();
        self.dispatcher = new CollisionDispatcher(self.config);
        self.broadphase = new DbvtBroadphase();
        self.solver = new SequentialImpulseConstraintSolver();

        self.world = new DiscreteDynamicsWorld(self.dispatcher, self.broadphase, self.solver, self.config)
        {
            Gravity = new Vector3(0, self.g, 0),
        };
    }
}