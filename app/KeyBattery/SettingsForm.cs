namespace KeyBattery;

/// <summary>Alert thresholds and current status.</summary>
sealed class SettingsForm : Form
{
    readonly NumericUpDown _warnAt;
    readonly NumericUpDown _criticalAt;
    readonly Label _status;
    readonly Label _error;

    public event Action<AppSettings>? Saved;

    public SettingsForm(AppSettings settings)
    {
        Text = "KeyBattery settings";
        Icon = LoadIcon();
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);
        Font = new Font("Segoe UI", 9f);

        _status = new Label { AutoSize = true, MaximumSize = new Size(360, 0), Margin = new Padding(0, 0, 0, 12) };
        _warnAt = new NumericUpDown { Minimum = 1, Maximum = 99, Value = settings.WarnAt, Width = 60 };
        _criticalAt = new NumericUpDown { Minimum = 1, Maximum = 99, Value = settings.CriticalAt, Width = 60 };
        _error = new Label { AutoSize = true, ForeColor = Color.Firebrick, Visible = false };

        var thresholds = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 0, 0, 8) };
        thresholds.Controls.Add(new Label { Text = "Warn me at (%)", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        thresholds.Controls.Add(_warnAt, 1, 0);
        thresholds.Controls.Add(new Label { Text = "Critical warning at (%)", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        thresholds.Controls.Add(_criticalAt, 1, 1);

        var save = new Button { Text = "Save", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        save.Click += (_, _) => OnSave();
        cancel.Click += (_, _) => Close();
        AcceptButton = save;
        CancelButton = cancel;

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(save);

        var layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Dock = DockStyle.Fill };
        layout.Controls.Add(_status);
        layout.Controls.Add(thresholds);
        layout.Controls.Add(_error);
        layout.Controls.Add(buttons);
        Controls.Add(layout);
    }

    public void ShowStatus(string status) => _status.Text = status;

    void OnSave()
    {
        if (_criticalAt.Value >= _warnAt.Value)
        {
            _error.Text = "The critical level needs to be lower than the warning level.";
            _error.Visible = true;
            return;
        }

        var settings = new AppSettings { WarnAt = (int)_warnAt.Value, CriticalAt = (int)_criticalAt.Value };
        settings.Save();
        Saved?.Invoke(settings);
        Close();
    }

    static Icon LoadIcon()
    {
        using var stream = typeof(SettingsForm).Assembly.GetManifestResourceStream("app.ico")!;
        return new Icon(stream);
    }
}
