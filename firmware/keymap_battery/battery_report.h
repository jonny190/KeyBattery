/* Battery reporting over raw HID for Keychron wireless keyboards.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 */

#pragma once

/* The report rides on VIA's custom value command (id_custom_get_value, 0x08)
 * using a channel that VIA itself never uses. See docs/protocol.md.
 *
 * Report layout, 32 bytes:
 *   [0] 0x08 id_custom_get_value
 *   [1] BATT_CHANNEL_ID
 *   [2] BATT_VALUE_ID_STATUS
 *   [3] BATT_PROTOCOL_VERSION
 *   [4] battery percentage, 0 to 100
 *   [5] battery voltage in mV, high byte
 *   [6] battery voltage in mV, low byte
 *   [7] charge state: 0 not charging, 1 charging, 2 full
 *   [8] transport bitmask: 1 USB, 2 Bluetooth, 4 2.4GHz
 *   [9] flags, see BATT_FLAG_*
 */
#define BATT_CHANNEL_ID 0xB7
#define BATT_VALUE_ID_STATUS 0x01
#define BATT_PROTOCOL_VERSION 0x01
#define BATT_REPORT_LEN 32

#define BATT_FLAG_MEASURED (0x01U << 0)
#define BATT_FLAG_LOW (0x01U << 1)

/* Push a report at least this often while connected wirelessly */
#ifndef BATT_PUSH_INTERVAL
#    define BATT_PUSH_INTERVAL 60000
#endif

/* Never push more often than this, even if the percentage changes */
#ifndef BATT_PUSH_MIN_INTERVAL
#    define BATT_PUSH_MIN_INTERVAL 10000
#endif
