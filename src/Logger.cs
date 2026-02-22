using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WB.Logging;

/// <inheritdoc/>
public sealed class Logger : ILogger
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly Channel<LogMessage> channel;

    private readonly Task task;

    private readonly CancellationTokenSource cancellationTokenSource = new();

    private ImmutableList<ILogSink> logSinks = [];

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public IReadOnlyList<ILogSink> LogSinks
        => logSinks;

    /// <summary>
    /// Gets the name of the logger.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the minimum <see cref="LogLevel"/> to log.
    /// </summary>
    /// <remarks>
    /// Messages with a <see cref="LogLevel"/> less severe than this level will be ignored.
    /// The default is <see cref="LogLevel.Info"/>.
    /// </remarks>
    public LogLevel MinimumLogLevel { get; init; } = LogLevel.Info;

    /// <summary>
    /// Gets or sets the parent <see cref="ILogger"/>.
    /// </summary>
    /// <remarks>
    /// If set, log messages will also be forwarded to the parent logger.
    /// </remarks>
    public ILogger? Parent { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="ITimestampProvider"/> to use for log messages.
    /// </summary>
    public ITimestampProvider TimestampProvider { get; init; } = new LocalTimeTimestampProvider();

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Constructors                                                         │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="name">The name of the logger.</param>
    public Logger(string name)
    {
        Name = name;

        channel = Channel.CreateUnbounded<LogMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        task = ProcessLogMessagesAsync(cancellationTokenSource.Token);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await FlushAsync().ConfigureAwait(false);
        await cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        await task.ConfigureAwait(false);
        cancellationTokenSource.Dispose();

        foreach (ILogSink logSink in logSinks)
        {
            if (logSink is IAsyncDisposable asyncDisposableLogSink)
            {
                await asyncDisposableLogSink.DisposeAsync().ConfigureAwait(false);
            }
            else if (logSink is IDisposable disposableLogSink)
            {
                disposableLogSink.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        FlushItem flushItem = new();

        using IDisposable registration = cancellationToken.Register(flushItem.Cancel);

        Log(null, flushItem);

        await flushItem.Task.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Log(LogLevel? logLevel, object message)
    {
        LogMessage logMessage = new(TimestampProvider.CurrentTimestamp, [Name], logLevel, message);

        _ = channel.Writer.TryWrite(logMessage);
    }

    /// <inheritdoc/>
    public void Log(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Log(null, exception);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info(object message)
        => Log(LogLevel.Info, message);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Warning(object message)
        => Log(LogLevel.Warning, message);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Error(object message)
        => Log(LogLevel.Error, message);

    /// <inheritdoc/>
    public IDisposable AttachLogSink(ILogSink logSink)
    {
        logSinks = logSinks.Add(logSink);

        return new DelegateDisposable(() => logSinks = logSinks.Remove(logSink));
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions that may occur when submitting log messages to log sinks, to prevent the logger from crashing.")]
    private async Task ProcessLogMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (LogMessage logMessage in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                if (logMessage.Message is FlushItem flushItem)
                {
                    flushItem.Complete();
                }
                else
                {
                    for (int i = 0; i < logSinks.Count; i++)
                    {
                        // The log message is only submitted to log sinks if its log level is greater than or equal to the minimum log level.
                        if (logMessage.LogLevel is not null && logMessage.LogLevel < MinimumLogLevel)
                        {
                            continue;
                        }

                        try
                        {
                            logSinks[i].Submit(logMessage);
                        }
                        catch (Exception exception)
                        {
                            await Console.Error.WriteLineAsync($"Error submitting log message to log sink: {exception}").ConfigureAwait(false);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}