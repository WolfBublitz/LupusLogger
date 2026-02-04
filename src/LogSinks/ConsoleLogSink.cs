using System;
using System.Collections.Generic;

namespace LupusLogger;

/// <summary>
/// A log sink that writes log messages to the console.
/// </summary>
public sealed class ConsoleLogSink : LogSinkBase
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets or sets the format string for timestamps.
    /// </summary>
    public string TimestampFormat { get; init; } = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    /// Gets or sets the color for timestamps.
    /// </summary>
    public ConsoleColor TimestampColor { get; init; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the color for senders.
    /// </summary>
    public ConsoleColor SendersColor { get; init; } = ConsoleColor.Cyan;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Protected Methods                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    protected override void Write(LogMessage logMessage)
    {
        Write(logMessage.Timestamp.ToString(TimestampFormat), TimestampColor);
        WriteSpace();
        WriteInBrackets(LogLevelToString(logMessage.LogLevel), LogLevelToColor(logMessage.LogLevel));
        WriteSpace();
        WriteInBrackets(string.Join(" ", logMessage.Senders), SendersColor);

        int position = WriteSpace();
        string message = logMessage.Message?.ToString() ?? string.Empty;
        string[] lines = [.. message.WrapLines(Console.WindowWidth - position)];

        for (int i = 0; i < lines.Length - 1; i++)
        {
            Write(lines[i], ConsoleColor.White);
            WriteLine(position);
        }

        Write(lines[^1], ConsoleColor.White);
        WriteLine();
    }

    /// <inheritdoc/>
    protected override void Write(DateTimeOffset timestamp, IReadOnlyList<string> senders, Exception exception)
    {
        DateTimeOffset timeOffset = exception.Data.Contains("Timestamp") && exception.Data["Timestamp"] is DateTimeOffset dto
            ? dto
            : DateTimeOffset.Now;

        Write(timeOffset.ToString(TimestampFormat), TimestampColor);
        WriteSpace();
        WriteInBrackets("EXCE", ConsoleColor.White, ConsoleColor.Red);
        WriteSpace();
        WriteInBrackets(string.Join(" ", senders), SendersColor);

        int position = WriteSpace();

        if (exception.StackTrace is not null)
        {
            int width = Console.WindowWidth - position;

            string[] lines = [.. exception.ToString().WrapLines(width)];

            for (int i = 0; i < lines.Length - 1; i++)
            {
                Write(lines[i], ConsoleColor.Red);
                WriteLine(position);
            }

            Write(lines[^1], ConsoleColor.Red);
        }

        WriteLine();
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private static int WriteSpace()
    {
        Console.Write(" ");

        return Console.CursorLeft;
    }

    private static void WriteLine()
        => Console.WriteLine();

    private static void WriteLine(int position)
    {
        Console.WriteLine();

        Console.CursorLeft = position;
    }

    private static string LogLevelToString(LogLevel? logLevel)
        => logLevel switch
        {
            LogLevel.Info => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERRO",
            null => "NONE",
            _ => "UNKN"
        };

    private static ConsoleColor LogLevelToColor(LogLevel? logLevel)
        => logLevel switch
        {
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            null => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };

    private static int Write(string message, ConsoleColor color)
    {
        ConsoleColor previousColor = Console.ForegroundColor;

        Console.ForegroundColor = color;
        Console.Write(message);

        Console.ForegroundColor = previousColor;

        return Console.CursorLeft;
    }

    private static int WriteInBrackets(string message, ConsoleColor color)
    {
        ConsoleColor previousColor = Console.ForegroundColor;

        Console.Write("[");

        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = previousColor;

        Console.Write("]");

        return Console.CursorLeft;
    }

    private static void WriteInBrackets(string message, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
    {
        ConsoleColor previousForegroundColor = Console.ForegroundColor;
        ConsoleColor previousBackgroundColor = Console.BackgroundColor;

        Console.Write("[");

        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        Console.Write(message);
        Console.ForegroundColor = previousForegroundColor;
        Console.BackgroundColor = previousBackgroundColor;

        Console.Write("]");
    }
}