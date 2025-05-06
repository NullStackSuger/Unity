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
                Console.WriteLine($"[{a.level}]: {a.message}");
                await Task.CompletedTask;
            }
        }

        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
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
            Console.ForegroundColor = ConsoleColor.Yellow;
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