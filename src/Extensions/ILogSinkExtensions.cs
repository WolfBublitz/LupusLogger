using System;
using System.Threading.Tasks;

namespace WB.Logging;

/// <summary>
/// Provides extension methods for the <see cref="ILogSink"/> interface.
/// </summary>
internal static class ILogSinkExtensions
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Internal Methods                                                            │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Submits the <paramref name="logMessage"/> to <paramref name="this"/> <see cref="ILogSink"/> safely, 
    /// catching any exceptions that occur and logging them to the console or a fallback logger instead of throwing.
    /// </summary>
    /// <typeparam name="TPayload">The type of the log message payload.</typeparam>
    /// <param name="this">The <see cref="ILogSink"/> to submit the log message to.</param>
    /// <param name="logMessage">The log message to submit.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions to prevent logging failures from crashing the application.")]
    internal static Task SubmitSafeAsync<TPayload>(this ILogSink @this, ILogMessage<TPayload> logMessage)
        where TPayload : notnull
    {
        return Task.Run(() =>
        {
            try
            {
                @this.Submit(logMessage);
            }
            catch (Exception ex)
            {
                // Log the exception to the console or a fallback logger
                _ = Console.Error.WriteLineAsync($"Error submitting log message: {ex}").ConfigureAwait(false);
            }
        });
    }
}