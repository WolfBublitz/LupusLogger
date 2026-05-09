using System;
using System.Collections.Generic;

namespace WB.Logging;

internal sealed record LogMessage<TPayload> : ILogMessagea<TPayload>
    where TPayload : notnull
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly List<string> senders = [];

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public required TPayload Payload { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset Timestamp { get; init; }
    
    /// <inheritdoc/>
    public IReadOnlyList<string> Senders => senders;

    /// <inheritdoc/>
    public LogLevel? LogLevel { get; init; }

    /// <inheritdoc/>
    object ILogMessage.Payload => Payload;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Adds the <paramref name="sender"/> to the list of senders of this log message.
    /// </summary>
    /// <param name="sender">The sender to add.</param>
    internal void AddSender(string sender)
        => senders.Add(sender);
}