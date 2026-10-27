# Battery report protocol

The keyboard and KeyBattery talk over QMK's raw HID interface (usage page `0xFF60`, usage `0x61`). On the 2.4GHz receiver that's interface `MI_03` of `VID_3434&PID_D030`. When the keyboard is plugged in by cable, its own USB device exposes the same interface.

Every message is 32 bytes. On Windows each read and write is 33 bytes, because the first byte is the HID report ID (always 0) and isn't part of the message.

## Why it piggybacks on VIA

The firmware already runs VIA, and VIA owns the raw HID interface. Rather than invent a new top level command that could clash with VIA or Keychron Launcher, the battery report uses VIA's existing "get custom value" command (`0x08`) on a channel VIA never uses (`0xB7`). VIA hands anything on an unknown channel to `via_custom_value_command_kb()`, which is where the firmware module answers it. Anything else gets VIA's normal "unhandled" reply (`0xFF`).

## Query, host to keyboard

| Byte | Value | Meaning |
|------|-------|---------|
| 0 | `0x08` | VIA `id_custom_get_value` |
| 1 | `0xB7` | Battery channel |
| 2 | `0x01` | Battery status value |
| 3 to 31 | `0x00` | Unused |

Every message the keyboard receives over wireless restarts its sleep timer (that's in Keychron's `wireless.c`). So KeyBattery only sends a query when it starts, when a receiver is plugged in, or when you choose Refresh now. It never polls.

## Report, keyboard to host

The answer to a query and the unprompted pushes use the same layout.

| Byte | Meaning |
|------|---------|
| 0 | `0x08` |
| 1 | `0xB7` |
| 2 | `0x01` |
| 3 | Protocol version, currently `0x01`. Later versions only add bytes on the end. |
| 4 | Battery percentage, 0 to 100 |
| 5 | Battery voltage in millivolts, high byte |
| 6 | Battery voltage in millivolts, low byte |
| 7 | Charge state: 0 not charging, 1 charging, 2 full |
| 8 | Transport bitmask: 1 USB, 2 Bluetooth, 4 2.4GHz |
| 9 | Flags: bit 0 measured, bit 1 low |
| 10 to 31 | `0x00` |

**Measured** stays clear for the first few seconds after power on. Until then the firmware hasn't sampled the battery and the voltage reads as full, so KeyBattery ignores the percentage.

**Low** is the firmware's own low battery state, the same one that lights the low battery LED. It sets when the voltage stays under 3.5 V. KeyBattery treats it as critical whatever the percentage says.

The percentage is Keychron's estimate from voltage: 3.3 V is 0%, 3.5 V is 20% and 4.1 V is 100%, linear in between. It reads high while charging because the charger lifts the voltage.

## Pushes

While the keyboard is connected over 2.4GHz or Bluetooth, the firmware sends a report:

- as soon as the link comes up (after the power on samples are in)
- whenever the percentage changes, at most once every 10 seconds
- otherwise once a minute

Sending to the host doesn't touch the sleep timer, so the pushes don't stop the keyboard sleeping. Once it's asleep the pushes stop, and KeyBattery marks the reading as stale after three minutes without one.

The intervals are `BATT_PUSH_INTERVAL` and `BATT_PUSH_MIN_INTERVAL` in `firmware/keymap_battery/battery_report.h`.
