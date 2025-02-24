using System;

public static class DebugLogger
{
    public static bool IsEnabled { get; set; } = true;

    public static void Print(string message)
    {
        if (IsEnabled)
        {
            string indent = new string(' ', 4);
            Console.WriteLine($"{indent}{message}");
        }
    }
}
