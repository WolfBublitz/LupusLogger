using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WB.Logging;

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="Logger"/> class.
/// </summary>
/// <param name="name">The name of the logger.</param>
public sealed class Logger(string name) : ILogger
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly Logger? parent;

    private readonly ActionBlock<Func<Task>> logMessageQueue = new(
            async logMessageAction =>
            {
                await logMessageAction().ConfigureAwait(false);
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                EnsureOrdered = true
            });

    private readonly ConcurrentBag<ILogger> childLoggers = [];

    private readonly ConcurrentBag<ILogSink> logSinks = [];

    private readonly ConcurrentBag<IAsyncLogSink> asyncLogSinks = [];

    private int isDisposed;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Constructors                                                        │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private Logger(string name, Logger parent)
        : this(name)
    {
        this.parent = parent;
        MinimumLogLevel = parent.MinimumLogLevel;
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets a value indicating whether the logger has been disposed.
    /// </summary>
    public bool IsDisposed => Interlocked.CompareExchange(ref isDisposed, 0, 0) == 1;

    /// <inheritdoc/>
    public IReadOnlyList<ILogSink> LogSinks
        => [.. logSinks];

    /// <inheritdoc/>
    public IReadOnlyList<IAsyncLogSink> AsyncLogSinks
        => [.. asyncLogSinks];

    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    /// <remarks>
    /// Log messages with a <see cref="LogLevel"/> less severe than the <see cref="MinimumLogLevel"/>
    /// will be ignored and not submitted to log sinks or the parent logger.
    /// If <see cref="MinimumLogLevel"/> is <c>null</c>, the minimum log level of the parent logger will be used. If there is no parent logger, all log messages will be logged regardless of their log level.
    /// </remarks>
    public LogLevel? MinimumLogLevel
    {
        get => field ?? parent?.MinimumLogLevel;
        set;
    } = LogLevel.Info;

    /// <summary>
    /// Gets or sets the parent <see cref="ILogger"/>.
    /// </summary>
    /// <remarks>
    /// If set, log messages will also be forwarded to the parent logger.
    /// </remarks>
    public ILogger? Parent => parent;

    /// <summary>
    /// Gets or sets the <see cref="ITimestampProvider"/> to use for log messages.
    /// </summary>
    public ITimestampProvider TimestampProvider { get; init; } = new LocalTimeTimestampProvider();

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref isDisposed, 1) == 1)
        {
            return;
        }

        logMessageQueue.Complete();
        await logMessageQueue.Completion.ConfigureAwait(false);

        foreach (ILogger childLogger in childLoggers)
        {
            await childLogger.DisposeAsync().ConfigureAwait(false);
        }

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

        foreach (IAsyncLogSink asyncLogSink in asyncLogSinks)
        {
            if (asyncLogSink is IDisposable disposableLogSink)
            {
                disposableLogSink.Dispose();
            }
            else if (asyncLogSink is IAsyncDisposable asyncDisposableLogSink)
            {
                await asyncDisposableLogSink.DisposeAsync().ConfigureAwait(false);
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

        if (Parent is not null)
        {
            await Parent.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public void Log<TPayload>(LogLevel? logLevel, TPayload payload)
    {
        LogMessage<TPayload> logMessage = new()
        {
            Timestamp = TimestampProvider.CurrentTimestamp,
            LogLevel = logLevel,
            Payload = payload
        };

        Log(logMessage);
    }

    /// <inheritdoc/>
    public IDisposable AttachLogSink(ILogSink logSink)
    {
        logSinks.Add(logSink);

        return new DelegateDisposable(() => logSinks.TryTake(out _));
    }

    /// <inheritdoc/>
    public IAsyncDisposable AttachLogSink(IAsyncLogSink logSink)
    {
        asyncLogSinks.Add(logSink);

        return new AsyncDelegateDisposable(() =>
        {
            asyncLogSinks.TryTake(out _);

            return ValueTask.CompletedTask;
        });
    }

    /// <inheritdoc/>
    public ILogger CreateChildLogger(string name)
    {
        Logger childLogger = new(name, this);

        childLoggers.Add(childLogger);

        return childLogger;
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private void Log<TPayload>(LogMessage<TPayload> logMessage)
    {
        logMessage.AddSender(Name);

        if (logMessage.LogLevel is not null && logMessage.LogLevel < MinimumLogLevel)
        {
            return;
        }

        parent?.Log(logMessage);

        logMessageQueue.Post(() =>
        {
            if (logMessage.Payload is FlushItem flushItem)
            {
                flushItem.Complete();

                return Task.CompletedTask;
            }
            else
            {
                Task[] submitTasks = [
                    .. logSinks.Select(logSink => logSink.SubmitSafeAsync(logMessage)),
                    .. asyncLogSinks.Select(asyncLogSink => asyncLogSink.SubmitSafeAsync(logMessage))
                ];

                return Task.WhenAll(submitTasks);
            }
        });
    }
}