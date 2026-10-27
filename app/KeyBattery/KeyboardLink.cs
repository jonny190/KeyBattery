using HidSharp;

namespace KeyBattery;

/// <summary>
/// Finds Keychron raw HID interfaces (usage page 0xFF60, usage 0x61), on the
/// 2.4GHz receiver or a cabled keyboard, and listens on each for battery reports.
/// Events are raised on background threads.
/// </summary>
public sealed class KeyboardLink : IDisposable
{
    const int KeychronVendorId = 0x3434;
    const uint RawHidUsage = 0xFF600061;

    readonly object _lock = new();
    readonly object _scanLock = new();
    readonly Dictionary<string, (HidDevice Device, HidStream Stream)> _open = new();
    bool _disposed;

    public event Action<BatteryReport>? ReportReceived;
    public event Action? DevicesChanged;

    public int OpenCount
    {
        get { lock (_lock) return _open.Count; }
    }

    public void Start()
    {
        DeviceList.Local.Changed += OnDeviceListChanged;
        Rescan();
    }

    /// <summary>
    /// Asks every open interface for a fresh report. Each query wakes the
    /// keyboard's sleep timer, so this is for start up and manual refreshes only.
    /// </summary>
    public void RequestUpdate()
    {
        List<(HidDevice Device, HidStream Stream)> targets;
        lock (_lock) targets = _open.Values.ToList();

        foreach (var (device, stream) in targets) SendQuery(device, stream);
    }

    void OnDeviceListChanged(object? sender, DeviceListChangedEventArgs e) => Rescan();

    void Rescan()
    {
        lock (_scanLock)
        {
            foreach (var device in DeviceList.Local.GetHidDevices(KeychronVendorId))
            {
                lock (_lock)
                {
                    if (_disposed || _open.ContainsKey(device.DevicePath)) continue;
                }

                if (!IsRawHid(device) || !device.TryOpen(out HidStream stream)) continue;

                stream.ReadTimeout = Timeout.Infinite;
                stream.WriteTimeout = 1000;
                lock (_lock) _open[device.DevicePath] = (device, stream);
                Log.Write($"Opened raw HID interface {device.ProductID:X4} {device.DevicePath}");

                new Thread(() => ReadLoop(device, stream)) { IsBackground = true, Name = "KeyBattery HID reader" }.Start();
                SendQuery(device, stream);
            }
        }

        DevicesChanged?.Invoke();
    }

    static bool IsRawHid(HidDevice device)
    {
        try
        {
            return device.GetReportDescriptor().DeviceItems
                .Any(item => item.Usages.GetAllValues().Contains(RawHidUsage));
        }
        catch (Exception)
        {
            // Some interfaces refuse descriptor access, they are never ours
            return false;
        }
    }

    static void SendQuery(HidDevice device, HidStream stream)
    {
        try
        {
            stream.Write(BatteryReport.BuildQuery(device.GetMaxOutputReportLength()));
            Log.Write($"Sent battery query to {device.ProductID:X4}");
        }
        catch (Exception e) when (e is IOException or TimeoutException or ObjectDisposedException)
        {
            // Receiver present but keyboard asleep, its next push will arrive anyway
            Log.Write($"Battery query to {device.ProductID:X4} failed: {e.Message}");
        }
    }

    void ReadLoop(HidDevice device, HidStream stream)
    {
        var buffer = new byte[device.GetMaxInputReportLength()];
        try
        {
            while (true)
            {
                int count = stream.Read(buffer, 0, buffer.Length);
                var report = BatteryReport.TryParse(buffer.AsSpan(0, count));
                if (report != null)
                {
                    Log.Write($"Report from {device.ProductID:X4}: {report}");
                    ReportReceived?.Invoke(report);
                }
                else
                {
                    Log.Write($"Ignored message from {device.ProductID:X4}: {BitConverter.ToString(buffer, 0, Math.Min(count, 12))}");
                }
            }
        }
        catch (Exception e) when (e is IOException or TimeoutException or ObjectDisposedException)
        {
            // Unplugged or shutting down, a later rescan reopens it if it comes back
            if (!_disposed) Log.Write($"Closed {device.ProductID:X4}: {e.Message}");
        }
        finally
        {
            bool removed;
            lock (_lock) removed = _open.Remove(device.DevicePath);
            stream.Dispose();
            if (removed && !_disposed) DevicesChanged?.Invoke();
        }
    }

    public void Dispose()
    {
        DeviceList.Local.Changed -= OnDeviceListChanged;

        List<HidStream> streams;
        lock (_lock)
        {
            _disposed = true;
            streams = _open.Values.Select(entry => entry.Stream).ToList();
            _open.Clear();
        }

        foreach (var stream in streams) stream.Dispose();
    }
}
