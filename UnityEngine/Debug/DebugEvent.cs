namespace UnityEngine.Events;

public struct DebugEvent
{
    public DebugLevel level;
    public Color color;
    public string time;
    public string message;
    public string path;
    public string method;
    public int line;
}

[Flags]
public enum DebugLevel
{
    None = 0,
    Info = 1 << 0,
    Warning = 1 << 1,
    Error = 1 << 2,
    All = Info | Warning | Error
}