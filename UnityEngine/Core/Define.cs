namespace UnityEngine
{
    public static class Define
    {
#if DEBUG
        public static bool IsDebug = true;
#else
    public static bool IsDebug = false;
#endif
    }
}