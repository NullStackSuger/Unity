namespace ET;

internal static class Helper
{
    internal static long GetLongHashCode(Type type)
    {
        return type.TypeHandle.Value.ToInt64();
    }
}