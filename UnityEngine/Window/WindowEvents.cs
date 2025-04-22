using SharpVk.Glfw;

namespace UnityEngine.Events;

public struct Window_KeyPress_Event
{
    public Window window;
    public Key key;
}

public struct Window_KeyRelease_Event
{
    public Window window;
    public Key key;
}

public struct Window_KeyRepeat_Event
{
    public Window window;
    public Key key;
}

public struct Window_MousePress_Event
{
    public Window window;
    public MouseButton button;
}

public struct Window_MouseRelease_Event
{
    public Window window;
    public MouseButton button;
}

public struct Window_MouseRepeat_Event
{
    public Window window;
    public MouseButton button;
}

/// <summary>
/// 鼠标滚轮移动
/// </summary>
public struct Window_MouseScroll_Event
{
    public Window window;
    public float xOffset, yOffset;
}

public struct Window_MouseMove_Event
{
    public Window window;
    public float xOffset, yOffset;
}

public struct Window_Resize_Event
{
    public Window window;
    public int preWidth, preHeight, curWidth, curHeight;
}