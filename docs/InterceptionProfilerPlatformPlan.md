# Interception Profiler Platform Plan

## Decision

The interception profiler currently supports **Windows x64, Linux x64, and
Linux Arm64**. This is a runtime-support boundary, not merely the platforms
used by local proofs. The repository keeps explicit build configuration,
package assets, launcher paths, artifact verification, and executable evidence
for `win-x64`, `linux-x64`, and `linux-arm64`.

The public C ABI remains fixed-width and mechanically layout-checked because it
is the source for generated bindings. Those checks do not constitute native
runtime support for another RID.

Support expands one RID at a time. A target is added only together with
its build configuration, generated-binding checks, artifact verification, and
an executable profiled-process test. Do not keep speculative platform branches
or checked-in binaries ahead of those gates.

## Current milestone: reproducible x64 and Arm64 CI

The native package pipeline runs the complete platform-specific portion on
Windows x64, Linux x64, and Linux Arm64:

1. Check out the `dotnet/runtime` tag pinned by
   `native/interception-profiler/version/CORECLR_TAG` and validate the required
   CoreCLR and PAL headers.
2. Configure and build the profiler as `win-x64`, `linux-x64`, and
   `linux-arm64` with the repository native build command.
3. Verify PE and ELF machine types, dependency allowlists, exact public
   exports, and reported ABI version.
4. Generate the managed bindings from the public C header in strict mode and
   fail when generated output differs from the expected projects.
5. Build the generated API and backend projects against the new native asset.
6. Run an isolated profiler-load check for every RID, prove that a following
   ordinary child inherits no activation, then consume the packed native
   package in a ReJIT test that installs a patch, executes the replacement,
   reverts it, and executes the original body.
7. Pack all three runtime assets into one native package and upload the native
   and verification artifacts. A main-branch native version-marker push
   publishes only after the same build and verification gates pass.

The milestone is complete only when a clean runner can produce the asset and
bindings without relying on a developer-machine header cache or checked-in
binary.

The native half of this milestone is implemented by
`.github/workflows/interception-profiler-native.yml`. Once that package is
published, `native/interception-profiler/version/BINDING_REVISION` activates
the API and backend release through `.github/workflows/c-header-bindings.yml`.

## Adding another RID

For each new target, add and pass all of these gates in the same change:

- explicit CMake target definitions rather than host-architecture inference;
- pinned CoreCLR source acquisition;
- target-native dependency and export verification;
- strict binding generation and generated-project builds;
- profiler ABI negotiation; and
- an end-to-end ReJIT and revert smoke test on a supported native .NET host.

Implemented Linux prerequisites include local COM/profiler GUID definitions,
hidden-by-default ELF visibility with an exact export set, undefined-symbol
link failure, and UTF-8 environment decoding.

Known prerequisites for later RIDs remain intentionally deferred:

- Windows x86 needs resolver-pointer bit preservation and an x86 .NET host;
- Windows Arm64 needs its native compiler toolset and an Arm64 runtime proof;
- Linux ARM32 needs real ARM32 execution hardware because QEMU is not a
  supported .NET runtime test environment; and
- macOS packaging must account for both architectures and eventual
  signing/library-validation requirements.

Until a target completes these gates, it is unsupported rather than
build-only, experimental, or implied by generic repository native tooling.
