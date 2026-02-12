using System;

namespace WB.Logging;

/// <summary>
/// Provides timestamps for log messages.
/// </summary>
public interface ITimestampProvider
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties.                                                          │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets the current timestamp.
    /// </summary>
    /// <returns>The current timestamp.</returns>
    public DateTimeOffset CurrentTimestamp { get; }
}