using System;

namespace WB.Logging;

/// <summary>
/// A disposable that executes a <paramref name="action"/> when disposed. This is useful 
/// for creating simple disposables without needing to define a new class for each one.
/// </summary>
/// <param name="action">The action to execute when the disposable is disposed.</param>
internal sealed class DelegateDisposable(Action action) : IDisposable
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public void Dispose()
        => action();
}