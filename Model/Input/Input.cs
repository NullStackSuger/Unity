using Veldrid;

namespace ET;

public class Input : Singleton<Input>, ISingletonAwake
{
    public void Awake()
    {
        foreach (Key key in Enum.GetValues(typeof(Key)))
        {
            keyStates[key] = InputState.LongTimeUp;
        }

        foreach (MouseButton mouse in Enum.GetValues(typeof(MouseButton)))
        {
            mouseStates[mouse] = InputState.LongTimeUp;
        }
    }
    
    public void Update(InputSnapshot snapshot)
    {
        Snapshot = snapshot;

        foreach (Key key in waitToLongTimeUpKeys)
        {
            keyStates[key] = InputState.LongTimeUp;
        }
        waitToLongTimeUpKeys.Clear();
        
        foreach (MouseButton mouse in waitToLongTimeUpMouses)
        {
            mouseStates[mouse] = InputState.LongTimeUp;
        }
        waitToLongTimeUpMouses.Clear();

        foreach (Key key in waitToLongTimeDownKeys)
        {
            keyStates[key] = InputState.LongTimeDown;
        }
        waitToLongTimeDownKeys.Clear();

        foreach (MouseButton mouse in waitToLongTimeDownMouses)
        {
            mouseStates[mouse] = InputState.LongTimeDown;
        }
        waitToLongTimeDownMouses.Clear();
        
        foreach (KeyEvent keyEvent in snapshot.KeyEvents)
        {
            Key key = keyEvent.Key;
            bool needDown = keyEvent.Down;
            
            if (needDown)
            {
                // TODO SDL_ERROR, 我16帧按住A, 16帧时KeyEvents里有A, 之后直到21帧, KeyEvents才再次包含A
                // 下面的Mouse也是同理
                //keyStates[key] = (keyStates[key] == InputState.Down || keyStates[key] == InputState.LongTimeDown) ? InputState.LongTimeDown : InputState.Down;
                if (keyStates[key] == InputState.Up || keyStates[key] == InputState.LongTimeUp)
                {
                    keyStates[key] = InputState.Down;
                    waitToLongTimeDownKeys.Add(key);
                }
            }
            else
            {
                keyStates[key] = InputState.Up;
                waitToLongTimeUpKeys.Add(key);
            }
        }

        foreach (MouseEvent mouseEvent in snapshot.MouseEvents)
        {
            MouseButton mouse = mouseEvent.MouseButton;
            bool needDown = mouseEvent.Down;
            
            if (needDown)
            {
                if (mouseStates[mouse] == InputState.Up || mouseStates[mouse] == InputState.LongTimeUp)
                {
                    mouseStates[mouse] = InputState.Down;
                    waitToLongTimeDownMouses.Add(mouse);
                }
            }
            else
            {
                mouseStates[mouse] = InputState.Up;
                waitToLongTimeUpMouses.Add(mouse);
            }
        }
    }

    public InputState Get(Key key)
    {
        return keyStates[key];
    }
    public InputState Get(MouseButton mouse)
    {
        return mouseStates[mouse];
    }
    
    private readonly Dictionary<Key, InputState> keyStates = new();
    private readonly HashSet<Key> waitToLongTimeUpKeys = new();
    private readonly HashSet<Key> waitToLongTimeDownKeys = new();
    private readonly Dictionary<MouseButton, InputState> mouseStates = new();
    private readonly HashSet<MouseButton> waitToLongTimeUpMouses = new();
    private readonly HashSet<MouseButton> waitToLongTimeDownMouses = new();
    
    public InputSnapshot Snapshot { get; private set; }
    
    public enum InputState
    {
        Up, Down, LongTimeDown, LongTimeUp
    }
}