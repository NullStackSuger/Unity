using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ET;

public class Window : Singleton<Window>, ISingletonAwake
{
    public void Awake()
    {
        window = VeldridStartup.CreateWindow(new WindowCreateInfo(0, 0, 800, 600, WindowState.Maximized, "Unity"));
    }

    public static implicit operator Sdl2Window(Window self)
    {
        return self.window;
    } 

    private Sdl2Window window;

    public bool Exist => window != null && window.Exists;
    public InputSnapshot InputSnapshot => window.PumpEvents();
}