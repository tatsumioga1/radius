using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Roundly.Services;

namespace Roundly;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            AppLog.Write("Unhandled exception");
            if (eventArgs.ExceptionObject is Exception exception)
            {
                AppLog.Write(exception);
            }
            else
            {
                AppLog.Write(eventArgs.ExceptionObject?.ToString() ?? "Unknown exception object");
            }
        };

        AppLog.Write("Process starting");

        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            AppLog.Write("COM wrappers initialized");
            Application.Start(initializationParams =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                AppLog.Write("Creating App");
                _ = new App();
            });
            AppLog.Write("Application.Start returned");
        }
        catch (Exception exception)
        {
            AppLog.Write("Startup exception");
            AppLog.Write(exception);
            throw;
        }
    }
}
