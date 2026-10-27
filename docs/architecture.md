# How it fits together

```
Keychron V6 Max                       2.4GHz receiver              Windows PC
+-----------------------------+       (VID 3434 PID D030)          +----------------------+
| battery.c measures voltage  |                                    | KeyBattery.exe       |
|            |                |       raw HID interface MI_03      |                      |
| battery_report.c            | ----> usage page 0xFF60 ---------> | KeyboardLink reads   |
|   pushes every minute       |                                    | AlertPolicy decides  |
|   answers 0x08 / 0xB7 query | <---- one query at start up <----- | TrayApp shows it     |
|            |                |                                    +----------------------+
| LKBT51 wireless module      |
+-----------------------------+
```

The project has two independent parts that share one protocol.

- **firmware/** adds a keymap to Keychron's QMK fork. It turns on raw HID over wireless and adds the battery report. See [firmware.md](firmware.md).
- **app/** is the Windows tray app. See [app.md](app.md).
- **docs/protocol.md** is the contract between them. If you change one side, check the other. See [protocol.md](protocol.md).

## Decisions worth knowing

**Push, not poll.** Any message sent to the keyboard over wireless restarts its sleep timer. An app polling every minute would keep the keyboard awake permanently and drain the battery faster than without the app. So the keyboard pushes, and the app only asks once when it starts.

**Keymap module, not a fork.** Keychron's code isn't edited. The module sits beside their stock keymap and the build fetches their repo at a pinned commit. Moving to a newer Keychron release is a one line change plus the checks in [firmware.md](firmware.md).

**VIA custom value channel, not a new command.** Using VIA's existing get value command on an unused channel means the battery report can't clash with VIA or with Keychron Launcher's own commands, and Launcher keeps working.

**Self-contained exe.** It's bigger, but there's no .NET runtime to install first.
