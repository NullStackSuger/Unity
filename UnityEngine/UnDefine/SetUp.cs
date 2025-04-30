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

    public static void Run()
    {
        while (window.Exists)
        {
            LogicSystem.Tick();
            RenderSystem.Tick();
        }
    }
    
    private static readonly Sdl2Window window;
}