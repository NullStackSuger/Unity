using Veldrid.Sdl2;

namespace UnityEngine.Events;

public struct WindowResizeEvent
{
    public Sdl2Window window;
    public int width;
    public int height;
}