using Veldrid;
using Veldrid.Sdl2;

namespace UnityEngine;

public static class LogicSystem
{
    static LogicSystem()
    {
        window = Window.window;
        fileSystem = new FileSystem($"{Define.BasePath}\\Sandbox\\Asset");
        scene = new Scene("Default Scene");
    }

    public static void Tick()
    {
        Time.Tick();
        Input.Tick(window.PumpEvents());
        scene.Tick();
    }
    
    private static readonly Sdl2Window window;
    public static readonly FileSystem fileSystem;
    private static readonly Scene scene;
}