using Veldrid.Sdl2;

namespace UnityEngine;

public static class SetUp
{
    static SetUp()
    {
        window = Window.window;
        _ = nameof(LogicSystem);
        _ = nameof(RenderSystem);
    }

    public static async void Run()
    {
        while (window.Exists)
        {
            LogicSystem.Tick();
            RenderSystem.Tick();
            
            Task.Delay(100).Wait();
        }
    }
    
    private static readonly Sdl2Window window;
}