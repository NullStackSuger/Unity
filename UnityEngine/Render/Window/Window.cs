using UnityEngine.Events;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace UnityEngine;

public static class Window
{
    static Window()
    {
        window = VeldridStartup.CreateWindow(new WindowCreateInfo(50, 50, 1920, 1080, WindowState.Maximized, "Unity"));
        
        window.Resized += () =>
        {
            EventSystem.PublishAsync(new WindowResizeEvent() { window = window, width = window.Width, height = window.Height });
        };
    }
    
    public static readonly Sdl2Window window;
}