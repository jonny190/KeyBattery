# KeyBattery (KeyboardFW)

Battery monitor for a Keychron V6 Max used on its 2.4GHz receiver. Two parts that share one protocol:

- `firmware/` is a QMK keymap module added to Keychron's fork (pinned commit in `firmware/build.sh`). It enables raw HID over wireless and adds a battery report on VIA's custom value command, channel `0xB7`.
- `app/` is KeyBattery, a .NET 10 WinForms tray app using HidSharp.
- `docs/protocol.md` is the contract between them. Change both sides together.

## Building

Both build in Docker. Nothing else is needed on the machine.

- `firmware/build.sh [layout...]` produces `firmware/dist/keychron_v6_max_<layout>_battery.bin` for `ansi_encoder`, `iso_encoder` and `jis_encoder`
- `app/build.sh` produces `app/dist/KeyBattery.exe`, a self-contained single file

## Things that aren't obvious

- Keychron's code is never edited. `firmware/keymap_battery` is copied next to Keychron's stock `keymaps/keychron/keymap.c` at build time. Keep it that way.
- `WILRESS_RAW_ENABLE` is Keychron's spelling. Their `wireless.c` calls a nonexistent `via_raw_hid_receive()`, fixed by a `#define` in the keymap `config.h`. See `docs/firmware.md`.
- Host to keyboard raw HID over wireless resets the keyboard's sleep timer, so the app must never poll. The keyboard pushes every 60s while awake.
- Flashing any new build wipes Launcher/VIA customisations (the EEPROM is stamped with the build date).
- The dongle is VID `0x3434` PID `0xD030`. Its raw HID interface is `MI_03` (usage page `0xFF60`). `MI_01` with usage page `0x8C` is Keychron's own receiver channel, so leave it alone.
- This is WSL. `powershell.exe` works for Windows-side checks. Run Windows exes from a local copy (e.g. `%TEMP%`), not the `\\wsl.localhost` path, because the UNC path triggers a security prompt that blocks.
- The app logs to `%AppData%\KeyBattery\log.txt`.

## Status

- 2026-10-27: working end to end. ISO build flashed (keyboard PID `0x0961`). The app gets reports through the 2.4GHz receiver (e.g. 91%, 4.038 V, transport 2.4GHz). In cable mode the firmware never measures the battery, and the app says so.

## Conventions

- Docs live in `docs/`, user guides in `guides/`. Keep both updated with code changes.
- No em dashes or emojis in docs. Write naturally.
- This is a personal project, not a Coria one. No Powered by Coria branding.
