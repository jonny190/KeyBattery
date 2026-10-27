# Flashing the battery firmware

This puts the battery reporting firmware on your Keychron V6 Max. It takes about ten minutes and you only need to do it once. If you ever want to go back, you can flash Keychron's stock firmware the same way.

## Before you start

You'll need:

- the USB cable that came with the keyboard
- [QMK Toolbox](https://github.com/qmk/qmk_toolbox/releases) for Windows (the `qmk_toolbox_install.exe` installer also installs the driver it needs)
- the right firmware file from the [latest release](https://github.com/jonny190/KeyBattery/releases/latest)

### Back up your layout

Flashing resets any remaps, macros and lighting you've set in Keychron Launcher or VIA. If you've changed anything, open [Keychron Launcher](https://launcher.keychron.com), connect the keyboard and export your layout to a file first. You can import it again afterwards.

### Pick the right file

There are three files, one for each layout:

| Your keyboard | File |
|---------------|------|
| ANSI (US style, wide left Shift) | `keychron_v6_max_ansi_encoder_battery.bin` |
| ISO (UK and European, tall Enter key) | `keychron_v6_max_iso_encoder_battery.bin` |
| JIS (Japanese) | `keychron_v6_max_jis_encoder_battery.bin` |

If you're not sure, plug the keyboard in by cable with the switch on Cable and open Device Manager. Find the Keychron entry under Keyboards, open its Properties, then the Details tab, then Hardware Ids. `PID_0960` is ANSI, `PID_0961` is ISO and `PID_0962` is JIS.

Download the one you need from the [latest release](https://github.com/jonny190/KeyBattery/releases/latest). If you'd rather build it yourself, see [docs/firmware.md](../docs/firmware.md).

## Put the keyboard into flashing mode

1. Slide the mode switch on the keyboard to **Cable**.
2. Unplug the cable from the keyboard.
3. Pull off the space bar keycap. On the PCB, just left of the space bar switch, there's a small reset button.
4. Hold the reset button down and plug the cable back in. Keep holding for about two seconds, then let go.

If you'd rather not remove the keycap, holding **Esc** while you plug the cable in usually works too.

The keyboard won't light up or type in this mode. That's normal.

## Flash it

1. Open QMK Toolbox. When the keyboard is in flashing mode, the log shows a yellow line saying an STM32 DFU device has connected.
2. Click **Open** and choose your `.bin` file.
3. Click **Flash**.
4. Wait for the log to say the flash is complete. **Don't unplug the cable while it's flashing.**
5. Unplug the cable, put the space bar keycap back, and plug it in again. The keyboard should start up normally.

## Afterwards

- Press **Fn + J + Z** together for four seconds to factory reset. This clears any stale settings left over from the old firmware.
- If you backed up your layout, import it in Keychron Launcher.
- Slide the switch back to **2.4GHz**. The receiver doesn't need pairing again.
- Start KeyBattery. See [using-keybattery.md](using-keybattery.md).

## If something goes wrong

**QMK Toolbox doesn't see the keyboard.** Make sure the switch is on Cable and try the reset button again. Hold it before the cable goes in, not after. If QMK Toolbox installed without its driver, run its Tools menu, Install Drivers.

**The keyboard doesn't type after flashing.** Check that you flashed the file that matches your layout. Then flash again. You can't break the bootloader this way, because the reset button always gets you back into flashing mode.

**Going back to stock firmware.** Download the V6 Max firmware from [Keychron's firmware page](https://www.keychron.com/pages/firmware-and-json-files-of-the-keychron-qmk-v-and-v-max-series-keyboards) and flash it the same way.
