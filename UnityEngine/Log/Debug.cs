using System.Diagnostics;

namespace UnityEngine
{
    // TODO 需要替换成NLog
    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[Debug]: {message}");
        }

        [Conditional("DEBUG")]
        public static void Log<T>(T message) where T : notnull
        {
            Log(message.ToString());
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Warning]: {message}");
        }

        [Conditional("DEBUG")]
        public static void Warning<T>(T message) where T : notnull
        {
            Warning(message.ToString());
        }

        [Conditional("DEBUG")]
        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            throw new Exception(message);
        }

        [Conditional("DEBUG")]
        public static void Error(Exception e)
        {
            Error(e.Message);
            throw e;
        }

        [Conditional("DEBUG")]
        public static void Error<T>(T message) where T : notnull
        {
            Error(message.ToString());
        }

        [Conditional("DEBUG")]
        public static void Assert(bool b, string message = "")
        {
            if (b)
            {
                Error(message);
            }
        }

        public static void Assert(Func<bool> func, string message = "")
        {
            try
            {
                bool b = func.Invoke();
                Assert(b, message);
            }
            catch (Exception e)
            {
                Error(e);
                throw;
            }
        }
    }
}