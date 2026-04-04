using System;
using System.Collections.Generic;

namespace WB.Logging;

/// <inheritdoc cref="ILogMessage{TPayload}"/>
internal readonly record struct LogMessage<TPayload> : ILogMessage<TPayload>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly List<string> senders = new(3);

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Constructors                                                         │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="LogMessage{TPayload}"/> struct.
    /// </summary>
    public LogMessage()
    {
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public required DateTimeOffset Timestamp { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Senders => senders;

    /// <inheritdoc/>
    public LogLevel? LogLevel { get; init; }

    /// <inheritdoc/>
    public TPayload? Payload { get; init; }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Internal Methods.                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Adds the <paramref name="sender"/> to the list of senders of the log message.
    /// </summary>
    /// <param name="sender">The sender to add.</param>
    internal void AddSender(string sender) => senders.Add(sender);

}
