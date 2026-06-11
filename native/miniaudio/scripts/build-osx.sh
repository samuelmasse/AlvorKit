#!/usr/bin/env bash
# Builds the miniaudio osx-arm64 binary for AlvorKit.MiniAudio.Native, at the
# tag pinned in native/miniaudio/TAG. Designed for an Apple Silicon Mac with
# the Xcode Command Line Tools. miniaudio runtime-links CoreAudio via dlopen,
# so no frameworks are linked.
# Output: native/miniaudio/runtimes/osx-<arch>/native/libminiaudio.dylib
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARCH="${1:-arm64}"  # arm64 | x86_64
RID="osx-x64"; [ "$ARCH" = "arm64" ] && RID="osx-arm64"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/miniaudio-build"
SRC_DIR="$WORK_DIR/miniaudio-$VERSION"
OUT_DIR="$SCRIPT_DIR/../runtimes/$RID/native"

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/mackron/miniaudio/archive/refs/tags/$VERSION.tar.gz" | tar xz

# Build. macOS 11.0 = first arm64 release.
clang -dynamiclib -O2 -arch "$ARCH" -mmacosx-version-min=11.0 \
    -install_name @rpath/libminiaudio.dylib -I "$SRC_DIR" \
    -o "$OUT_DIR/libminiaudio.dylib" "$SCRIPT_DIR/../miniaudio.c" \
    -lpthread -lm
strip -x "$OUT_DIR/libminiaudio.dylib"

# Verify: arm64, Apple system libraries only expected.
file "$OUT_DIR/libminiaudio.dylib" | grep -q "$ARCH"
otool -L "$OUT_DIR/libminiaudio.dylib"
echo "OK $OUT_DIR/libminiaudio.dylib"
