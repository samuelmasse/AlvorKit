# Interception Profiler Platform Plan

## Decision

The interception profiler currently supports **Windows x64 only**. This is a
runtime-support boundary, not merely the platform used by the latest local
proof. The repository keeps only the `win-x64` profiler build configuration,
runtime asset, launcher path, and executable evidence.

The public C ABI remains fixed-width and mechanically layout-checked because it
is the source for generated bindings. Those checks do not constitute native
runtime support for another RID.

Support expands one RID at a time. A future target is added only together with
its build configuration, generated-binding checks, artifact verification, and
an executable profiled-process test. Do not keep speculative platform branches
or checked-in binaries ahead of those gates.

## First milestone: reproducible Windows x64 CI

The next profiler milestone establishes the complete pipeline on a Windows x64
runner before adding another target:

1. Check out the `dotnet/runtime` tag pinned by
   `native/interception-profiler/version/CORECLR_TAG` and validate the required
   CoreCLR headers.
2. Configure and build the profiler as `win-x64` with the repository native
   build command.
3. Verify the PE machine type, dependency allowlist, exact public exports, and
   reported ABI version.
4. Generate the managed bindings from the public C header in strict mode and
   fail when generated output differs from the expected projects.
5. Build the generated API and backend projects against the new native asset.
6. Run an isolated Windows x64 profiled process that installs a patch, executes
   the replacement, reverts it, executes the original body, and shuts down
   cleanly.
7. Upload the native and verification artifacts. A main-branch native
   version-marker push publishes the package only after the same build and
   verification gates pass.

The milestone is complete only when a clean runner can produce the asset and
bindings without relying on a developer-machine header cache or checked-in
binary.

The native half of this milestone is implemented by
`.github/workflows/interception-profiler-native.yml`. Binding activation
remains separate so the native package can be published before the generated
backend attempts to restore it.

## Adding another RID

For each new target, add and pass all of these gates in the same change:

- explicit CMake target definitions rather than host-architecture inference;
- pinned CoreCLR source acquisition;
- target-native dependency and export verification;
- strict binding generation and generated-project builds;
- profiler ABI negotiation; and
- an end-to-end ReJIT and revert smoke test on a supported native .NET host.

Known prerequisites remain intentionally deferred:

- Windows x86 needs resolver-pointer bit preservation and an x86 .NET host;
- Windows Arm64 needs its native compiler toolset and an Arm64 runtime proof;
- Unix needs local COM/profiler GUID definitions, controlled symbol visibility,
  undefined-symbol link failures, and correct UTF-8 environment decoding;
- Linux ARM32 needs real ARM32 execution hardware because QEMU is not a
  supported .NET runtime test environment; and
- macOS packaging must account for both architectures and eventual
  signing/library-validation requirements.

Until a target completes these gates, it is unsupported rather than
build-only, experimental, or implied by generic repository native tooling.
