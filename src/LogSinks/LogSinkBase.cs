using System;
using System.Collections.Generic;
using R3;

namespace WB.Logging;

/// <summary>
/// A base class for log sinks.
/// </summary>
public abstract class LogSinkBase : Observer<LogMessage>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Protected Methods                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    protected sealed override void OnNextCore(LogMessage value)
    {
        if (value.Message is Exception exception)
        {
            Write(value.Timestamp, value.Senders, exception);
        }
        else
        {
            Write(value);
        }
    }

    /// <inheritdoc/>
    protected sealed override void OnErrorResumeCore(Exception error)
    {
    }

    /// <inheritdoc/>
    protected sealed override void OnCompletedCore(Result result)
    {
    }

    /// <summary>
    /// Writes the <paramref name="logMessage"/>.
    /// </summary>
    /// <param name="logMessage">The <see cref="LogMessage"/> to write.</param>
    protected abstract void Write(LogMessage logMessage);

    /// <summary>
    /// Writes the <paramref name="exception"/>.
    /// </summary>
    /// <param name="timestamp">The timestamp of the log message.</param>
    /// <param name="senders">The senders of the log message.</param>
    /// <param name="exception">The <see cref="Exception"/> to write.</param>
    protected abstract void Write(DateTimeOffset timestamp, IReadOnlyList<string> senders, Exception exception);
}