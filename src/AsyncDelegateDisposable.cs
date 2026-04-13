using System;
using System.Threading.Tasks;

namespace WB.Logging;

/// <summary>
/// A disposable that executes an asynchronous <paramref name="action"/> when disposed. This is useful 
/// for creating simple disposables without needing to define a new class for each one.
/// </summary>
/// <param name="action">The action to execute when the disposable is disposed.</param>
internal sealed class AsyncDelegateDisposable(Func<ValueTask> action) : IAsyncDisposable
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
        => action();
}