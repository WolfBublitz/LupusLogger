using System;
using System.Threading;
using System.Threading.Tasks;
using R3;

namespace Lupus.Logger;

/// <summary>
/// A logger.
/// </summary>
public interface ILogger : IAsyncDisposable
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets an observable sequence of <see cref="LogMessage"/>s.
    /// </summary>
    public Observable<LogMessage> Messages { get; }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods.                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Logs a <paramref name="message"/> at the specified <paramref name="logLevel"/>.
    /// </summary>
    /// <param name="logLevel">The <see cref="LogLevel"/>.</param>
    /// <param name="message">The message to log.</param>
    public void Log(LogLevel? logLevel, object message);

    /// <summary>
    /// Logs the <paramref name="exception"/>
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> to log.</param>
    public void Log(Exception exception);

    /// <summary>
    /// Logs the <paramref name="message"/> at <see cref="LogLevel.Info"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(object message)
        => Log(LogLevel.Info, message);

    /// <summary>
    /// Logs the <paramref name="message"/> at <see cref="LogLevel.Warning"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Warning(object message)
        => Log(LogLevel.Warning, message);

    /// <summary>
    /// Logs the <paramref name="message"/> at <see cref="LogLevel.Error"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Common logging term.")]
    public void Error(object message)
        => Log(LogLevel.Error, message);

    /// <summary>
    /// Flushes all pending log messages.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the flush to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous flush operation.</returns>
    public Task FlushAsync(CancellationToken cancellationToken = default);
}
