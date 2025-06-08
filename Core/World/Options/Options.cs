namespace ET
{
    public static class Options
    {
        public static int Process { get; set; } = 1;

        public static int LogLevel { get; set; } = 0;

        /// <summary>
        /// 0开发
        /// 1正式
        /// 2测试
        /// </summary>
        public static uint Develop { get; set; } = 0;

        public static bool IsEditor => Develop != 1;

        static Options()
        {
            
        }
    }
}