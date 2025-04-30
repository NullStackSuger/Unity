namespace UnityEngine;

public static class Time
{
    static Time()
    {
        startTime = Time.Now;
        lastTime = startTime;
        deltaTime = TimeSpan.Zero;
    }

    public static void Tick()
    {
        DateTime nowTime = DateTime.Now;
        deltaTime = (nowTime - lastTime);
        lastTime = nowTime;
    }
    
    public static DateTime Now => DateTime.Now;
    public static float DeltaTime => (float)deltaTime.TotalSeconds;

    private static TimeSpan deltaTime;
    private static DateTime lastTime;
    private static readonly DateTime startTime;
}