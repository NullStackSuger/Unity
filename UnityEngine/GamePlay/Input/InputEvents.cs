using Veldrid;

namespace UnityEngine.Events;

public struct InputMouseEvent
{
    public MouseButton button;
    public InputState state;
}

public struct InputKeyEvent
{
    public Key button;
    public InputState state;
}