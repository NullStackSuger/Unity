using Veldrid;
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
        Time.Tick();
        Input.Tick(window.PumpEvents());
        
        if (Input.Get(MouseButton.Right) == Input.InputState.Down)
        {
            Debug.Warning("Down");
        }

        if (Input.Get(MouseButton.Right) == Input.InputState.Up)
        {
            Debug.Warning("Up");
        }
        
        if (Input.Get(MouseButton.Right) == Input.InputState.LongTimeDown)
        {
            Debug.Log("LongTimeDown");
        }
    }
    
    private static readonly Sdl2Window window;
}