# C Binding Documentation

This guide is for generated C bindings where public managed APIs are projected
from native headers. It is intentionally general-purpose: use it for GLFW,
FreeType, MiniAudio, XxHash, or any future C-header binding.

## Principles

Public binding documentation should help a reader connect the managed API back
to the native API it represents.

- Prefer original upstream documentation whenever a native symbol has usable
  docs.
- Use mechanical fallback docs only when upstream docs are missing or unusable.
- Always name the original C symbol in public docs with `<c>...</c>`.
- Describe the public API shape, not the generator's internal selection process.
- Treat generated convenience overloads as managed projections of a native
  function, not as independent APIs with no native anchor.

Useful native anchors include:

- Function names, such as `<c>glfwCreateWindow</c>`.
- Macro constants, such as `<c>GLFW_KEY_SPACE</c>`.
- Macro families, such as `<c>GLFW_CURSOR_*</c>`.
- Typedefs, such as `<c>GLFWwindowsizefun</c>`.
- Structs, unions, and fields, such as `<c>GLFWgamepadstate</c>` and
  `<c>buttons</c>`.
- Native library or header names for support types, such as `<c>glfw3</c>` or
  `<c>glfw3.h</c>`.

## Common Traps

Macro docs are easy to over-capture. A nearby group heading like `@name
Printable keys` is not documentation for every following macro. A same-line C
comment like `/* non-US #1 */` is not necessarily public API prose either. Do
not let a fallback parser walk across intervening `#define` lines or inherit a
group block as member documentation.

Doxygen group metadata is usually not member prose. Markers such as `@name`,
`@{`, `@}`, `@ingroup`, `@defgroup`, and `@addtogroup` should be ignored unless
the generated documentation is explicitly about the group itself.

Callback docs often contain signature metadata. Sections such as
`@callback_signature`, `@code`, and `@endcode` should not leak into summaries,
returns, or remarks.

Convenience overload docs still need a native anchor. A remark like "Managed
overload. Accepts typed managed values" is weaker than "Managed overload for
`<c>native_function</c>`...".

Generated support types can be public API. Noop types, wrappers, backend types,
native import classes, inline-array helper structs, and UTF-8 helpers should
still reference a native symbol, native field, native header, or native library
when they appear in public documentation.

Catch-all enum docs should not imply the binding selected arbitrary constants.
Prefer "Native macro constants from `<c>library</c>`" or a more specific
macro-family rule. For narrower synthesized enums, describe the native grouping
rule or accepted native API use.

Avoid duplicate anchors. If upstream text already starts with the native symbol,
do not emit `<c>NAME</c> - <c>NAME</c> - ...`.

## Audit Checklist

After changing binding documentation generation, inspect generated output before
handing off.

Check for raw source or Doxygen leakage:

```powershell
rg -n -P "@callback_signature|@brief|@param|@return|@name|@\{|@\}|@code|@endcode|#define|/\*|\*/|\\brief|\\param|\\return" out\bindgen\<BindingProject>
```

Check for generator/meta wording:

```powershell
rg -n -P "selected for the binding|Every .* macro constant|generated type|shared by generated types|this file emits|configured scalar family|Maps <c>|Maps the native field" out\bindgen\<BindingProject>
```

Check for duplicated native anchors:

```powershell
rg -n -P "<c>([^<]+)</c> - <c>\1</c>" out\bindgen\<BindingProject>
```

Check suspiciously long one-line remarks manually. Long upstream remarks are not
automatically wrong, but they can reveal swallowed source blocks, tables, or
group prose:

```powershell
$root = "out\bindgen\<BindingProject>"
Get-ChildItem $root -Recurse -Filter *.cs | ForEach-Object {
    $path = $_.FullName
    $lines = Get-Content $path
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '///\s*<(summary|param|returns|remarks)\b' -and $lines[$i].Length -gt 500) {
            "{0}:{1}:{2}" -f (Resolve-Path -Relative $path), ($i + 1), $lines[$i].Trim()
        }
    }
}
```

For generated public docs, sample each category:

- API class summary.
- Native-shaped function docs.
- Managed convenience overload docs.
- Macro enum group summary and member summaries.
- Catch-all macro enum summaries.
- Struct, union, field, and inline-buffer docs.
- Handle docs.
- Callback delegate and parameter docs.
- Backend, native import, noop, and wrapper docs.

If any category cannot name a native symbol naturally, name the closest native
surface honestly, such as the native library or header. Do not invent a C symbol
that does not exist.

