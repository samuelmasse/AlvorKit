#!/usr/bin/env bash
# Builds the miniaudio linux-x64 binary for AlvorKit.MiniAudio.Native, at the
# tag pinned in native/miniaudio/TAG. Designed for Ubuntu 24.04. miniaudio
# dlopens its backends (ALSA/PulseAudio/...) at runtime, so no audio dev
# headers are needed.
# Output: native/miniaudio/runtimes/linux-x64/native/libminiaudio.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/miniaudio-build"
SRC_DIR="$WORK_DIR/miniaudio-$VERSION"
OUT_DIR="$SCRIPT_DIR/../runtimes/linux-x64/native"

sudo apt-get update -qq
sudo apt-get install -y -qq build-essential curl ca-certificates

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/mackron/miniaudio/archive/refs/tags/$VERSION.tar.gz" | tar xz

# Build. Link line per upstream docs (dl, pthread, m); --no-undefined turns
# underlinking into a build error instead of a load-time failure.
gcc -shared -fPIC -O2 -Wl,--no-undefined -I "$SRC_DIR" \
    -o "$OUT_DIR/libminiaudio.so" "$SCRIPT_DIR/../miniaudio.c" \
    -ldl -lpthread -lm
strip "$OUT_DIR/libminiaudio.so"

# Verify: only libc/libm/libpthread/libdl expected.
ldd "$OUT_DIR/libminiaudio.so"
echo "OK $OUT_DIR/libminiaudio.so"
