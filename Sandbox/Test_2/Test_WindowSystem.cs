using SharpVk.Glfw;
using UnityEngine;
using UnityEngine.Events;

namespace Sandbox.Test_2;

public class Test_WindowSystem
{
    public void Run()
    {
        // 这里在Window里面写个内置的KeyCode更好, 就不用引用SharpVk.Glfw了, 但是我懒得写了
        EventSystem.Add(new Window_MousePress_EventHandler());
    }
}

public class Window_MousePress_EventHandler : AEvent<Window_MousePress_Event>
{
    protected override async Task Run(Window_MousePress_Event a)
    {
        Debug.Warning($"MouseEvent: {a.window.Title}, {a.button == MouseButton.Right}");
        await Task.CompletedTask;
    }
}