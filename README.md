# KeyBattery

Battery level for a Keychron V6 Max on its 2.4GHz receiver, shown in the Windows tray, with a warning before it runs flat.

Windows shows keyboard battery over Bluetooth, but not through Keychron's 2.4GHz receiver. Keychron's stock firmware doesn't send it that way either. This project fixes both ends:

- **firmware/** adds a small module to Keychron's own QMK firmware so the keyboard reports its battery through the receiver
- **app/** is KeyBattery, a Windows tray app that shows the level and notifies you when it's low

## Quick start

1. Download the firmware for your layout and `KeyBattery.exe` from the [latest release](https://github.com/jonny190/KeyBattery/releases/latest).
2. Flash the firmware: [guides/flashing-the-firmware.md](guides/flashing-the-firmware.md)
3. Run the app: [guides/using-keybattery.md](guides/using-keybattery.md)

Only tested on the V6 Max. Other Keychron Max boards share the same wireless code, so the module should carry over, but `firmware/build.sh` only builds the V6 Max.

## Building it yourself

`firmware/build.sh` and `app/build.sh` only need Docker. Outputs land in `firmware/dist` and `app/dist`. See [docs/firmware.md](docs/firmware.md) and [docs/app.md](docs/app.md).

## Documentation

- [docs/architecture.md](docs/architecture.md) how the parts fit together and why
- [docs/protocol.md](docs/protocol.md) the battery report format both sides share
- [docs/firmware.md](docs/firmware.md) what the firmware module changes and how to build it
- [docs/app.md](docs/app.md) how the app is put together

## Licence

- `firmware/` is GPL-2.0-or-later, the same as the Keychron QMK firmware it builds into. See [firmware/LICENSE](firmware/LICENSE).
- `app/` and everything else is MIT. See [LICENSE](LICENSE).

Not affiliated with Keychron.
