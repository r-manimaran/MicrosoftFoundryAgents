using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib;

public static class Utils
{
    public static void Red(Exception e)
    {
        Red(e.ToString());
    }

    public static void Red(string text)
    {
        WriteLine(text, ConsoleColor.Red);
    }

    public static void Yellow(object text)
    {
        WriteLine(text.ToString()!, ConsoleColor.Yellow);
    }

    public static void Yellow(string text)
    {
        WriteLine(text, ConsoleColor.Yellow);
    }

    public static void Gray(string text)
    {
        WriteLine(text, ConsoleColor.DarkGray);
    }

    public static void Green(string text)
    {
        WriteLine(text, ConsoleColor.Green);
    }

    public static void WriteLine(string text, ConsoleColor color)
    {
        ConsoleColor orgColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        finally
        {
            Console.ForegroundColor = orgColor;
        }
    }

    public static void WriteLineInformation(string text)
    {
        ConsoleColor orgColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = System.ConsoleColor.Gray;
            Console.WriteLine(text);
        }
        finally
        {
            Console.ForegroundColor = orgColor;
        }
    }

    public static void Separator()
    {
        Console.WriteLine();
        WriteLine("".PadLeft(Console.WindowWidth, '-'), ConsoleColor.Gray);
        Console.WriteLine();
    }
}