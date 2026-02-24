using System;

namespace WB.Logging;

/// <summary>
/// Provides extension methods for <see cref="ILogger"/> to simplify common logging tasks.
/// </summary>
public static class ILoggerExtensions
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Attaches a <see cref="ConsoleLogSink"/> to <paramref name="this"/> <see cref="ILogger"/>.
    /// </summary>
    /// <param name="this">The logger to attach the console sink to.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to detach the console sink.</returns>
    public static IDisposable AttachConsole(this ILogger @this)
    {
        ArgumentNullException.ThrowIfNull(@this, nameof(@this));

        ConsoleLogSink consoleLogSink = new();

        return @this.AttachLogSink(consoleLogSink);
    }

    /// <summary>
    /// Attaches the <paramref name="consoleLogSink"/> to <paramref name="this"/> <see cref="ILogger"/>.
    /// </summary>
    /// <param name="this">The logger to attach the console sink to.</param>
    /// <param name="consoleLogSink">The console sink to attach.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to detach the console sink.</returns>
    public static IDisposable AttachConsole(this ILogger @this, ConsoleLogSink consoleLogSink)
    {
        ArgumentNullException.ThrowIfNull(@this, nameof(@this));
        ArgumentNullException.ThrowIfNull(consoleLogSink, nameof(consoleLogSink));

        return @this.AttachLogSink(consoleLogSink);
    }
}
