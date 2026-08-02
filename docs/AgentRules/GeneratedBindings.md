# Generated Bindings And Templates Policy

## Scope

Read this policy before changing a code generator, repository template,
generator configuration, generated binding source or project, generated binding
documentation, native test double, or generated package-version property.

## Hard Stop

Native package builds remain governed by **NATIVE-BUILD-001**. Generated-output
review does not authorize a native build or native dependency installation.

## Generated Output Checks

Read [GeneratedOutputChecks.md](../GeneratedOutputChecks.md) before
changing a code generator, generator configuration, generated binding output, or
generated binding documentation, and whenever the user asks for generated-output
checks.

When changing a code generator or generator configuration, capture generated
output before and after the change in Commit Mode or when the user asks for
generated-output checks. In Working Mode, do this only when useful. Regenerate
only the binding library whose inputs, configuration, or source project changed;
use `all` only when the change intentionally affects every generated binding
project, and say why in the handoff.

Do not embed generated source, project files, scripts, or other multi-line
output directly inside C# string literals. Put emitted text in a template under
`res/templates/` and render it with the repository template helper.

When doing generated-output checks, read the generated source and project-file
diff carefully, use focused fixtures when full binding output is too large, and
summarize meaningful generated-code changes before handoff. Delete disposable
`out/bindgen-review/` snapshots before a Commit Mode handoff unless the user
asks to keep them; in Working Mode, list skipped generated-output checks or
cleanup.

Do not wire bindgen into normal restore or build targets. Run bindgen for the
changed library, then build. The bindgen default writes to non-active
`out/generated/bindgen`; pass `--setup-local` only when the task intentionally
needs consumers to use local generated binding projects. Consumers use the exact
local generated project under `out/bindgen` when it exists and otherwise use the
pinned package. Do not add `LOCAL_BINDINGS` or any other compile-time symbol to
distinguish local generated bindings from packaged bindings.

Native package builds are intended for CI. Agents must never run
`scripts/AlvorKit.Script.NativeBuild`, invoke native runtime package builds, or
install native build dependencies on a developer machine unless the user
explicitly asks for that work and grants permission for that run.

## Generated Native Test Doubles

Generated native binding API projects emit an abstract API class plus
`<ApiClass>Noop` and a forwarding `<ApiClass>Wrapper`. For tests that need a
native library double, subclass the generated noop and override only observed or
construction-needed calls; use the wrapper when most calls should forward to a
real backend. Keep native-library test doubles in tests. Do not add alternate
runtime constructors, ownership flags, or native-free product special cases just
to avoid native calls in tests.

## Package Version Properties

Keep version properties in `AlvorKit.Packages.props` limited to generated
binding packages, generated-package roots, and similarly pinned generated
inputs. Ordinary hand-authored project dependencies, including script utilities
and runtime helper packages, should declare package versions directly in the
project file unless there is a clear non-generated repo-wide reason to
centralize them.
