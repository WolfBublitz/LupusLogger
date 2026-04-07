using System;
using System.Threading.Tasks;

namespace WB.Logging;

/// <summary>
/// Provides extension methods for the <see cref="IAsyncLogSink"/> interface.
/// </summary>
internal static class IAsyncLogSinkExtensions
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Internal Methods                                                            │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Submits the <paramref name="logMessage"/> to <paramref name="this"/> <see cref="IAsyncLogSink"/> safely, 
    /// catching any exceptions that occur and logging them to the console or a fallback logger instead of throwing.
    /// </summary>
    /// <typeparam name="TPayload">The type of the log message payload.</typeparam>
    /// <param name="this">The <see cref="IAsyncLogSink"/> to submit the log message to.</param>
    /// <param name="logMessage">The log message to submit.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions to prevent logging failures from crashing the application.")]
    internal static async Task SubmitSafeAsync<TPayload>(this IAsyncLogSink @this, ILogMessage<TPayload> logMessage)
    {
        try
        {
            await @this.SubmitAsync(logMessage).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log the exception to the console or a fallback logger
            _ = Console.Error.WriteLineAsync($"Error submitting log message: {ex}").ConfigureAwait(false);
        }
    }
}