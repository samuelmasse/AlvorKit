#!/usr/bin/env bash
# Builds the xxHash osx-arm64 binary for AlvorKit.XxHash.Native, at the tag
# pinned in native/xxhash/TAG. Designed for an Apple Silicon Mac with the
# Xcode Command Line Tools. xxHash is pure computation, so no frameworks are
# linked.
# Output: native/xxhash/runtimes/osx-<arch>/native/libxxhash.dylib
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARCH="${1:-arm64}"  # arm64 | x86_64
RID="osx-x64"; [ "$ARCH" = "arm64" ] && RID="osx-arm64"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/xxhash-build"
SRC_DIR="$WORK_DIR/xxHash-$VERSION"
OUT_DIR="$SCRIPT_DIR/../runtimes/$RID/native"

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/Cyan4973/xxHash/archive/refs/tags/v$VERSION.tar.gz" | tar xz

# Build. macOS 11.0 = first arm64 release.
clang -dynamiclib -O2 -arch "$ARCH" -mmacosx-version-min=11.0 \
    -install_name @rpath/libxxhash.dylib -I "$SRC_DIR" \
    -o "$OUT_DIR/libxxhash.dylib" "$SCRIPT_DIR/../xxhash.c"
strip -x "$OUT_DIR/libxxhash.dylib"

# Verify: arm64, Apple system libraries only expected.
file "$OUT_DIR/libxxhash.dylib" | grep -q "$ARCH"
otool -L "$OUT_DIR/libxxhash.dylib"
echo "OK $OUT_DIR/libxxhash.dylib"
