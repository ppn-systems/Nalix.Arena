namespace Nalix.Host.Extensions;

internal static class TaskExtensions
{
    public static void Forget(this System.Threading.Tasks.Task t)
    {
        _ = t.ContinueWith(static antecedent =>
        {
            if (antecedent.IsFaulted && antecedent.Exception is { } ex)
            {
                if (ex.InnerException is System.OperationCanceledException or System.Threading.Tasks.TaskCanceledException)
                {
                    Logging.NLogix.Host.Instance.Debug("Background task canceled.");
                }
                else
                {
                    Logging.NLogix.Host.Instance.Error($"Background task error: {ex.Flatten()}");
                }
            }
        }, System.Threading.Tasks.TaskScheduler.Default);
    }
}
