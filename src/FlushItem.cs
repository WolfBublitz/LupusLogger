using System.Threading.Tasks;

namespace WB.Logging;

/// <summary>
/// Represents an item in the flush queue, which is used to signal when all 
/// log messages have been processed and flushed to their respective sinks.
/// </summary>
internal sealed class FlushItem
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets a <see cref="Task"/> that completes when the flush operation is complete.
    /// This allows callers to await the completion of the flush operation.
    /// </summary>
    public Task Task => taskCompletionSource.Task;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Marks the flush operation as complete, allowing any awaiting tasks to continue.
    /// </summary>
    public void Complete() => taskCompletionSource.TrySetResult();


    /// <summary>
    /// Marks the flush operation as failed, allowing any awaiting tasks to observe the exception.
    /// </summary>
    public void Cancel() => taskCompletionSource.TrySetCanceled();
}
