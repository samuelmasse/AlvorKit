#!/usr/bin/env bash
# Builds the GLFW osx-arm64 binary for AlvorKit.GLFW.Native, at the version
# pinned in native/glfw/TAG. Designed for an Apple Silicon Mac with the Xcode
# Command Line Tools and CMake.
# Output: native/glfw/runtimes/osx-<arch>/native/libglfw3.dylib
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARCH="${1:-arm64}"  # arm64 | x86_64
RID="osx-x64"; [ "$ARCH" = "arm64" ] && RID="osx-arm64"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/glfw-build"
SRC_DIR="$WORK_DIR/glfw-$VERSION"
OUT_DIR="$SCRIPT_DIR/../runtimes/$RID/native"

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/glfw/glfw/archive/refs/tags/$VERSION.tar.gz" | tar xz

# Build. Shared library, no examples/tests/docs; macOS 11.0 = first arm64 release.
cmake --fresh -S "$SRC_DIR" -B "$WORK_DIR/build-osx-$ARCH" \
    -DCMAKE_BUILD_TYPE=Release \
    -DBUILD_SHARED_LIBS=ON \
    -DCMAKE_OSX_ARCHITECTURES="$ARCH" \
    -DCMAKE_OSX_DEPLOYMENT_TARGET=11.0 \
    -DGLFW_BUILD_EXAMPLES=OFF -DGLFW_BUILD_TESTS=OFF -DGLFW_BUILD_DOCS=OFF
cmake --build "$WORK_DIR/build-osx-$ARCH" -j

# CMake produces a versioned dylib plus symlinks; ship the resolved real file
# under the name the .NET loader probes for (nativeLibrary = glfw3).
cp "$(cd "$WORK_DIR/build-osx-$ARCH/src" && readlink -f libglfw.dylib)" "$OUT_DIR/libglfw3.dylib"
strip -x "$OUT_DIR/libglfw3.dylib"

# Verify: arm64, Apple system libraries/frameworks only expected.
file "$OUT_DIR/libglfw3.dylib" | grep -q "$ARCH"
otool -L "$OUT_DIR/libglfw3.dylib"
echo "OK $OUT_DIR/libglfw3.dylib"
