#!/usr/bin/env bash
# Builds the FreeType linux-x64 binary for AlvorKit.FreeType.Native, at the
# version pinned in native/freetype/TAG (upstream tag VER-x-y-z). Designed
# for Ubuntu 24.04.
# Output: native/freetype/runtimes/linux-x64/native/libfreetype.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
UPSTREAM_TAG="VER-${VERSION//./-}"
WORK_DIR="$HOME/freetype-build"
SRC_DIR="$WORK_DIR/freetype-$UPSTREAM_TAG"
OUT_DIR="$SCRIPT_DIR/../runtimes/linux-x64/native"

sudo apt-get update -qq
sudo apt-get install -y -qq build-essential cmake curl ca-certificates

# Fetch the pinned source from upstream GitLab.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://gitlab.freedesktop.org/freetype/freetype/-/archive/$UPSTREAM_TAG/freetype-$UPSTREAM_TAG.tar.gz" | tar xz

# Build. Internal zlib, optional deps off — zero library dependencies.
cmake --fresh -S "$SRC_DIR" -B "$WORK_DIR/build-linux64" \
    -DCMAKE_BUILD_TYPE=Release \
    -DBUILD_SHARED_LIBS=ON \
    -DFT_DISABLE_ZLIB=ON -DFT_DISABLE_BZIP2=ON -DFT_DISABLE_PNG=ON \
    -DFT_DISABLE_HARFBUZZ=ON -DFT_DISABLE_BROTLI=ON
cmake --build "$WORK_DIR/build-linux64" -j

# CMake produces libfreetype.so.6.x.y plus symlinks; ship the resolved real
# file under the name the .NET loader probes for.
cp "$(readlink -f "$WORK_DIR/build-linux64/libfreetype.so")" "$OUT_DIR/libfreetype.so"
strip "$OUT_DIR/libfreetype.so"

# Verify: only libc/libm expected.
ldd "$OUT_DIR/libfreetype.so"
echo "OK $OUT_DIR/libfreetype.so"
