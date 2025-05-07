using Veldrid;
using Veldrid.Sdl2;

namespace UnityEngine;

public static class LogicSystem
{
    static LogicSystem()
    {
        window = Window.window;
        scene = new Scene("Default Scene");
    }

    public static void Tick()
    {
        Time.Tick();
        Input.Tick(window.PumpEvents());
        scene.Tick();
    }
    
    private static readonly Sdl2Window window;
    private static readonly Scene scene;
}