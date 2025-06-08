using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ET;

public static class Log
{
    private const int TraceLevel = 1;
    private const int DebugLevel = 2;
    private const int InfoLevel = 3;
    private const int WarningLevel = 4;
    
    [Conditional("DEBUG")]
    public static void Trace(string message)
    {
        if (Options.LogLevel > TraceLevel)
        {
            return;
        }
        
        message = Msg2LinkStackMsg(message);
        WriteLine(message, ConsoleColor.Gray);
    }

    [Conditional("DEBUG")]
    public static void Debug(string message)
    {
        if (Options.LogLevel > DebugLevel)
        {
            return;
        }
        
        WriteLine(message, ConsoleColor.Gray);
    }
    
    public static void Info(string message)
    {
        if (Options.LogLevel > InfoLevel)
        {
            return;
        }
        
        WriteLine(message, ConsoleColor.Gray);
    }

    public static void Warning(string message)
    {
        if (Options.LogLevel > WarningLevel)
        {
            return;
        }
        
        WriteLine(message, ConsoleColor.Yellow);
    }

    public static void Error(string message)
    {
        message = Msg2LinkStackMsg(message);
        WriteLine(message, ConsoleColor.Red);
    }

    public static void Error(Exception e)
    {
        Error(e.ToString());
    }
    
    // TODO 这里超链接不起作用
    private static string Msg2LinkStackMsg(string msg)
    {
        msg = Regex.Replace(msg,@"at (.*?) in (.*?\.cs):(\w+)", match =>
        {
            string path = match.Groups[2].Value;
            string line = match.Groups[3].Value;
            return $"{match.Groups[1].Value}\n<a href=\"{path}\" line=\"{line}\">{path}:{line}</a>";
        });
        return msg;
    }

    private static void WriteLine(string message, ConsoleColor color)
    {
        // TODO 这里ConsoleColor不起作用
        switch (color)
        {
            case ConsoleColor.Yellow:
                Console.WriteLine($"\u001b[33m{message}\u001b[0m");
                break;
            case ConsoleColor.Red:
                Console.WriteLine($"\u001b[31m{message}\u001b[0m");
                break;
            default:
                Console.WriteLine(message);
                break;
        }
        /*var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;*/
    }
}