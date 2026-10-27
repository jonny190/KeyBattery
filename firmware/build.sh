#!/usr/bin/env bash
# Builds the battery reporting firmware for the Keychron V6 Max.
#
# Fetches Keychron's QMK fork at a pinned commit, adds the battery keymap
# alongside Keychron's stock keymap for each layout, and compiles inside the
# official QMK Docker image. Finished .bin files land in firmware/dist.
#
# Usage: ./build.sh [layout ...]   (default: ansi_encoder iso_encoder jis_encoder)

set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KEYCHRON_REPO="https://github.com/Keychron/qmk_firmware.git"
KEYCHRON_REF="2b493ff95f2665db719b1e95c91babb5036e72f3" # branch 2025q3
QMK_IMAGE="qmkfm/qmk_cli"
KEYBOARD="keychron/v6_max"
KEYMAP="battery"

QMK_DIR="$HERE/.build/qmk_firmware"
DIST_DIR="$HERE/dist"
LAYOUTS=("$@")
[ ${#LAYOUTS[@]} -eq 0 ] && LAYOUTS=(ansi_encoder iso_encoder jis_encoder)

fetch_keychron() {
    if [ -d "$QMK_DIR/.git" ] && [ "$(git -C "$QMK_DIR" rev-parse HEAD)" = "$KEYCHRON_REF" ]; then
        return
    fi

    echo "Fetching Keychron firmware at $KEYCHRON_REF"
    rm -rf "$QMK_DIR"
    mkdir -p "$QMK_DIR"
    git -C "$QMK_DIR" init -q
    git -C "$QMK_DIR" remote add origin "$KEYCHRON_REPO"
    git -C "$QMK_DIR" fetch -q --depth 1 origin "$KEYCHRON_REF"
    git -C "$QMK_DIR" checkout -q FETCH_HEAD
    git -C "$QMK_DIR" submodule update -q --init --depth 1
}

install_keymap() {
    local layout="$1"
    local layout_dir="$QMK_DIR/keyboards/$KEYBOARD/$layout"
    local target="$layout_dir/keymaps/$KEYMAP"

    rm -rf "$target"
    mkdir -p "$target"
    # Keep Keychron's stock layout, the module only adds files around it
    cp "$layout_dir/keymaps/keychron/keymap.c" "$target/keymap.c"
    cp "$HERE/keymap_battery/"* "$target/"
}

build_layout() {
    local layout="$1"
    local name="${KEYBOARD//\//_}_${layout}_${KEYMAP}"

    echo "Building $KEYBOARD/$layout:$KEYMAP"
    docker run --rm \
        --user "$(id -u):$(id -g)" \
        -e QMK_USERSPACE= \
        -v "$QMK_DIR":/qmk_firmware \
        -w /qmk_firmware \
        "$QMK_IMAGE" \
        make "$KEYBOARD/$layout:$KEYMAP"

    cp "$QMK_DIR/$name.bin" "$DIST_DIR/$name.bin"
    echo "Built $DIST_DIR/$name.bin"
}

fetch_keychron
mkdir -p "$DIST_DIR"
for layout in "${LAYOUTS[@]}"; do
    install_keymap "$layout"
    build_layout "$layout"
done
