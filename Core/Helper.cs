namespace ET;

public static class Helper
{
    public static long GetLongHashCode(this Type type)
    {
        return type.TypeHandle.Value.ToInt64();
    }
}