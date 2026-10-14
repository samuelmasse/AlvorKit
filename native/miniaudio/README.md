# AlvorKit.MiniAudio.Native

miniaudio binaries for win (x64, x86, arm64), linux (x64, arm, arm64) and osx (x64, arm64)

The package version is the upstream [miniaudio](https://github.com/mackron/miniaudio) tag the binaries were built from, compiled with `MA_DLL`. A 4th version segment (x.y.z.N) is an AlvorKit packaging revision: same upstream release, rebuilt packaging.

## Ogg/Vorbis streaming

Native revision `0.11.25.14` bundles libogg 1.3.6 and libvorbis 1.3.7 as static
dependencies. No system codec installation or additional codec DLL is required.
The pinned source archives and SHA256 checks are in `CMakeLists.txt`.
Distributions must include `THIRD-PARTY-NOTICES.txt` from the package.

Binding revision `0.11.25.16` exposes two configuration initializers:

- `Ma.ResourceManagerConfigInitVorbis()` / `ma_resource_manager_config_init_vorbis`
  enables Vorbis in an explicitly owned resource manager. Pass that manager to
  `MaEngineConfig.PResourceManager`, then use `SoundInitFromFile` with
  `SoundFlagStream` to stream compressed files directly.
- `Ma.DecoderConfigInitVorbis(format, channels, sampleRate)` /
  `ma_decoder_config_init_vorbis` enables Vorbis for standalone decoders.

Both preserve the corresponding standard initializer's other defaults. WAV,
FLAC, and MP3 remain available. The decoder table is native-library-owned and
must not be modified or freed. Destroy sounds before the engine, then destroy
the externally owned resource manager after the engine has stopped.

MiniAudio's upstream libvorbis backend supports length queries and sample-based
seeking through its VFS, including resource-manager streams. The bundled
stb_vorbis backend is not enabled: its callback path lacks length queries and
decodes intervening samples when seeking. Neither upstream library is patched.

## Cross-platform bindings and releases

MiniAudio exposes native records whose layouts depend on the operating system
and architecture, including mutexes, semaphores, contexts, devices, and engines.
The API and backend packages contain a managed assembly for every supported RID
under `runtimes/<rid>/lib/net10.0`. NuGet supplies the compile-time reference;
.NET selects the matching implementation at runtime. Common function signatures
are verified across all variants. Platform-specific native record internals remain
platform-specific; consumers should use the public configuration and function APIs.

Run `.github/workflows/miniaudio-release.yml` to build and release MiniAudio.
Changes to its version markers on `main` publish automatically; manual dispatch
builds and verifies artifacts unless `publish` is enabled. MiniAudio is excluded
from the independent native and C-header package workflows.

The release builds every native RID, packs the complete native matrix, generates
and builds every managed ABI, and restores the resulting candidate NuGet packages
on each target. Publication waits for native/managed size and nested field-offset
comparisons and no-device audio checks, including real Vorbis decoding, streaming,
seeking, and looping. Windows x86 runs as x86; Linux ARM32 uses QEMU on ARM64.
Physical audio-device listening remains separate from these automated checks.

The generated `src/miniaudio_abi.inc` contains C `sizeof` and `offsetof`
expressions compiled with real platform headers. Regenerate it with MiniAudio's
bindings before changing the native revision. Strict generation rejects stale
verification tables. Native packaging rejects a missing runtime, and managed
packaging rejects a missing runtime assembly.

| RID         | Binary               |
| ----------- | -------------------- |
| win-x64     | `miniaudio.dll`      |
| win-x86     | `miniaudio.dll`      |
| win-arm64   | `miniaudio.dll`      |
| linux-x64   | `libminiaudio.so`    |
| linux-arm   | `libminiaudio.so`    |
| linux-arm64 | `libminiaudio.so`    |
| osx-x64     | `libminiaudio.dylib` |
| osx-arm64   | `libminiaudio.dylib` |
