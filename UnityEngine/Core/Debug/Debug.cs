using System.Diagnostics;
using UnityEngine.Events;

namespace UnityEngine
{
    public static class Debug
    {
        static Debug()
        {
            EventSystem.Add(new DebugEventHandler());
        }
        
        private class DebugEventHandler : AEvent<DebugEvent>
        {
            protected override async Task Run(DebugEvent a)
            {
                // Info
                if ((a.level & DebugLevel.Info) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"[{a.level}]: {a.message}");
                }
                // Warning
                else if ((a.level & DebugLevel.Warning) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{a.level}]: {a.message}");
                }
                // Error
                else if ((a.level & DebugLevel.Error) != 0)
                {
                    throw new Exception($"[{a.level}]: {a.message}");
                }
                await Task.CompletedTask;
            }
        }
        
        /// <summary>
        /// 获取输出信息
        /// </summary>
        public static IEnumerable<DebugEvent> Get()
        {
            foreach (DebugEvent info in debugInfos)
            {
                if ((info.level & Level) != 0)
                {
                    yield return info;
                }
            }
        }
        
        /// <summary>
        /// 清除输出信息
        /// </summary>
        public static void Clear()
        {
            InfoCount = 0;
            WarningCount = 0;
            ErrorCount = 0;
            debugInfos.Clear();
        }

        public static DebugLevel Level = DebugLevel.All;
        public static uint InfoCount { get; private set; }
        public static uint WarningCount { get; private set; }
        public static uint ErrorCount { get; private set; }
        private static readonly List<DebugEvent> debugInfos = new();

        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            DebugEvent logEvent = new()
            {
                level = DebugLevel.Info, 
                time = Time.Now.ToString("mm:ss"), 
                color = Color.White,
                message = message.TrimEnd(".cs".ToCharArray())
            };
            
            var stackTrace = new StackTrace(true);
            var caller = stackTrace.GetFrame(1);
            if (caller != null)
            {
                logEvent.path = Path.GetRelativePath(Define.BasePath, caller.GetFileName()!);
                logEvent.line = caller.GetFileLineNumber();
                logEvent.method = caller.GetMethod()!.Name;
            }
            
            ++InfoCount;
            debugInfos.Add(logEvent);
            EventSystem.PublishAsync(logEvent);
        }
        [Conditional("DEBUG")]
        public static void Log<T>(T message) where T : notnull
        {
            Log(message.ToString());
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            DebugEvent warningEvent = new()
            {
                level = DebugLevel.Warning, 
                time = Time.Now.ToString("mm:ss"),
                color = Color.Yellow,
                message = message.TrimEnd(".cs".ToCharArray())
            };
            
            var stackTrace = new StackTrace(true);
            var caller = stackTrace.GetFrame(1);
            if (caller != null)
            {
                warningEvent.path = Path.GetRelativePath(Define.BasePath, caller.GetFileName()!);
                warningEvent.line = caller.GetFileLineNumber();
                warningEvent.method = caller.GetMethod()!.Name;
            }
            
            ++WarningCount;
            debugInfos.Add(warningEvent);
            EventSystem.PublishAsync(warningEvent);
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
            DebugEvent errorEvent = new()
            {
                level = DebugLevel.Error, 
                time = Time.Now.ToString("mm:ss"),
                color = Color.Red,
                message = message.TrimEnd(".cs".ToCharArray())
            };
            
            var stackTrace = new StackTrace(true);
            var caller = stackTrace.GetFrame(1);
            if (caller != null)
            {
                errorEvent.path = Path.GetRelativePath(Define.BasePath, caller.GetFileName()!);
                errorEvent.line = caller.GetFileLineNumber();
                errorEvent.method = caller.GetMethod()!.Name;
            }
            
            ++ErrorCount;
            debugInfos.Add(errorEvent);
            EventSystem.PublishAsync(errorEvent);
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

        [Conditional("DEBUG")]
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