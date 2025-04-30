using System.Numerics;
using UnityEngine.Events;
using Veldrid;
using Veldrid.Sdl2;

namespace UnityEngine;

public static class Input
{
    static Input()
    {
        window = Window.window;
    }
    
    public static void Tick(InputSnapshot snapshot)
    {
        Snapshot = snapshot;

        foreach (MouseEvent mouseEvent in snapshot.MouseEvents)
        {
            if (mouseEvent.Down) Down(mouseEvent.MouseButton);
            else Up(mouseEvent.MouseButton);
        }

        foreach (KeyEvent keyEvent in snapshot.KeyEvents)
        {
            if (keyEvent.Down) Down(keyEvent.Key);
            else Up(keyEvent.Key);
        }
    }
    
    public static Vector2 MousePosition => Snapshot.MousePosition;
    public static Vector2 MouseOffset => window.MouseDelta;

    public static InputState GetMouseButton(MouseButton button)
    {
        return mouseButtons[button];
    }
    public static bool GetMouseButton(MouseButton button, InputState state)
    {
        return mouseButtons[button] == state;   
    }
    public static InputState GetKeyButton(Key key)
    {
        return keyButtons[key];
    }
    public static bool GetKeyButton(Key key, InputState state)
    {
        return keyButtons[key] == state;   
    }
    /// <summary>
    /// 鼠标按键被按下
    /// </summary>
    private static void Down(MouseButton button)
    {
        if (mouseButtons.TryAdd(button, InputState.Down)) return;

        switch (mouseButtons[button])
        {
            case InputState.Up:
                mouseButtons[button] = InputState.Down;
                EventSystem.PublishAsync(new InputMouseEvent() { button = button, state = InputState.Down });
                break;
            case InputState.Down:
                mouseButtons[button] = InputState.Press;
                EventSystem.PublishAsync(new InputMouseEvent() { button = button, state = InputState.Press });
                break;
            case InputState.Press:
                EventSystem.PublishAsync(new InputMouseEvent() { button = button, state = InputState.Press });
                break;
        }
    }
    /// <summary>
    /// 鼠标按键被抬起
    /// </summary>
    private static void Up(MouseButton button)
    {
        if (mouseButtons.TryAdd(button, InputState.Up)) return;
        
        switch (mouseButtons[button])
        {
            case InputState.Up:
                break;
            case InputState.Down:
                mouseButtons[button] = InputState.Up;
                EventSystem.PublishAsync(new InputMouseEvent() { button = button, state = InputState.Up });
                break;
            case InputState.Press:
                mouseButtons[button] = InputState.Up;
                EventSystem.PublishAsync(new InputMouseEvent() { button = button, state = InputState.Up });
                break;
        }
    }
    /// <summary>
    /// 键盘按键被按下
    /// </summary>
    private static void Down(Key button)
    {
        if (keyButtons.TryAdd(button, InputState.Down)) return;
        
        switch (keyButtons[button])
        {
            case InputState.Up:
                keyButtons[button] = InputState.Down;
                EventSystem.PublishAsync(new InputKeyEvent() { button = button, state = InputState.Down });
                break;
            case InputState.Down:
                keyButtons[button] = InputState.Press;
                EventSystem.PublishAsync(new InputKeyEvent() { button = button, state = InputState.Press });
                break;
            case InputState.Press:
                EventSystem.PublishAsync(new InputKeyEvent() { button = button, state = InputState.Press });
                break;
        }
    }
    /// <summary>
    /// 键盘按键被抬起
    /// </summary>
    private static void Up(Key button)
    {
        if (keyButtons.TryAdd(button, InputState.Up)) return;
        
        switch (keyButtons[button])
        {
            case InputState.Up:
                break;
            case InputState.Down:
                keyButtons[button] = InputState.Up;
                EventSystem.PublishAsync(new InputKeyEvent() { button = button, state = InputState.Up });
                break;
            case InputState.Press:
                keyButtons[button] = InputState.Up;
                EventSystem.PublishAsync(new InputKeyEvent() { button = button, state = InputState.Up });
                break;
        }
    }
    private static readonly Dictionary<MouseButton, InputState> mouseButtons = new();
    private static readonly Dictionary<Key, InputState> keyButtons = new();
    
    public static InputSnapshot Snapshot { get; private set; }
    private static readonly Sdl2Window window;
}

public enum InputState
{
    Down, Up, Press
}