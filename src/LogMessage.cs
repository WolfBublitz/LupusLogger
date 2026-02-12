using System;
using System.Collections.Generic;

namespace WB.Logging;

/// <summary>
/// A log message.
/// </summary>
/// <param name="Timestamp">The timestamp of the log message.</param>
/// <param name="Senders">The senders of the log message.</param>
/// <param name="LogLevel">The <see cref="LogLevel"/> of the log message.</param>
/// <param name="Message">The message.</param>
public readonly record struct LogMessage(
    DateTimeOffset Timestamp,
    IReadOnlyList<string> Senders,
    LogLevel? LogLevel,
    object Message);