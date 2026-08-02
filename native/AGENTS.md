# Native Source Instructions

## Scope

These instructions apply to first-party native code under `native/`. They are a
delta over the repository root instructions. Generated, vendored, extracted,
and packaged runtime files are not hand-authored native source.

Read [`../docs/AgentRules/RuntimePerformance.md`](../docs/AgentRules/RuntimePerformance.md)
when changing runtime callbacks, allocation, ownership, or native-boundary
behavior. The root **NATIVE-BUILD-001** authorization gate remains in force.

## Source Shape

- In Commit Mode, keep each edited hand-authored `.c`, `.cc`, `.cpp`, `.h`, and
  `.hpp` file at or below 250 lines.
- Native verification and test files may be up to 750 lines when related cases
  read better together.
- A reviewed public ABI header may exceed 250 lines when keeping one complete,
  versioned contract together is materially easier to audit. Call out the
  exception and do not use it to host implementation details.
- Keep hand-authored native and CMake lines at or below 170 characters.
- Split source by cohesive ownership and behavior. Do not distribute one broad
  class across translation units solely to satisfy the file-size target.
- Prefer one primary class per header/source pair. Keep small value types beside
  their owning domain instead of creating generic `Types` or `Helpers` files.
- Use RAII for native resources, COM interfaces, locks, and threads. Make
  lifetime and ownership explicit where a resource cannot use ordinary RAII.
- Document public ABI declarations and non-obvious concurrency, ownership,
  callback, and platform contracts. Routine private mechanics do not require
  comments that merely repeat the code.

## Clang Tooling

- Use `native/.clang-format` for hand-authored C and C++ source. Do not reformat
  generated, vendored, extracted, or packaged runtime code.
- Refresh `out/clangd/compile_commands.json` with the VS Code
  `clangd: refresh compile database` task after changing native source layout,
  the pinned CoreCLR tree, or the active Visual Studio/Windows SDK toolchain.
- Keep the compile database under `out/`; it contains machine-specific absolute
  paths and must not be committed.

## Profiler And Runtime Callbacks

- Keep CLR callbacks bounded. Do not parse metadata, construct replacement IL,
  format diagnostics, or perform avoidable heap allocation in a callback when
  that work can be prepared on the profiler worker.
- Publish immutable prepared callback payloads. A callback may acquire a stable
  snapshot, copy it into runtime-owned memory, and report state without holding
  a mutable state lock across CoreCLR calls.
- Give each mutex one documented ownership domain. Keep patch and completion
  transitions atomic, define lock ordering where domains must interact, and do
  not call arbitrary managed code from a profiler callback.
- No C++ exception may cross a COM callback or exported C ABI boundary.
- Keep the exported C header versioned, fixed-width, and free of C++ types. The
  public header remains the source of truth for generated bindings.

## Verification

- Prefer direct native tests for signature parsing, method-body parsing and
  emission, request validation, hashing, and state transitions.
- Use isolated profiled-process tests for CLR loading, ReJIT callbacks,
  metadata emission, inliner repair, revert, unload, and shutdown behavior.
- Native package builds and dependency installation remain governed by the root
  authorization rule. Do not run them without an explicit user request and
  permission for that run.
