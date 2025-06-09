using ET.Event;

namespace ET;

[Event]
public class OnCollisionTestBeginHandler : AEvent<OnCollisionTestBegin>
{
    protected override async ETTask Run(OnCollisionTestBegin message)
    {
        foreach (Component component in message.obj.Components)
        {
            CollisionSystem.Instance.CollisionTestBegin(component);
        }
        
        await ETTask.CompletedTask;
    }
}

[Event]
public class OnCollisionTestEndHandler : AEvent<OnCollisionTestEnd>
{
    protected override async ETTask Run(OnCollisionTestEnd message)
    {
        foreach (Component component in message.obj.Components)
        {
            CollisionSystem.Instance.CollisionTestEnd(component);
        }
        
        await ETTask.CompletedTask;
    }
}

[Event]
public class OnCollisionEnterHandler : AEvent<OnCollisionEnter>
{
    protected override async ETTask Run(OnCollisionEnter message)
    {
        // 这里为什么不递归调用child去传递这个事件
        // 1.一个obj上 实现这个事件的不会很多, a是有Rigidbody的GameObject, 回调的实现最可能在它身上的组件中
        // 2.比如一个obj上有2个Collision(不同层级), 一左一右, 那递归调用必然会导致obj身上的实现被调用2次, 而不递归就可以让左边碰撞体的回调处理左边碰撞体, 右边的回调处理右边
        foreach (Component componentA in message.a.Components)
        {
            // 我自己更喜欢单写一个CollisionSystem, 主要是没用过dynamic, 怕有问题
            //EntitySystem.Instance.CollisionEnter(componentA);
            CollisionSystem.Instance.CollisionEnter(componentA);

            foreach (Component componentB in message.b.Components)
            {
                //EntitySystem.Instance.CollisionEnter(componentA, componentB);
                CollisionSystem.Instance.CollisionEnter(componentA, componentB);
            }
        }
        
        await ETTask.CompletedTask;
    }
}

[Event]
public class OnCollisionStayHandler : AEvent<OnCollisionStay>
{
    protected override async ETTask Run(OnCollisionStay message)
    {
        foreach (Component componentA in message.a.Components)
        {
            //EntitySystem.Instance.CollisionStay(componentA);
            CollisionSystem.Instance.CollisionStay(componentA);

            foreach (Component componentB in message.b.Components)
            {
                //EntitySystem.Instance.CollisionStay(componentA, componentB);
                CollisionSystem.Instance.CollisionStay(componentA, componentB);
            }
        }
        
        await ETTask.CompletedTask;
    }
}

[Event]
public class OnCollisionExitHandler : AEvent<OnCollisionExit>
{
    protected override async ETTask Run(OnCollisionExit message)
    {
        foreach (Component componentA in message.a.Components)
        {
            //EntitySystem.Instance.CollisionExit(componentA);
            CollisionSystem.Instance.CollisionExit(componentA);

            foreach (Component componentB in message.b.Components)
            {
                //EntitySystem.Instance.CollisionExit(componentA, componentB);
                CollisionSystem.Instance.CollisionExit(componentA, componentB);
            }
        }
        
        await ETTask.CompletedTask;
    }
}