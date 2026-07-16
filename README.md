# AlvorKit

AlvorKit is a C# game development kit

## Ent-component system

AlvorKit games use the AlvorKit ECS for game Ents. Start with the
[AlvorKit ECS guide](docs/ECS.md) for generated components, Ent and arena
ownership, and the Indexed context, hook, bag, iteration, and teardown pattern
used by AlvorKit game repositories.

## Binding development mode

Projects use published binding packages by default. When an exact generated
project exists under `out/bindgen`, consumers automatically use that local
project instead of the pinned package for that binding. Build and restore
intentionally do not run bindgen automatically.

The default bindgen output root is `out/generated/bindgen`, which is safe for
inspection because it does not activate local project references. Generate into
the active local project root only when you want consumers to use the generated
project:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library> --setup-local
```

If the generated project is missing, consumers fall back to the pinned package
version in `AlvorKit.Packages.props`. Use `all` only when bootstrapping every
binding project or making a change that intentionally affects them all.

To compare generator changes without activating local bindings, write snapshots
under `out/` and diff them:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- xxhash --output-root out\bindgen-review\xxhash-before
dotnet run --project scripts\AlvorKit.Script.Bindgen -- xxhash --output-root out\bindgen-review\xxhash-after
git diff --no-index -- out\bindgen-review\xxhash-before out\bindgen-review\xxhash-after
```

## Maths package development mode

Projects use the published `AlvorKit.Maths.Primitives` package when
`out/mathgen/AlvorKit.Maths.Primitives` is missing. The default MathsGen output
root is `out/generated/mathgen`, which is safe for inspection because it does
not activate local project references. Generate the active local primitives
project only when changing the maths generator or generated primitive surface:

```powershell
dotnet run --project scripts\AlvorKit.Script.MathsGen -- --setup-local
```

The user-facing package is `AlvorKit.Maths`; it is a facade that brings in
`AlvorKit.Maths.Core` and `AlvorKit.Maths.Primitives`. Maths package releases are
triggered by changing `src/AlvorKit.Maths/version/VERSION` on `main`. Manual runs
of the maths package workflow build and upload `.nupkg` artifacts without
publishing.

## OpenGL maths overloads

`AlvorKit.OpenGL.Maths` provides allocation-free vector, matrix, interval,
extent, and typed vertex overloads for the generated OpenGL API. It works with
raw `Gl` and is included transitively by `AlvorKit.OpenGL.Layer`. See the
[OpenGL maths guide](docs/OpenGLMaths.md) for package use, examples, validation
rules, and raw-API escape hatches.

## Linting

Run the repository linter from the repository root with scoped includes while iterating:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint -- --include "scripts/**/*.cs"
```

Repeat `--include` for multiple files, directories, or globs. Use `--fix` with the same scope
to apply supported formatter fixes locally:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint -- --fix --include "AGENTS.md"
```

Run the full repository linter for broad changes or CI parity checks:

```powershell
dotnet run --project scripts\AlvorKit.Script.Lint
```

The linter coordinates C# formatting, Prettier checks, EditorConfig checks, and
GitHub Actions workflow validation.

## Unit test coverage

Run focused coverage for a source project while iterating:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --source-project AlvorKit.Script.NativeBuild --threshold 0
```

The tool runs test projects that reference the selected source project and gates
coverage only on that source project. To choose tests explicitly, combine
`--source-project` with `--test-project`:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --source-project AlvorKit.Script.NativeBuild --test-project AlvorKit.Script.NativeBuild.Test --threshold 0
```

Run the full coverage report for broad changes or CI parity checks:

```powershell
dotnet run --project scripts\AlvorKit.Script.TestCoverage -- --threshold 0
```

Open the ReportGenerator HTML report:

```powershell
$artifacts = Get-Content .\out\coverage\latest-run.json | ConvertFrom-Json | Select-Object -ExpandProperty artifacts
Invoke-Item $artifacts.html
```

Each run writes isolated artifacts under `out/coverage/runs/<run-id>/` by default,
and the console output prints the exact agent, human, and HTML report paths.
Use `--run-id <name>` for a stable run directory, or `--output-root <path>` to
place isolated runs under another parent such as `out/coverage/agents/<agent-id>/`.
`out/coverage/latest-run.json` is a convenience pointer to the most recent run,
not a concurrency-safe artifact. The strict coverage gate is the same command
without `--threshold 0`; it fails unless line, branch, and method coverage are
all 100%.

## Native package builds

Native packages are built by the shared native build runner. To see the configured native packages:

```powershell
dotnet run --project scripts\AlvorKit.Script.NativeBuild -- list
```

Build one RID with:

```powershell
dotnet run --project scripts\AlvorKit.Script.NativeBuild -- build xxhash --rid win-x64
```

Then pack the native project:

```powershell
dotnet build -c Release native\xxhash\AlvorKit.XxHash.Native.csproj
```

Native package revisions live in `native/<lib>/version/REVISION`; generated binding package
revisions live in `native/<lib>/version/BINDING_REVISION`. Native build settings live in
`native/<lib>/conf/native-build.yml`.
