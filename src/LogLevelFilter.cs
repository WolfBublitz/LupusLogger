using System;

namespace WB.Logging;

internal sealed class MinimumLogLevelFilter(LogLevel minimumLogLevel) : ILogMessageFilter
{
    public LogLevel? MinimumLogLevel { get; set; } = minimumLogLevel;

    public bool IsMatch<TPayload>(ILogMessage<TPayload> logMessage)
    where TPayload : notnull
        => logMessage.LogLevel is null || logMessage.LogLevel >= MinimumLogLevel;
}
