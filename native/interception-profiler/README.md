# AlvorKit Interception Profiler

This package is AlvorKit's native CoreCLR ReJIT backend. It does not decide
which injector scope receives a patch and it does not host a network endpoint.
Its job is deliberately small:

- catalog allowed managed modules by MVID, method-definition token, and exact
  signature hash;
- accept bounded install, replace, and remove commands through a versioned C
  ABI;
- request ReJIT for the selected method and its existing inliners;
- publish callback counts, HRESULTs, elapsed time, and terminal state; and
- restore the original IL when the final owner removes a patch.

The current supported runtime target is **Windows x64 only**. Build
configuration, checked-in runtime assets, launch guards, and executable proofs
must not imply support for another RID until that RID has its own build and
runtime gates. The staged expansion decision is recorded in
[`docs/InterceptionProfilerPlatformPlan.md`](https://github.com/AlvorKit/AlvorKit/blob/main/docs/InterceptionProfilerPlatformPlan.md).

ABI v3 retains raw IL and exact managed dispatch and adds immutable method
generations. A generation carries its prior-generation ID, the authoritative
loaded-body SHA-256 identity, a bounded private body copy, exact ECMA signature
blobs, bounded metadata relocations, and an original-to-instrumented IL map.
Native code emits or reuses StandAloneSig, TypeSpec, MemberRef, and MethodSpec
tokens only for the loaded module epoch, patches the private body, and publishes
structured generation and relocation results.

Exact dispatch still uses the target's real receiver, arguments, `ref`/`out`
locations, and return type. There is no `DispatchInt32`-style matrix and no
dispatcher specialized per return type.

## Source architecture

`InterceptionProfiler` is the thin COM and exported-ABI adapter.
`ProfilerRuntime` owns lifecycle and the bounded worker queue. The runtime
delegates module epochs, method discovery, allowlisting, and metadata tokens to
`ModuleCatalog`; patch and completion transitions remain atomic inside
`RejitState`.

Signature traversal, method-body parsing, exact-dispatch emission, generation
validation, and body identity are independent collaborators. Exact-dispatch
bodies and IL maps are prepared on the profiler worker and published as an
immutable `PreparedRejit` snapshot. `GetReJITParameters` only acquires that
snapshot, copies it into runtime-owned memory, applies its map and flags, and
reports status.

Hand-authored native implementation and internal header files follow the
repository's 250-line Commit Mode target. The public ABI header is the reviewed
exception because it intentionally keeps the complete versioned contract in
one place.

The current ABI-v3 milestone has deliberately bounded proof coverage:

- Windows x64 has executable post-JIT coverage for all four relocation kinds.
  One proof creates a previously absent Cdecl `int(int)` StandAloneSig. A
  second generation creates a closed generic TypeSpec, a custom-modifier-bearing
  private MemberRef, an internal MemberRef, and a closed generic MethodSpec in
  one already JITted module. Both proofs execute the relocated bodies, reuse
  the exact tokens on replacement, submit IL maps, and restore the baseline.
- Loaded-body identity rejection is executable. `VAR`/`MVAR` signatures,
  cross-module access, constructor/field MemberRefs, and collectible unload do
  not yet have equivalent profiler coverage.
- Managed symbolic caller plans do not yet lower their canonical relocation
  descriptions into the exact body and metadata blobs accepted by ABI v3.
- Target ReJITID evidence is reported, but exhaustive inliner correlation,
  compensating generation rollback, and future tier/code-version
  reconciliation remain incomplete.

## Startup

CoreCLR must load the profiler before managed startup:

```text
CORECLR_ENABLE_PROFILING=1
CORECLR_PROFILER={3840ACF7-5AF1-49EA-BF94-5F7086C57F57}
CORECLR_PROFILER_PATH_64=<absolute native profiler path>
ALVORKIT_INTERCEPTION_PROFILER_PATH=<the same absolute path>
ALVORKIT_INTERCEPTION_MODULES=MyGame.Dev;MyGame.Game
```

`ALVORKIT_INTERCEPTION_MODULES` is an explicit semicolon-separated module
allowlist. `*` is accepted for isolated proof processes, but development games
should list their patchable assemblies.

The managed binding opens the already loaded native library through
`ALVORKIT_INTERCEPTION_PROFILER_PATH`; it does not attach a profiler to an
arbitrary running process.

ReJIT requires `COR_PRF_DISABLE_ALL_NGEN_IMAGES`, so a profiler-enabled launch
does more cold JIT work instead of consuming ReadyToRun images. The process is
still an optimized Release process. Unprofiled launches do not load this
library and are unaffected.

## Build and package

The pinned CoreCLR tag is recorded in `version/CORECLR_TAG`. Point the build at
a license-reviewed checkout of that tag:

```powershell
$env:ALVORKIT_CORECLR_SOURCE = "<dotnet-runtime v10.0.9 checkout>"

dotnet run --project scripts\AlvorKit.Script.NativeBuild -- `
  build interception-profiler --rid win-x64

dotnet build `
  native\interception-profiler\AlvorKit.Interception.Profiler.Native.csproj `
  -c Release
```

The CMake project deliberately rejects every target except `win-x64`. Other
platform work resumes only after the Windows x64 CI build, binding-generation,
artifact-verification, and runtime-proof gates in the platform plan are
established.

The `interception profiler native package` workflow checks out the pinned
CoreCLR headers, builds the DLL, verifies its PE architecture, exact export
set, dependency allowlist, and ABI version, then packs and uploads the native
NuGet package. Pull requests stop at artifacts, and manual runs do so by
default. A main-branch native version-marker push publishes the package.
Manual runs publish only when the workflow's publish input is explicitly
enabled.

The public header
`include/alvorkit_interception_profiler.h` is the sole ABI source. Bindings are
generated with:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- `
  interception-profiler --setup-local --strict
```

The strict check currently validates 11 exported functions, 6 enums, and 14
natural-layout structs. Cross-target layout checks protect the fixed-width C
contract; they do not indicate that a native runtime asset is supported.
