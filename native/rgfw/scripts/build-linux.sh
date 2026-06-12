#!/usr/bin/env bash
# Builds the RGFW linux-x64 binary for AlvorKit.RGFW.Native, at the tag pinned
# in native/rgfw/TAG. Designed for Ubuntu 24.04.
# Output: native/rgfw/runtimes/linux-<arch>/native/libRGFW.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/rgfw-build"
SRC_DIR="$WORK_DIR/RGFW-$VERSION"
TARGET="${1:-native}"
CC=gcc
if [[ "$TARGET" == "arm" ]]; then
    RID="linux-arm"
    CC=arm-linux-gnueabihf-gcc
else
    case "$(uname -m)" in
        x86_64) RID="linux-x64" ;;
        aarch64) RID="linux-arm64" ;;
        *) echo "Unsupported architecture: $(uname -m)" >&2; exit 1 ;;
    esac
fi
OUT_DIR="$SCRIPT_DIR/../runtimes/$RID/native"

# Toolchain + X11 dev headers; RGFW dlopens optional extensions (Xcursor, Xi) at runtime.
if [[ "$TARGET" == "arm" ]]; then
    # 32-bit ARM cross build: armhf toolchain plus armhf X11/GL dev libraries.
    sudo dpkg --add-architecture armhf
    sudo apt-get update -qq
    sudo apt-get install -y -qq build-essential curl ca-certificates gcc-arm-linux-gnueabihf \
        libx11-dev:armhf libxrandr-dev:armhf libxcursor-dev:armhf libxi-dev:armhf libgl1-mesa-dev:armhf
else
    sudo apt-get update -qq
    sudo apt-get install -y -qq build-essential curl ca-certificates \
        libx11-dev libxrandr-dev libxcursor-dev libxi-dev libgl1-mesa-dev
fi

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/ColleagueRiley/RGFW/archive/refs/tags/$VERSION.tar.gz" | tar xz

# Build. Link line per upstream's Makefile; --no-undefined turns underlinking
# into a build error instead of a load-time failure on a user's machine.
"$CC" -shared -fPIC -O2 -Wl,--no-undefined -I "$SRC_DIR" \
    -o "$OUT_DIR/libRGFW.so" "$SCRIPT_DIR/../rgfw.c" \
    -lXrandr -lX11 -ldl -lpthread -lm -lGL
strip "$OUT_DIR/libRGFW.so"

# Verify: only X11/GL/system libraries expected.
ldd "$OUT_DIR/libRGFW.so"
echo "OK $OUT_DIR/libRGFW.so"
