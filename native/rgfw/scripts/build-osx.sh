#!/usr/bin/env bash
# Builds the RGFW osx-arm64 binary for AlvorKit.RGFW.Native, at the tag pinned
# in native/rgfw/TAG. Designed for an Apple Silicon Mac with the Xcode Command
# Line Tools — no CMake, no Homebrew.
# Output: native/rgfw/runtimes/osx-<arch>/native/libRGFW.dylib
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARCH="${1:-arm64}"  # arm64 | x86_64
RID="osx-x64"; [ "$ARCH" = "arm64" ] && RID="osx-arm64"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/rgfw-build"
SRC_DIR="$WORK_DIR/RGFW-$VERSION"
OUT_DIR="$SCRIPT_DIR/../runtimes/$RID/native"

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/ColleagueRiley/RGFW/archive/refs/tags/$VERSION.tar.gz" | tar xz

# Build. Frameworks per upstream's Makefile; macOS 11.0 = first arm64 release.
clang -dynamiclib -O2 -arch "$ARCH" -mmacosx-version-min=11.0 \
    -install_name @rpath/libRGFW.dylib -I "$SRC_DIR" \
    -o "$OUT_DIR/libRGFW.dylib" "$SCRIPT_DIR/../rgfw.c" \
    -framework CoreVideo -framework Cocoa -framework OpenGL -framework IOKit
strip -x "$OUT_DIR/libRGFW.dylib"

# Verify: arm64, Apple system libraries/frameworks only expected.
file "$OUT_DIR/libRGFW.dylib" | grep -q "$ARCH"
otool -L "$OUT_DIR/libRGFW.dylib"
echo "OK $OUT_DIR/libRGFW.dylib"
