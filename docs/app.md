# KeyBattery app

A Windows tray app in C# on .NET 10 WinForms. It listens for battery reports from the keyboard and shows the level on the tray icon, with notifications when it runs low.

## Layout

| File | What it does |
|------|--------------|
| `Program.cs` | Entry point. Single instance mutex, starts the log, runs the tray. |
| `TrayApp.cs` | Tray icon, menu, notifications and the stale timer. Everything here runs on the UI thread. |
| `KeyboardLink.cs` | Finds Keychron raw HID interfaces with HidSharp, keeps a reader thread on each one and sends queries. Raises events on background threads. |
| `BatteryReport.cs` | Builds the query and parses reports. Must match [protocol.md](protocol.md). |
| `AlertPolicy.cs` | Decides when to notify. |
| `AppSettings.cs` | Thresholds stored in `%AppData%\KeyBattery\settings.json`, plus the Start with Windows registry entry. |
| `TrayIconRenderer.cs` | Draws the percentage on a coloured tile at the system's small icon size. |
| `SettingsForm.cs` | Thresholds and current status. |
| `Log.cs` | `%AppData%\KeyBattery\log.txt`, cleared on each start. |

## How it finds the keyboard

`KeyboardLink` lists every HID device with vendor ID `0x3434` and keeps the ones whose report descriptor includes usage `0xFF60:0x61`. That picks up the 2.4GHz receiver and, if it's plugged in, the keyboard's own USB interface. It doesn't pick up the receiver's other vendor interface (usage page `0x8C`), which belongs to Keychron's receiver tools.

Each match gets a background thread doing blocking reads. Messages that aren't battery reports, such as VIA traffic when Launcher is open, are logged and ignored. HidSharp's device list change event triggers a rescan, so plugging the receiver in later works.

When an interface opens, the app sends one query. After that it waits for the keyboard's pushes. It never polls, because every message sent to the keyboard over wireless restarts its sleep timer.

## Alerts

- **Warning** fires once when the level drops to the warning threshold (20% by default).
- **Critical** fires at the critical threshold (10% by default), or whenever the firmware sets its own low flag. It repeats every 30 minutes until the keyboard is charging.
- Both re-arm once the keyboard is charging, or the level climbs 5% above the warning threshold. The margin stops a reading that wobbles across the line from notifying over and over.
- Nothing fires on a reading taken before the firmware has finished its power on samples.

Notifications use `NotifyIcon.ShowBalloonTip`, which Windows 10 and 11 show as normal toasts.

## Tray icon colours

Green above the warning level, amber at or below it, red at or below critical, blue while charging. It turns grey with the last known percentage once three minutes pass without a push, which usually means the keyboard has gone to sleep. It shows a grey `?` before the first reading.

## Building

```
cd app
./build.sh
```

This uses the `mcr.microsoft.com/dotnet/sdk:10.0` Docker image and writes a self-contained single file `app/dist/KeyBattery.exe` of about 50 MB. It's that size because the .NET runtime is bundled, so nothing needs installing on the PC. On a Windows machine with the .NET 10 SDK, `dotnet publish KeyBattery -c Release -o dist` from the `app` folder does the same thing.

`EnableWindowsTargeting` in the project file is what lets a WinForms project build on Linux.

## Dependencies

- [HidSharp](https://www.nuget.org/packages/HidSharp) 2.1.0 (Apache 2.0) for HID access.
