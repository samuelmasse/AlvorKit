# Documentation Policy

## Scope

Read this policy before changing public documentation, generated documentation,
or native binding documentation.

## Rules

## Documentation

Write public documentation for a reader who only sees the published API, tool,
or document. Avoid meta descriptions that only make sense to the author, an
agent, or a generator maintainer unless the generation process is itself the
subject. Prefer domain wording and concrete examples of the public things the
documentation describes.

Before changing generated C binding documentation, read
`docs/CBindingDocumentation.md`. Use its audit checklist against generated
output when doing generated-output checks or Commit Mode checks; in a Working
Mode handoff, list that audit if it was skipped.

For generated native bindings, use original upstream documentation whenever it
exists. When upstream has no usable documentation, author documentation from
the public API shape rather than describing the generator or selection process.
Every public binding documentation comment must reference the original C symbol
using exact native names in `<c>...</c>`. For managed convenience overloads or
helpers, inherit or point back to the
native-shaped member and keep the underlying C symbol visible. For enum groups
synthesized from macros, document the public grouping rule or native API use.
