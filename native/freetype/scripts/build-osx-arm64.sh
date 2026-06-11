#!/usr/bin/env bash
# Builds the FreeType osx-arm64 binary for AlvorKit.FreeType.Native, at the
# version pinned in native/freetype/TAG (upstream tag VER-x-y-z). Designed
# for an Apple Silicon Mac with the Xcode Command Line Tools and CMake.
# Output: native/freetype/runtimes/osx-arm64/native/libfreetype.dylib
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
UPSTREAM_TAG="VER-${VERSION//./-}"
WORK_DIR="$HOME/freetype-build"
SRC_DIR="$WORK_DIR/freetype-$UPSTREAM_TAG"
OUT_DIR="$SCRIPT_DIR/../runtimes/osx-arm64/native"

# Fetch the pinned source from upstream GitLab.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://gitlab.freedesktop.org/freetype/freetype/-/archive/$UPSTREAM_TAG/freetype-$UPSTREAM_TAG.tar.gz" | tar xz

# Build. Internal zlib, optional deps off; macOS 11.0 = first arm64 release.
cmake --fresh -S "$SRC_DIR" -B "$WORK_DIR/build-osx-arm64" \
    -DCMAKE_BUILD_TYPE=Release \
    -DBUILD_SHARED_LIBS=ON \
    -DCMAKE_OSX_ARCHITECTURES=arm64 \
    -DCMAKE_OSX_DEPLOYMENT_TARGET=11.0 \
    -DFT_DISABLE_ZLIB=ON -DFT_DISABLE_BZIP2=ON -DFT_DISABLE_PNG=ON \
    -DFT_DISABLE_HARFBUZZ=ON -DFT_DISABLE_BROTLI=ON
cmake --build "$WORK_DIR/build-osx-arm64" -j

# CMake produces a versioned dylib plus symlinks; ship the resolved real file.
cp "$(cd "$WORK_DIR/build-osx-arm64" && readlink -f libfreetype.dylib)" "$OUT_DIR/libfreetype.dylib"
strip -x "$OUT_DIR/libfreetype.dylib"

# Verify: arm64, Apple system libraries only expected.
file "$OUT_DIR/libfreetype.dylib" | grep -q arm64
otool -L "$OUT_DIR/libfreetype.dylib"
echo "OK $OUT_DIR/libfreetype.dylib"
