namespace Roundly.Services;

public static class AppLog
{
    private static readonly object SyncRoot = new();
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Roundly");

    public static readonly string LogPath = Path.Combine(LogDirectory, "startup.log");

    public static void Write(string message)
    {
        try
        {
            lock (SyncRoot)
            {
                Directory.CreateDirectory(LogDirectory);
                File.AppendAllText(LogPath, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
                File.AppendAllText(
                    Path.Combine(Path.GetTempPath(), "Roundly-startup.log"),
                    $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never become the startup failure.
        }
    }

    public static void Write(Exception exception)
    {
        Write(exception.ToString());
    }
}
