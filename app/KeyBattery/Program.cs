namespace KeyBattery;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Only one tray icon at a time
        using var mutex = new Mutex(true, @"Local\KeyBattery", out bool firstInstance);
        if (!firstInstance) return;

        ApplicationConfiguration.Initialize();
        Log.Start();
        Application.Run(new TrayApp());
    }
}
