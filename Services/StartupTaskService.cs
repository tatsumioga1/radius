using Windows.ApplicationModel;

namespace Radius.Services;

public sealed class StartupTaskService
{
    private const string StartupTaskId = "RadiusStartup";

    public async Task<bool> IsEnabledAsync()
    {
        try
        {
            var task = await StartupTask.GetAsync(StartupTaskId);
            return task.State == StartupTaskState.Enabled;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetEnabledAsync(bool enabled)
    {
        try
        {
            var task = await StartupTask.GetAsync(StartupTaskId);
            if (enabled)
            {
                var state = await task.RequestEnableAsync();
                return state == StartupTaskState.Enabled;
            }

            task.Disable();
            return false;
        }
        catch
        {
            return false;
        }
    }
}
