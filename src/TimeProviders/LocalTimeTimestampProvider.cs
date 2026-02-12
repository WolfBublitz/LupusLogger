using System;

namespace WB.Logging;

/// <summary>
/// Provides timestamps for log messages based on the local time of the system.
/// </summary>
public sealed class LocalTimeTimestampProvider : ITimestampProvider
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties.                                                          │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public DateTimeOffset CurrentTimestamp => DateTimeOffset.Now;
}
