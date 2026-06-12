#!/usr/bin/env bash
# Builds the xxHash linux-x64 binary for AlvorKit.XxHash.Native, at the tag
# pinned in native/xxhash/TAG. Designed for Ubuntu 24.04. xxHash is pure
# computation, so no extra dev headers are needed.
# Output: native/xxhash/runtimes/linux-<arch>/native/libxxhash.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERSION="$(tr -d '[:space:]' < "$SCRIPT_DIR/../TAG")"
WORK_DIR="$HOME/xxhash-build"
SRC_DIR="$WORK_DIR/xxHash-$VERSION"
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

sudo apt-get update -qq
sudo apt-get install -y -qq build-essential curl ca-certificates
[[ "$TARGET" == "arm" ]] && sudo apt-get install -y -qq gcc-arm-linux-gnueabihf

# Fetch the pinned source.
mkdir -p "$WORK_DIR" "$OUT_DIR"
cd "$WORK_DIR"
[[ -d "$SRC_DIR" ]] || curl -fsSL "https://github.com/Cyan4973/xxHash/archive/refs/tags/v$VERSION.tar.gz" | tar xz

# Build. --no-undefined turns underlinking into a build error instead of a
# load-time failure.
"$CC" -shared -fPIC -O2 -Wl,--no-undefined -I "$SRC_DIR" \
    -o "$OUT_DIR/libxxhash.so" "$SCRIPT_DIR/../xxhash.c"
strip "$OUT_DIR/libxxhash.so"

# Verify dependencies without ldd, which cannot inspect cross-built armhf
# binaries on an arm64 runner.
verify_needed_libraries "$OUT_DIR/libxxhash.so" libc.so.6
echo "OK $OUT_DIR/libxxhash.so"
