using UnityEngine;

namespace Test_1;

public class Test_EventSystem
{
    public void Run()
    {
        // 具体使用可以看ET的事件系统, 仿照那个写的
        // https://github.com/egametang/ET
        EventSystem.Add(new TestEventHandler());
        EventSystem.PublishAsync(new TestEvent() { message = "111" });
    }
}

public class TestEventHandler : AEvent<TestEvent>
{
    protected override async Task Run(TestEvent a)
    {
        Debug.Warning($"TestEvent {a}");
        await Task.CompletedTask;
    }
}