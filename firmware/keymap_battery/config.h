/* Battery reporting over raw HID for Keychron wireless keyboards.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 */

#pragma once

/* Route raw HID received over 2.4GHz and Bluetooth into the firmware.
 * The spelling matches the define in common/wireless/wireless.c. */
#define WILRESS_RAW_ENABLE

/* wireless.c calls via_raw_hid_receive(), which does not exist in this
 * tree. QMK's VIA entry point is raw_hid_receive() with the same signature,
 * and this config is included in every file, so map the name across rather
 * than editing Keychron's common code. */
#define via_raw_hid_receive raw_hid_receive
