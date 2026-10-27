namespace KeyBattery;

public enum ChargeState : byte
{
    NotCharging = 0,
    Charging = 1,
    Full = 2,
}

[Flags]
public enum Transport : byte
{
    None = 0,
    Usb = 1 << 0,
    Bluetooth = 1 << 1,
    Wireless24 = 1 << 2,
}

/// <summary>
/// Battery status sent by the keyboard firmware. The wire format is described
/// in docs/protocol.md and must match firmware/keymap_battery/battery_report.h.
/// </summary>
public sealed record BatteryReport(
    int Percentage,
    int Millivolts,
    ChargeState Charge,
    Transport Transport,
    bool Measured,
    bool LowFlag,
    DateTime ReceivedAt)
{
    const byte CustomGetValue = 0x08;
    const byte ChannelId = 0xB7;
    const byte ValueIdStatus = 0x01;
    const byte MinProtocolVersion = 0x01;
    const byte FlagMeasured = 1 << 0;
    const byte FlagLow = 1 << 1;

    public bool IsCharging => Charge != ChargeState.NotCharging;

    /// <summary>Builds the query, sized to the device's output report including the report ID byte.</summary>
    public static byte[] BuildQuery(int reportLength)
    {
        var buffer = new byte[reportLength];
        buffer[1] = CustomGetValue;
        buffer[2] = ChannelId;
        buffer[3] = ValueIdStatus;
        return buffer;
    }

    /// <summary>Returns null for anything that is not a battery report, such as VIA traffic.</summary>
    public static BatteryReport? TryParse(ReadOnlySpan<byte> report)
    {
        // report[0] is the HID report ID, the firmware's bytes start after it
        if (report.Length < 11) return null;
        var data = report[1..];

        if (data[0] != CustomGetValue || data[1] != ChannelId || data[2] != ValueIdStatus) return null;
        if (data[3] < MinProtocolVersion) return null;

        return new BatteryReport(
            Percentage: Math.Min((int)data[4], 100),
            Millivolts: (data[5] << 8) | data[6],
            Charge: (ChargeState)data[7],
            Transport: (Transport)data[8],
            Measured: (data[9] & FlagMeasured) != 0,
            LowFlag: (data[9] & FlagLow) != 0,
            ReceivedAt: DateTime.Now);
    }
}
