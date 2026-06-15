# AlvorKit

AlvorKit is a C# game development kit

## Binding development mode

Projects use published binding packages by default. To develop against generated bindings in
`out/bindgen`, add an ignored `AlvorKit.Local.props` at the repository root:

```xml
<Project>
    <PropertyGroup>
        <UseLocalBindings>true</UseLocalBindings>
    </PropertyGroup>
</Project>
```

Build and restore intentionally do not run bindgen automatically. Generate the
local project you are changing explicitly with:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library>
```

If `UseLocalBindings=true` and `out/bindgen` is missing, builds fail until the
generated project is created with that command. Use `all` only when bootstrapping
every binding project or making a change that intentionally affects them all.

To compare generator changes without overwriting the default local bindings,
write snapshots under `out/` and diff them:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- xxhash --output-root out\bindgen-review\xxhash-before
dotnet run --project scripts\AlvorKit.Script.Bindgen -- xxhash --output-root out\bindgen-review\xxhash-after
git diff --no-index -- out\bindgen-review\xxhash-before out\bindgen-review\xxhash-after
```

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
Invoke-Item .\out\coverage\html\index.html
```

Raw per-project coverage files and logs are written under `out/coverage/projects/<test-project>/`.
The strict coverage gate is the same command without `--threshold 0`; it fails
unless line, branch, and method coverage are all 100%.

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
`native/<lib>/conf/native-build.json`.
