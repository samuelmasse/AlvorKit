# CoreCLR profiler header provenance

The profiler builds against the public CoreCLR profiling and PAL headers from
the `dotnet/runtime` tag recorded in `version/CORECLR_TAG` (`v10.0.9`).

Set `ALVORKIT_CORECLR_SOURCE` to a license-reviewed checkout of that exact tag
before invoking `AlvorKit.Script.NativeBuild`. All builds consume:

- `src/coreclr/inc`
- `src/coreclr/pal/prebuilt/inc`

The Unix builds (Linux and macOS) additionally consume:

- `src/coreclr/pal/inc`
- `src/coreclr/pal/inc/rt`
- `src/native/minipal`

## Windows checkout

Windows agents must enable Git for Windows long-path handling for this checkout
and use a sparse checkout under `out/upstream`. Do not work around path limits
with an abbreviated sibling checkout outside the repository. From the AlvorKit
root, run:

```powershell
$coreClrTag = Get-Content native\interception-profiler\version\CORECLR_TAG
$coreClrSource = Join-Path (Get-Location) "out\upstream\dotnet-runtime-$coreClrTag"

git -c core.longPaths=true clone `
  --filter=blob:none `
  --depth 1 `
  --branch $coreClrTag `
  --sparse `
  https://github.com/dotnet/runtime.git `
  $coreClrSource

git -C $coreClrSource sparse-checkout set `
  src/coreclr/inc `
  src/coreclr/pal/prebuilt/inc `
  src/coreclr/pal/inc `
  src/native/minipal

$env:ALVORKIT_CORECLR_SOURCE = $coreClrSource
```

The per-command `core.longPaths` setting avoids changing the user's global Git
configuration. The sparse checkout also avoids materializing unrelated runtime
source paths that the profiler build does not consume.

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
