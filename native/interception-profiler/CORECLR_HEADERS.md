# CoreCLR profiler header provenance

The profiler builds against the public CoreCLR profiling and PAL headers from
the `dotnet/runtime` tag recorded in `version/CORECLR_TAG` (`v10.0.9`).

Set `ALVORKIT_CORECLR_SOURCE` to a license-reviewed checkout of that exact tag
before invoking `AlvorKit.Script.NativeBuild`. Both builds consume:

- `src/coreclr/inc`
- `src/coreclr/pal/prebuilt/inc`

The Linux x64 build additionally consumes:

- `src/coreclr/pal/inc`
- `src/coreclr/pal/inc/rt`
- `src/native/minipal`

The headers remain governed by the .NET runtime repository's MIT license. They
are not copied into this package; build agents cache the pinned checkout.

## clangd

The repository's VS Code task `clangd: refresh compile database` locates
`clang-cl`, resolves this same pinned CoreCLR tree, and writes
`out/clangd/compile_commands.json` without configuring or building native code.
It runs when the folder opens after automatic tasks are trusted, and it can be
run manually from **Tasks: Run Task**.

If the pinned headers are not already under `out/upstream`, set
`ALVORKIT_CORECLR_SOURCE` before running the task. Restart the clangd language
server after changing that path.
