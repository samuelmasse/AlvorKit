# Code Templates

This directory stores text templates used by source generators and repository
scripts. Templates use simple double-brace placeholders such as `{{TypeName}}`;
the rendering code replaces placeholders with caller-provided values and fails
when a placeholder is missing.

## File Naming

Use a target-language suffix before `.tmpl` whenever the rendered output has a
clear language or file type.

- `*.cs.tmpl` is a full generated C# source file.
- `*.csfrag.tmpl` is a generated C# fragment that is inserted into another
  template or generated source buffer.
- `*.csproj.tmpl` and `*.props.tmpl` are generated MSBuild XML files.
- `*.ps1.tmpl` is a generated PowerShell script.
- `*.txt.tmpl` is generated plain text.

Avoid bare `*.tmpl` for new templates. A bare suffix loses useful editor and
review context, and GitHub Linguist may classify `.tmpl` as Go Template unless
`.gitattributes` overrides it.

## Fragment Rules

C# fragments should contain only the smallest useful declaration, member, or
statement block needed by the emitter. Keep indentation in the fragment when it
is fixed by the surrounding output shape. Put larger generated files in full
templates and pass rendered fragments through placeholders.

Fragment renderers trim trailing newlines from the template file and return the
rendered fragment with exactly one trailing blank line. Put additional
intentional blank lines in the containing full template or in explicit emitter
composition.

Prefer adding or editing templates under this directory instead of embedding
multi-line generated C#, project XML, scripts, or other emitted output directly
inside C# string literals.

## Follow-Up Plan

Use this plan for the remaining template cleanup work.

1. Unify template loading where practical. Bindgen and native-build each carry a
   similar `TemplateResource`, while ECS uses embedded resources and maths wraps
   bindgen's renderer. Keep the embedded-resource path for analyzer packaging if
   needed, but align placeholder validation, newline normalization, and error
   wording.

2. Extend formatting/lint coverage for templates. At minimum, include
   `*.cs.tmpl`, `*.csfrag.tmpl`, `*.csproj.tmpl`, `*.props.tmpl`, `*.ps1.tmpl`,
   and `*.txt.tmpl` in line-length and trailing-whitespace checks where the
   template syntax makes that practical.

3. Review generated output after future behavior changes. For bindgen changes,
   use the bindgen review helper. For maths and ECS generator changes, capture
   focused before/after generated files or run the matching generator tests and
   inspect meaningful output diffs.
