/* Battery reporting over raw HID for Keychron wireless keyboards.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 */

#include QMK_KEYBOARD_H
#include <string.h>
#include "via.h"
#include "raw_hid.h"
#include "keychron_raw_hid.h"
#include "transport.h"
#include "wireless.h"
#include "battery.h"
#include "lpm.h"
#include "battery_report.h"

static uint32_t push_timer      = 0;
static uint8_t  last_percentage = 0xFF;

static uint8_t charge_state(void) {
#ifdef BAT_CHARGING_PIN
    if (usb_power_connected()) {
        return gpio_read_pin(BAT_CHARGING_PIN) == BAT_CHARGING_LEVEL ? BAT_CHARGING : BAT_FULL_CHARGED;
    }
#endif
    return BAT_NOT_CHARGING;
}

static void fill_report(uint8_t *data) {
    uint16_t mv    = battery_get_voltage();
    uint8_t  flags = 0;

    /* Voltage reads as full until the power on samples are taken */
    if (!battery_power_on_sample()) flags |= BATT_FLAG_MEASURED;
    if (battery_is_empty()) flags |= BATT_FLAG_LOW;

    data[0] = id_custom_get_value;
    data[1] = BATT_CHANNEL_ID;
    data[2] = BATT_VALUE_ID_STATUS;
    data[3] = BATT_PROTOCOL_VERSION;
    data[4] = battery_get_percentage();
    data[5] = mv >> 8;
    data[6] = mv & 0xFF;
    data[7] = charge_state();
    data[8] = get_transport();
    data[9] = flags;
}

/* Answers a query from the host. VIA sends the buffer back on the
 * transport the query arrived on, so this works over USB and wireless. */
void via_custom_value_command_kb(uint8_t *data, uint8_t length) {
    // data = [ command_id, channel_id, value_id, value_data ]
    if (data[0] == id_custom_get_value && data[1] == BATT_CHANNEL_ID && data[2] == BATT_VALUE_ID_STATUS) {
        fill_report(data);
        return;
    }

    data[0] = id_unhandled;
}

/* Pushes a report while connected wirelessly so the host never has to poll.
 * Host to keyboard raw HID resets the sleep timer, keyboard to host does not. */
void housekeeping_task_user(void) {
    if (!(get_transport() & TRANSPORT_WIRELESS) || wireless_get_state() != WT_CONNECTED) {
        /* Report again as soon as the link comes back */
        last_percentage = 0xFF;
        return;
    }

    if (battery_power_on_sample()) return;

    uint8_t  percentage = battery_get_percentage();
    uint32_t elapsed    = timer_elapsed32(push_timer);
    bool     reconnect  = last_percentage == 0xFF;
    bool     changed    = percentage != last_percentage && elapsed > BATT_PUSH_MIN_INTERVAL;

    if (!reconnect && !changed && elapsed < BATT_PUSH_INTERVAL) return;

    uint8_t report[BATT_REPORT_LEN];
    memset(report, 0, sizeof(report));
    fill_report(report);
    kc_raw_hid_send(RAW_HID_SRC_WIRELESS, report, sizeof(report));

    last_percentage = percentage;
    push_timer      = timer_read32();
}
