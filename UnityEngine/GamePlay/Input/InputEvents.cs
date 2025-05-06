using Veldrid;

namespace UnityEngine.Events;

public struct InputMouseEvent
{
    public MouseButton button;
    public Input.InputState state;
}

public struct InputKeyEvent
{
    public Key button;
    public Input.InputState state;
}