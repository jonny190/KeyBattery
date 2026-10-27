namespace KeyBattery;

/// <summary>The tray icon, its menu and notifications. Runs on the UI thread.</summary>
sealed class TrayApp : ApplicationContext
{
    // The firmware pushes every minute while awake, so a few missed pushes
    // means the keyboard is asleep, switched off or out of range
    static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(3);

    readonly Control _ui = new();
    readonly NotifyIcon _tray;
    readonly ToolStripMenuItem _statusItem;
    readonly ToolStripMenuItem _startupItem;
    readonly System.Windows.Forms.Timer _staleTimer;
    readonly KeyboardLink _link = new();
    readonly AlertPolicy _alerts = new();

    AppSettings _settings = AppSettings.Load();
    BatteryReport? _last;
    Icon? _icon;
    SettingsForm? _settingsForm;

    public TrayApp()
    {
        // Hidden control used to hop from the HID reader threads onto the UI thread
        _ui.CreateControl();

        _statusItem = new ToolStripMenuItem { Enabled = false };
        _startupItem = new ToolStripMenuItem("Start with Windows", null, OnToggleStartup) { Checked = StartWithWindows.IsEnabled };

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Refresh now", null, (_, _) => _link.RequestUpdate());
        menu.Items.Add("Settings", null, (_, _) => ShowSettings());
        menu.Items.Add(_startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());

        _tray = new NotifyIcon { ContextMenuStrip = menu, Visible = true };
        _tray.DoubleClick += (_, _) => ShowSettings();

        _staleTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
        _staleTimer.Tick += (_, _) => UpdateView();
        _staleTimer.Start();

        _link.ReportReceived += report => Post(() => OnReport(report));
        _link.DevicesChanged += () => Post(UpdateView);

        UpdateView();
        _link.Start();
    }

    void Post(Action action)
    {
        if (_ui.IsDisposed) return;
        try
        {
            _ui.BeginInvoke(action);
        }
        catch (InvalidOperationException)
        {
            // Handle already gone during shutdown
        }
    }

    void OnReport(BatteryReport report)
    {
        _last = report;

        switch (_alerts.Evaluate(report, _settings))
        {
            case Alert.Warning:
                _tray.ShowBalloonTip(10_000, "Keyboard battery low",
                    $"Your keyboard is at {report.Percentage}%. Plug it in when you get a chance.", ToolTipIcon.Warning);
                break;
            case Alert.Critical:
                _tray.ShowBalloonTip(10_000, "Keyboard battery critical",
                    $"Your keyboard is at {report.Percentage}% and will switch itself off soon. Plug it in now.", ToolTipIcon.Error);
                break;
        }

        UpdateView();
    }

    void UpdateView()
    {
        var report = _last;
        bool stale = report == null || DateTime.Now - report.ReceivedAt > StaleAfter;
        bool measured = report is { Measured: true };

        string status = Describe(report, stale);
        _statusItem.Text = status;
        // NotifyIcon.Text is capped at 127 characters
        _tray.Text = status.Length > 127 ? status[..127] : status;

        var previous = _icon;
        _icon = TrayIconRenderer.Render(measured ? report!.Percentage : null, report?.IsCharging == true, stale, _settings);
        _tray.Icon = _icon;
        previous?.Dispose();

        _settingsForm?.ShowStatus(status);
    }

    string Describe(BatteryReport? report, bool stale)
    {
        if (report == null)
        {
            return _link.OpenCount == 0
                ? "Keyboard receiver not found"
                : "Waiting for the keyboard, press a key to wake it";
        }

        // Keychron's firmware only samples the battery in wireless modes
        if (!report.Measured && report.Transport == Transport.Usb)
            return "Keyboard is in cable mode, switch to 2.4GHz to read the battery";

        if (!report.Measured) return "Keyboard is measuring its battery";

        string reading = $"Keyboard battery {report.Percentage}%";
        if (stale) return $"{reading} at {report.ReceivedAt:t}, keyboard asleep or out of range";

        return report.Charge switch
        {
            ChargeState.Charging => $"{reading}, charging",
            ChargeState.Full => $"{reading}, fully charged",
            _ => $"{reading} ({report.Millivolts / 1000.0:0.00} V)",
        };
    }

    void ShowSettings()
    {
        if (_settingsForm == null)
        {
            _settingsForm = new SettingsForm(_settings);
            _settingsForm.Saved += settings =>
            {
                _settings = settings;
                UpdateView();
            };
            _settingsForm.FormClosed += (_, _) => _settingsForm = null;
            _settingsForm.ShowStatus(_statusItem.Text ?? "");
        }

        _settingsForm.Show();
        _settingsForm.Activate();
    }

    void OnToggleStartup(object? sender, EventArgs e)
    {
        StartWithWindows.Set(!_startupItem.Checked);
        _startupItem.Checked = StartWithWindows.IsEnabled;
    }

    protected override void ExitThreadCore()
    {
        _staleTimer.Stop();
        _link.Dispose();
        _settingsForm?.Close();
        _tray.Visible = false;
        _tray.Dispose();
        _icon?.Dispose();
        _ui.Dispose();
        base.ExitThreadCore();
    }
}
