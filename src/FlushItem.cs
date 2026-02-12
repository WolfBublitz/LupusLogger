using System.Threading.Tasks;

namespace WB.Logging;

internal sealed class FlushItem
{
    private readonly TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Task => taskCompletionSource.Task;

    public void Complete() => taskCompletionSource.TrySetResult();

    public void Cancel() => taskCompletionSource.TrySetCanceled();
}
