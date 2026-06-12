#!/usr/bin/env bash
# Builds the FreeType linux-x64 binary for AlvorKit.FreeType.Native, at the
# version pinned in native/freetype/TAG (upstream tag VER-x-y-z). Designed
# for Ubuntu 24.04.
# Output: native/freetype/runtimes/linux-<arch>/native/libfreetype.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
UPSTREAM_TAG="VER-${VERSION//./-}"
WORK_DIR="$HOME/freetype-build"
SRC_DIR="$WORK_DIR/freetype-$UPSTREAM_TAG"
TARGET="${1:-native}"
CC=gcc
READELF=readelf
if [[ "$TARGET" == "arm" ]]; then
    RID="linux-arm"
    CC=arm-linux-gnueabihf-gcc
    READELF=arm-linux-gnueabihf-readelf
else
    case "$(uname -m)" in
        x86_64) RID="linux-x64" ;;
        aarch64) RID="linux-arm64" ;;
        *) echo "Unsupported architecture: $(uname -m)" >&2; exit 1 ;;
    esac
fi
OUT_DIR="$SCRIPT_DIR/../runtimes/$RID/native"

verify_needed_libraries() {
    local library="$1"
    shift
    local expected=" $* "
    local found=()
    local dependency

    mapfile -t found < <("$READELF" -d "$library" | sed -n 's/.*Shared library: \[\(.*\)\]/\1/p')

    printf "ELF dependencies for %s:\n" "$library"
    for dependency in "${found[@]}"; do
        printf "  %s\n" "$dependency"
        if [[ "$expected" != *" $dependency "* ]]; then
            echo "Unexpected dependency: $dependency" >&2
            exit 1
        fi
    done
}

sudo apt-get update -qq
sudo apt-get install -y -qq build-essential cmake curl ca-certificates
[[ "$TARGET" == "arm" ]] && sudo apt-get install -y -qq gcc-arm-linux-gnueabihf

# Fetch the pinned source from upstream GitLab.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://gitlab.freedesktop.org/freetype/freetype/-/archive/$UPSTREAM_TAG/freetype-$UPSTREAM_TAG.tar.gz" | tar xz

# Build. Internal zlib, optional deps off — zero library dependencies.
cmake --fresh -S "$SRC_DIR" -B "$WORK_DIR/build-$RID" \
    -DCMAKE_C_COMPILER="$CC" \
    -DCMAKE_BUILD_TYPE=Release \
    -DBUILD_SHARED_LIBS=ON \
    -DFT_DISABLE_ZLIB=ON -DFT_DISABLE_BZIP2=ON -DFT_DISABLE_PNG=ON \
    -DFT_DISABLE_HARFBUZZ=ON -DFT_DISABLE_BROTLI=ON
cmake --build "$WORK_DIR/build-$RID" -j

# CMake produces libfreetype.so.6.x.y plus symlinks; ship the resolved real
# file under the name the .NET loader probes for.
cp "$(readlink -f "$WORK_DIR/build-$RID/libfreetype.so")" "$OUT_DIR/libfreetype.so"
strip "$OUT_DIR/libfreetype.so"

# Verify dependencies without ldd, which cannot inspect cross-built armhf
# binaries on an arm64 runner.
verify_needed_libraries "$OUT_DIR/libfreetype.so" libc.so.6 libm.so.6
echo "OK $OUT_DIR/libfreetype.so"
