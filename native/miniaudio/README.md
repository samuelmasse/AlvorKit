# AlvorKit.MiniAudio.Native

miniaudio binaries for win (x64, x86, arm64), linux (x64, arm, arm64) and osx (x64, arm64)

The package version is the upstream [miniaudio](https://github.com/mackron/miniaudio) tag the binaries were built from, compiled with `MA_DLL`. A 4th version segment (x.y.z.N) is an AlvorKit packaging revision: same upstream release, rebuilt packaging.

## Ogg/Vorbis streaming

Native revision `0.11.25.13` bundles libogg 1.3.6 and libvorbis 1.3.7 as static
dependencies. No system codec installation or additional codec DLL is required.
The pinned source archives and SHA256 checks are in `CMakeLists.txt`.
Distributions must include `THIRD-PARTY-NOTICES.txt` from the package.

Binding revision `0.11.25.15` exposes two configuration initializers:

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

Build with the repository native-build runner, then package the generated
runtime binaries through `AlvorKit.MiniAudio.Native.csproj`. CMake uses the same
recipe on Windows, Linux, and macOS. Local verification of this revision covers
Windows x64; other runtime identifiers require their platform builds and checks.

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
