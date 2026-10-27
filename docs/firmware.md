# Firmware

The firmware is Keychron's own QMK fork for the V6 Max with one extra keymap added. Nothing in Keychron's code is edited. Everything we add lives in `firmware/keymap_battery` and gets copied in next to Keychron's stock keymap at build time.

## What stock firmware is missing

Two things stop stock firmware from reporting battery over the 2.4GHz receiver.

1. **No battery command.** Keychron's raw HID commands (`0xA0` to `0xAB` in `common/keychron_raw_hid.h`) cover versions, features, RGB, debounce and similar, but not the battery. The keyboard does measure it (`battery_get_percentage()` in `common/wireless/battery.c`) and sends it to the wireless module for Bluetooth's battery service, but the receiver firmware is closed and doesn't pass it on.

2. **Wireless raw HID is switched off.** The wireless module forwards raw HID from the receiver to the keyboard (`LKBT51_CMD_RAW_HID_RX`), but `common/wireless/wireless.c` only acts on it when `WILRESS_RAW_ENABLE` is defined (that spelling is Keychron's), and the V6 Max doesn't define it. Turning it on as is breaks the build, because the code calls `via_raw_hid_receive()`, which doesn't exist anywhere in the tree.

We confirmed this on real hardware before writing anything. The receiver exposes the raw HID interface, but stock firmware never answers on it.

## What the module does

`keymap_battery/config.h`

- defines `WILRESS_RAW_ENABLE`
- maps `via_raw_hid_receive` to `raw_hid_receive`, which is VIA's real entry point and has the same signature. QMK includes the keymap's `config.h` in every file it compiles, so the fix reaches `wireless.c` without editing it. It's a macro rename rather than a wrapper function because `wireless.c` doesn't declare the old name, and GCC 14 treats that as an error.

`keymap_battery/battery_report.c`

- `via_custom_value_command_kb()` answers the battery query described in [protocol.md](protocol.md). VIA leaves this hook weak and neither Keychron's common code nor the V6 Max code overrides it, so the keymap is free to.
- `housekeeping_task_user()` pushes a report while the keyboard is connected wirelessly. QMK's core calls it alongside Keychron's `housekeeping_task_kb()`, so the two don't collide.

`keymap_battery/rules.mk` turns on VIA (which brings raw HID with it) and adds `battery_report.c`.

`keymap.c` isn't stored here. The build copies Keychron's `keymaps/keychron/keymap.c` for whichever layout it's building, so the default layout matches stock exactly.

## Building

```
cd firmware
./build.sh                  # all three layouts
./build.sh iso_encoder      # just one
```

You need Docker and nothing else. The script:

1. Fetches Keychron's fork at a pinned commit (`KEYCHRON_REF` in the script, branch `2025q3`) into `firmware/.build/qmk_firmware`, with shallow submodules. Later runs reuse it.
2. Copies the module into `keyboards/keychron/v6_max/<layout>/keymaps/battery`.
3. Runs `make keychron/v6_max/<layout>:battery` in the `qmkfm/qmk_cli` image.
4. Copies the `.bin` into `firmware/dist`.

The three layouts are `ansi_encoder` (USB PID `0x0960`), `iso_encoder` (`0x0961`) and `jis_encoder` (`0x0962`).

### Moving to a newer Keychron commit

Change `KEYCHRON_REF` and rebuild. Then check these, since they're what the module relies on:

- `common/wireless/wireless.c` still guards `EVT_RAW_HID` with `WILRESS_RAW_ENABLE`. If Keychron fixes the spelling or the `via_raw_hid_receive` call, update `config.h` to match.
- `battery_get_percentage()`, `battery_get_voltage()`, `battery_power_on_sample()` and `battery_is_empty()` still exist in `battery.h`.
- Nothing in `keyboards/keychron` has started overriding `via_custom_value_command_kb()` or `housekeeping_task_user()`. If something does, the link fails with a duplicate symbol, so you'll know.

A quick check that the wireless path is really wired up after a build:

```
docker run --rm -v "$PWD/.build/qmk_firmware":/q -w /q qmkfm/qmk_cli \
  arm-none-eabi-objdump -dr .build/obj_keychron_v6_max_ansi_encoder_battery/keyboards/keychron/common/wireless/wireless.o \
  | grep raw_hid_receive
```

There should be a call to `raw_hid_receive`.

## Side effects of flashing

- **Launcher and VIA changes are wiped.** VIA stamps its saved data with the build date, so any new build resets remaps and macros to the defaults. Export your layout from Keychron Launcher first and import it afterwards.
- Keychron Launcher keeps working. The vendor and product IDs are unchanged and VIA is on.
- The wireless module and the receiver aren't touched.
- To go back, flash Keychron's stock firmware for the V6 Max the same way.
