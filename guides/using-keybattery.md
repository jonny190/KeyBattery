# Using KeyBattery

KeyBattery sits in the Windows system tray and shows your keyboard's battery level. It warns you when the battery is getting low, so the keyboard doesn't die halfway through something.

It only works once the keyboard has the battery firmware on it. See [flashing-the-firmware.md](flashing-the-firmware.md).

## Getting it running

1. Download `KeyBattery.exe` from the [latest release](https://github.com/jonny190/KeyBattery/releases/latest) and put it somewhere it can stay, such as `C:\Tools\KeyBattery`.
2. Double click it. There's no installer and nothing else to install. The exe isn't code signed, so Windows SmartScreen may warn you the first time. Choose More info, then Run anyway.
3. Look for a small coloured square with a number on it in the tray. If you can't see it, click the up arrow next to the clock. You can drag it out onto the taskbar so it's always visible.
4. Right click it and tick **Start with Windows** so it's there after every restart.

## Reading the icon

| Icon | Meaning |
|------|---------|
| Green with a number | Battery is fine |
| Amber | At or below your warning level |
| Red | At or below your critical level |
| Blue | Charging |
| Grey with a number | Last known level. The keyboard is asleep, switched off or out of range. |
| Grey `?` | No reading yet |

Hover over the icon for the details, such as "Keyboard battery 64% (3.86 V)" or "Keyboard battery 100%, charging".

The keyboard sends an update about once a minute while you're using it, and straight away when the level changes. When it goes to sleep the updates stop, which is why the icon goes grey after a few minutes of not typing. Press any key and it'll catch up within a few seconds.

## Notifications

- **Battery low** appears once when the level reaches your warning level (20% unless you change it).
- **Battery critical** appears at your critical level (10% unless you change it), and again every 30 minutes until you plug the keyboard in.

Both reset once you start charging.

## Changing the warning levels

Right click the icon and choose **Settings**, or double click the icon. Set the two levels and click **Save**. The critical level has to be lower than the warning level.

## The menu

- **Refresh now** asks the keyboard for a fresh reading. You don't normally need it.
- **Settings** opens the warning levels.
- **Start with Windows** runs KeyBattery when you log in.
- **Exit** closes KeyBattery.

## Troubleshooting

**"Keyboard receiver not found".** The 2.4GHz receiver isn't plugged in, or Windows hasn't finished setting it up. Plug it in and KeyBattery picks it up on its own.

**Grey `?` saying the keyboard is in cable mode.** Keychron's firmware only measures the battery when the switch is on 2.4GHz or Bluetooth. Slide it to 2.4GHz. You can leave the cable plugged in to charge, and the icon turns blue while it charges.

**Stuck on "Waiting for the keyboard".** Press a key to wake the keyboard and check the switch is on 2.4GHz. If it still never shows a reading, the keyboard is probably still on Keychron's stock firmware, which doesn't send battery information.

**Want to see what it's doing?** KeyBattery keeps a log at `%AppData%\KeyBattery\log.txt`. Paste that into Explorer's address bar to open it. It starts fresh each time the app starts. A working setup shows a line saying it opened the raw HID interface, then lines starting "Report from D030" about once a minute.

**Does it drain the keyboard battery?** Barely. The app never wakes the keyboard. It only listens for updates the keyboard sends while it's awake anyway, and asks once when the app starts.
