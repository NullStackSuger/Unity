namespace UnityEngine
{
    public static partial class Define
    {
#if DEBUG
        public static bool IsDebug = true;
#else
        public static bool IsDebug = false;
#endif
        public static string BasePath = "D:\\Rider\\Project\\Unity";
        public static string AssetPath = $"{BasePath}\\Sandbox\\Asset";
        public static string BinPath = $"{BasePath}\\Sandbox\\bin\\Debug\\net8.0-windows";
    }
}