using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using R3;

namespace Lupus.Logger;

/// <inheritdoc/>
public sealed class Logger : ILogger
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly Channel<LogMessage> channel;

    private readonly Task task;

    private readonly CancellationTokenSource cancellationTokenSource = new();

    private readonly object flushItem = new();

    private readonly Subject<LogMessage> logMessageSubject = new();

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

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

    /// <inheritdoc/>
    public Observable<LogMessage> Messages => logMessageSubject
        .Where(logMessage => logMessage.Message != flushItem)
        .AsObservable();

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
        logMessageSubject.Dispose();
    }

    /// <inheritdoc/>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        using IDisposable registration = cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));
        using IDisposable subscription = logMessageSubject
            .Where(logMessage => logMessage.Message == flushItem)
            .Subscribe(_ => taskCompletionSource.SetResult());

        Log(null, flushItem);

        await taskCompletionSource.Task.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Log(LogLevel? logLevel, object message)
    {
        LogMessage logMessage = new(DateTimeOffset.Now, [Name], logLevel, message);

        _ = channel.Writer.TryWrite(logMessage);
    }

    /// <inheritdoc/>
    public void Log(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Log(null, exception);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private async Task ProcessLogMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (LogMessage logMessage in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                logMessageSubject.OnNext(logMessage);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}