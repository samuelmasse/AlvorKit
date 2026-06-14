#!/usr/bin/env bash
# Builds the GLFW linux-x64 binary for AlvorKit.GLFW.Native, at the version
# pinned in native/glfw/TAG. Designed for Ubuntu 24.04.
# Output: native/glfw/runtimes/linux-<arch>/native/libglfw3.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/glfw-build"
SRC_DIR="$WORK_DIR/glfw-$VERSION"
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
        if [[ "$dependency" == ld-linux*.so.* ]]; then
            continue
        fi

        if [[ "$expected" != *" $dependency "* ]]; then
            echo "Unexpected dependency: $dependency" >&2
            exit 1
        fi
    done
}

# Toolchain plus X11 dev headers. GLFW is built X11-only (Wayland off) to keep
# the dependency set small, matching the other native libraries.
if [[ "$TARGET" == "arm" ]]; then
    # 32-bit ARM cross build: armhf toolchain plus armhf X11 dev libraries.
    sudo dpkg --add-architecture armhf
    sudo apt-get update -qq
    sudo apt-get install -y -qq build-essential cmake curl ca-certificates gcc-arm-linux-gnueabihf \
        libx11-dev:armhf libxrandr-dev:armhf libxinerama-dev:armhf libxcursor-dev:armhf libxi-dev:armhf
else
    sudo apt-get update -qq
    sudo apt-get install -y -qq build-essential cmake curl ca-certificates \
        libx11-dev libxrandr-dev libxinerama-dev libxcursor-dev libxi-dev
fi

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/glfw/glfw/archive/refs/tags/$VERSION.tar.gz" | tar xz

# Build. Shared library, X11 only, no examples/tests/docs.
cmake --fresh -S "$SRC_DIR" -B "$WORK_DIR/build-$RID" \
    -DCMAKE_C_COMPILER="$CC" \
    -DCMAKE_BUILD_TYPE=Release \
    -DBUILD_SHARED_LIBS=ON \
    -DGLFW_BUILD_EXAMPLES=OFF -DGLFW_BUILD_TESTS=OFF -DGLFW_BUILD_DOCS=OFF \
    -DGLFW_BUILD_WAYLAND=OFF -DGLFW_BUILD_X11=ON
cmake --build "$WORK_DIR/build-$RID" -j

# CMake produces libglfw.so.3.x plus symlinks; ship the resolved real file under
# the name the .NET loader probes for (nativeLibrary = glfw3).
cp "$(readlink -f "$WORK_DIR/build-$RID/src/libglfw.so")" "$OUT_DIR/libglfw3.so"
strip "$OUT_DIR/libglfw3.so"

# Verify dependencies without ldd, which cannot inspect cross-built armhf
# binaries on an arm64 runner. The allow-list is a superset of the X11 and
# system libraries GLFW may pull in: which it links directly versus dlopens
# varies by distro, and listing extras never causes a false failure.
verify_needed_libraries "$OUT_DIR/libglfw3.so" \
    libc.so.6 libm.so.6 libpthread.so.0 libdl.so.2 librt.so.1 \
    libX11.so.6 libXrandr.so.2 libXinerama.so.1 libXcursor.so.1 libXi.so.6 \
    libXext.so.6 libxcb.so.1 libXau.so.6 libXdmcp.so.6
echo "OK $OUT_DIR/libglfw3.so"
