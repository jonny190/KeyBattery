namespace KeyBattery;

/// <summary>
/// Plain text log at %AppData%\KeyBattery\log.txt for troubleshooting.
/// Starts fresh on each launch so it never grows without limit.
/// </summary>
static class Log
{
    static readonly object Lock = new();

    public static string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyBattery", "log.txt");

    public static void Start()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, "");
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Logging is best effort
        }

        Write($"KeyBattery {Application.ProductVersion} started");
    }

    public static void Write(string message)
    {
        lock (Lock)
        {
            try
            {
                File.AppendAllText(FilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                // Logging is best effort
            }
        }
    }
}
