using Veldrid.Sdl2;

namespace UnityEngine;

public static class LogicSystem
{
    static LogicSystem()
    {
        window = Window.window;
    }

    public static void Tick()
    {
        Input.Tick(window.PumpEvents());
        Time.Tick();
    }
    
    private static readonly Sdl2Window window;
}