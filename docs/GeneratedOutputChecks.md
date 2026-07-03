# Generated Output Checks

This document holds the generated-output checking workflow, command examples, and
artifact conventions. The root `AGENTS.md` controls when these checks are
required.

## When To Read This

Read this document before changing a code generator, generator configuration,
generated source, generated project files, generated scripts, generated native
binding documentation, or generated binding tests. Also read it whenever the
user asks for Commit Mode generated-output checks.

Before changing generated C binding documentation, also read
`docs/CBindingDocumentation.md` and use its audit checklist against generated
output when doing generated-output checks or Commit Mode checks.

## Snapshot Workflow

When changing a code generator or generator configuration, capture generated
output before and after the change when the user asks for Commit Mode or
generated-output checks. In Working Mode, use this only when useful for the
specific change.

Prefer the helper, which adds a random five-character suffix to avoid collisions
and removes the snapshot directory when `finish` completes:

```powershell
dotnet run --project scripts\AlvorKit.Script.BindgenReview -- start <library> --case <case>
dotnet run --project scripts\AlvorKit.Script.BindgenReview -- finish <review-root-printed-by-start>
```

If you capture snapshots manually, use an ignored directory under `out/` and
append a random five-character suffix to the case directory:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library> --output-root out\bindgen-review\<case>-<suffix>\before
dotnet run --project scripts\AlvorKit.Script.Bindgen -- <library> --output-root out\bindgen-review\<case>-<suffix>\after
git diff --no-index -- out\bindgen-review\<case>-<suffix>\before out\bindgen-review\<case>-<suffix>\after
```

Regenerate only the binding library whose generator inputs, configuration, or
source project changed. Use `all` only when the change intentionally affects
every generated binding project, and say why in the handoff.

When doing generated-output checks, read the generated source and project-file
diff carefully. Use focused fixtures under `out/bindgen-review/` when a full
binding output is too large, and summarize meaningful generated-code changes
before handoff. Delete disposable `out/bindgen-review/` snapshot directories
before a Commit Mode handoff unless the user explicitly asks to keep them for
follow-up inspection. In a Working Mode handoff, list any generated-output
checks or cleanup that was skipped.

## Generated Text Templates

When adding or changing generated source, project files, scripts, or other
multi-line output, never embed that emitted code directly inside C# string
literals. Put the emitted text in a template under `res/templates/` and render
it with the repository template helper, passing only the dynamic values needed
by the template.

Short identifiers, resource paths, and single-expression replacement values are
fine in code. Full methods, declarations, XML/project fragments, or script
bodies belong in templates.

## Bindgen Boundaries

Do not wire bindgen into normal restore or build targets. Run bindgen for the
changed library, then build; consumers automatically use the exact local
generated project when it exists under `out/bindgen`, and otherwise use the
pinned package.

Do not add `LOCAL_BINDINGS` or any other compile-time symbol to distinguish
local generated bindings from packaged bindings. C# source and tests must
compile the same way whether MSBuild selects a project reference or a package
reference.

Native package builds are intended for CI. Agents must never run
`scripts/AlvorKit.Script.NativeBuild`, invoke native runtime package builds, or
install native build dependencies on a developer machine unless the user
explicitly asks for that work and grants permission for that run.

## Binding Documentation

For generated native bindings, use original upstream documentation whenever it
exists. Mechanical fallback documentation is acceptable only when the upstream
symbol has no usable documentation, and it must describe the public API shape
rather than the generator or selection process.

Every public binding documentation comment must reference the original C symbol
it represents, such as a function, macro constant, typedef, struct, union,
field, or callback. Use exact native names in `<c>...</c>` so a reader can find
the source API. For managed convenience overloads or helpers, inherit or point
back to the native-shaped member and keep the underlying C symbol visible.

For enum groups synthesized from macros, document the public grouping rule or
native API use, such as constants matching `<c>GLFW_CURSOR_*</c>` accepted by
`<c>glfwSetInputMode</c>`. Do not describe groups as constants "selected for the
binding" unless the generation process itself is what the docs are about.

## Generated Native Test Doubles

Generated native binding API projects emit an abstract API class plus
`<ApiClass>Noop` and a forwarding `<ApiClass>Wrapper`. For tests that need a
native library double, subclass the generated noop and override only the calls
the test observes or that construction needs.

Use the wrapper when most calls should forward to a real backend and only a few
need interception or recording. Keep native-library test doubles in tests. Do
not add alternate runtime constructors, ownership flags, or native-free special
cases to product classes just to avoid native calls in tests. Use the generated
backend only when the test intentionally exercises the real native library.
