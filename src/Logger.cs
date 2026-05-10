using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WB.Logging;

internal delegate Task Dispatcher(ILogMessage logMessage, CancellationToken cancellationToken);

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="Logger"/> class.
/// </summary>
public sealed class Logger : ILogger
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly Logger? parent;

    private readonly Channel<ILogMessage> logMessageQueue = Channel.CreateUnbounded<ILogMessage>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = true,
    });

    private readonly Task logMessageProcessingTask;

    private readonly CancellationTokenSource cancellationTokenSource = new();

    private readonly ConcurrentBag<ILogger> childLoggers = [];

    private readonly ConcurrentBag<ILogSink> logSinks = [];

    private readonly ConcurrentBag<IAsyncLogSink> asyncLogSinks = [];

    private readonly LogMessageFilterPipeline logMessageFilterPipeline = new();

    private readonly IDisposable minimumLogLevelFilter;

    private readonly ConcurrentDictionary<Type, Dispatcher> dispatchCache = new();

    private int isDisposed;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Constructors                                                         │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the logger.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
    public Logger(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        minimumLogLevelFilter = AddLogMessageFilter(logMessage => logMessage.LogLevel is null || logMessage.LogLevel >= MinimumLogLevel);
        logMessageProcessingTask = ExecuteAsync(cancellationTokenSource.Token);
    }

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
    public string Name { get; }

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

        await cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        await logMessageProcessingTask.ConfigureAwait(false);

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

        minimumLogLevelFilter.Dispose();
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
        where TPayload : notnull
    {
        LogMessage<TPayload> logMessage = new()
        {
            Timestamp = TimestampProvider.CurrentTimestamp,
            LogLevel = logLevel,
            Payload = payload,
        };

        Log(logMessage);

        parent?.Log(logMessage);
    }

    /// <inheritdoc/>
    public IDisposable AttachLogSink(ILogSink logSink)
    {
        logSinks.Add(logSink);

        return new DelegateDisposable(() => logSinks.TryTake(out _));
    }

    /// <inheritdoc/>
    public IDisposable AttachLogSink(IAsyncLogSink logSink)
    {
        asyncLogSinks.Add(logSink);

        return new DelegateDisposable(() => asyncLogSinks.TryTake(out _));
    }

    /// <inheritdoc/>
    public ILogger CreateChildLogger(string name)
    {
        Logger childLogger = new(name, this);

        childLoggers.Add(childLogger);

        return childLogger;
    }

    /// <inheritdoc/>
    public IDisposable AddLogMessageFilter(LogMessageFilter filter)
        => logMessageFilterPipeline.Add(filter);

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘

    private void Log<TPayload>(LogMessage<TPayload> logMessage)
        where TPayload : notnull
    {
        logMessage.AddSender(Name);

        logMessageQueue.Writer.TryWrite(logMessage);
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (ILogMessage logMessage in logMessageQueue.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                if (logMessage.Payload is FlushItem flushItem)
                {
                    flushItem.Complete();
                }
                else if (logMessageFilterPipeline.IsMatch(logMessage))
                {
                    await DispatchAsync(logMessage, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
    }

    private Task DispatchAsync(ILogMessage logMessage, CancellationToken cancellationToken)
    {
        Type payloadType = logMessage.Payload.GetType();

        Dispatcher dispatcher = dispatchCache.GetOrAdd(
            payloadType,
            static (t, self) => self.CreateDispatcher(t),
            this);

        return dispatcher(logMessage, cancellationToken);
    }

    private Dispatcher CreateDispatcher(Type payloadType)
    {
        ParameterExpression logMessageParameter = Expression.Parameter(typeof(ILogMessage), "msg");
        ParameterExpression cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "ct");

        UnaryExpression cast = Expression.Convert(logMessageParameter, typeof(ILogMessage<>).MakeGenericType(payloadType));

        MethodCallExpression methodCall = Expression.Call(
            Expression.Constant(this),
            typeof(Logger)
                .GetMethod(nameof(DispatchTyped), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(payloadType),
            cast,
            cancellationTokenParameter);

        Expression<Dispatcher> lambda = Expression.Lambda<Dispatcher>(methodCall, logMessageParameter, cancellationTokenParameter);
        return lambda.Compile();
    }

    private Task DispatchTyped<TPayload>(ILogMessage logMessage, CancellationToken cancellationToken)
        where TPayload : notnull
    {
        ILogMessage<TPayload> typedLogMessage = (ILogMessage<TPayload>)logMessage;

        Task[] tasks = [.. logSinks.Select(logSink => logSink.WriteSafeAsync(typedLogMessage)),
            .. asyncLogSinks.Select(asyncLogSink => asyncLogSink.SubmitSafeAsync(typedLogMessage, cancellationToken))];

        return Task.WhenAll(tasks);
    }
}
