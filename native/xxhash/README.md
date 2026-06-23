# AlvorKit.XxHash.Native

xxHash binaries for win (x64, x86, arm64), linux (x64, arm, arm64) and osx (x64, arm64)

The package version is the upstream [xxHash](https://github.com/Cyan4973/xxHash) tag the binaries were built from,
compiled with `XXH_STATIC_LINKING_ONLY` so the advanced API (`XXH3_*_withSecretandSeed`, secret generation) is exported
too. A 4th version segment (x.y.z.N) is an AlvorKit packaging revision: same upstream release, rebuilt packaging.

| RID         | Binary            |
| ----------- | ----------------- |
| win-x64     | `xxhash.dll`      |
| win-x86     | `xxhash.dll`      |
| win-arm64   | `xxhash.dll`      |
| linux-x64   | `libxxhash.so`    |
| linux-arm   | `libxxhash.so`    |
| linux-arm64 | `libxxhash.so`    |
| osx-x64     | `libxxhash.dylib` |
| osx-arm64   | `libxxhash.dylib` |

## Verification

The native package workflow runs `dotnet run --project scripts/AlvorKit.Script.NativeBuild -- verify xxhash --rid <rid>`
immediately after each xxhash runtime build. That gate verifies the exported runtime for the RID before the package artifact
is accepted.

Verification reports are uploaded from `out/native-verify/xxhash/<rid>/` as workflow artifacts named
`xxhash-verify-<rid>`. The `linux-arm` workflow cell builds on the Ubuntu ARM64 runner and installs `qemu-user` so the
armhf verifier executable can run under emulation.
