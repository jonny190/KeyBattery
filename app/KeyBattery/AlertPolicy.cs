namespace KeyBattery;

public enum Alert
{
    None,
    Warning,
    Critical,
}

/// <summary>
/// Decides when to notify, so each threshold fires once per discharge rather
/// than on every report. Critical repeats on a timer until the keyboard is charged.
/// </summary>
public sealed class AlertPolicy
{
    // Must climb this far above the warning level before warnings re-arm,
    // otherwise a reading bouncing across the line would notify repeatedly
    const int RearmMargin = 5;
    static readonly TimeSpan CriticalRepeat = TimeSpan.FromMinutes(30);

    bool _warned;
    DateTime? _lastCritical;

    public Alert Evaluate(BatteryReport report, AppSettings settings)
    {
        if (!report.Measured) return Alert.None;

        if (report.IsCharging || report.Percentage > settings.WarnAt + RearmMargin)
        {
            _warned = false;
            _lastCritical = null;
            return Alert.None;
        }

        if (report.Percentage <= settings.CriticalAt || report.LowFlag)
        {
            if (_lastCritical is { } last && report.ReceivedAt - last < CriticalRepeat) return Alert.None;

            _lastCritical = report.ReceivedAt;
            _warned = true;
            return Alert.Critical;
        }

        if (report.Percentage <= settings.WarnAt && !_warned)
        {
            _warned = true;
            return Alert.Warning;
        }

        return Alert.None;
    }
}
