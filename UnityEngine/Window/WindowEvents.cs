using SharpVk.Glfw;

namespace UnityEngine.Events;

public struct Window_KeyPress_Event
{
    public WindowHandle window;
    public Key key;
}

public struct Window_KeyRelease_Event
{
    public WindowHandle window;
    public Key key;
}

public struct Window_KeyRepeat_Event
{
    public WindowHandle window;
    public Key key;
}

public struct Window_MousePress_Event
{
    public WindowHandle window;
    public MouseButton button;
}

public struct Window_MouseRelease_Event
{
    public WindowHandle window;
    public MouseButton button;
}

public struct Window_MouseRepeat_Event
{
    public WindowHandle window;
    public MouseButton button;
}

public struct Window_Resize_Event
{
    public WindowHandle window;
    public int preWidth, preHeight, curWidth, curHeight;
}