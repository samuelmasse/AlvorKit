# C# Policy

## Scope

Read this policy before changing any hand-authored or generated C# source or C#
template in AlvorKit or an inheriting game repository.

## Hard Stops And Overrides

These rules are invariants unless this document explicitly names an exception.
Closer instructions may add stricter requirements but cannot relax the
repository-wide 140-character hard maximum or the bans on repository-owned
`sealed` and `checked` declarations.

## Line Length

Use conventional, readable C# formatting. Do not compress code into cryptic
one-liners or pack unrelated constructs together merely to reduce vertical
space. Conversely, when a cohesive declaration, call, condition, initializer,
or expression reads clearly on one line and fits within 120 characters, keep
it on one line instead of breaking it prematurely into vertical fragments.
Treat 120 characters as the preferred C# wrapping point and 140 characters as
the hard maximum for agent-authored C# in both Working Mode and Commit Mode.
No closer `AGENTS.md` may relax this rule. This does not change automated
checks, which retain their existing 170-character failure threshold.

## C# Defaults

These defaults are unconditional. A closer `AGENTS.md` may add stricter
requirements but must not relax them.

- Omit braces when an `if`, `else`, `for`, `foreach`, `while`, or similar
  control-flow body contains exactly one statement and that complete statement
  occupies one physical line. A statement split across multiple physical lines
  requires braces even when it is syntactically one statement. This restriction
  applies to control-flow bodies, not expression-bodied members, lambdas, or
  other `=>` expressions. Braces are for multiline or multi-statement bodies,
  not single-line bodies. An unbraced `else` with exactly one statement must put
  that statement on the same physical line as `else`, as in
  `else DoOtherThing();`. Placing that single statement on the following line
  is banned. If the complete `else` statement cannot stay readable within the
  line-length limit, use a braced body.

  ```csharp
  if (condition)
      DoThing();
  else DoOtherThing();

  for (var index = 0; index < count; index++)
      Process(index);

  foreach (var value in values)
  {
      PublishTransformedValue(value, sourceConfiguration, destinationConfiguration, transformationContext,
          validationContext, diagnosticContext);
  }
  ```

- Strongly prefer guard clauses and early `return`, `continue`, or `break`
  statements that keep the main path flat. Avoid complicated `if`/`else`
  chains and nested conditionals when their exceptional or terminal cases can
  be handled first.
- Keep each mathematical computation and boolean expression on one physical
  line whenever it remains readable within the hard line-length limit. Prefer
  that single-line form even when it exceeds the preferred wrapping point. If
  the logic cannot remain clear on one line, split it into named intermediate
  variables or sequential steps whose individual computations and boolean
  expressions each stay on one line. Do not wrap one operator chain or
  parenthesized computation across multiple lines.
- Standalone discard assignments such as `_ = expression;` are banned. Do not
  create a fake assignment to silence an unused-value warning. Remove a
  no-effect expression, invoke a method directly when only its side effect
  matters, or name and use the result when it matters. Required interface,
  delegate, or framework parameters may remain unused without assigning them
  to `_`. This rule does not ban `out _`, deconstruction discards, or `_`
  patterns.
- Keep the input expression and `switch` keyword of a switch expression on the
  same physical line as the declaration, assignment, `return`, or expression
  arrow when the line fits within the length limit. Put the opening brace on
  the next line at the declaration's normal indentation and indent switch arms
  one level, just like a normal method body. Do not put `switch` on a separately
  indented continuation line.

  ```csharp
  private static Result Convert(Value value) => value switch
  {
      Value.First => Result.First,
      Value.Second => Result.Second,
      _ => throw new UnreachableException(),
  };
  ```

- Use binary literals (`0b...`) for bit masks, packed flags, and other bitwise
  constants whose meaning depends on the position, adjacency, or grouping of
  individual bits. Group binary digits with `_` when that makes fields easier
  to read. Hexadecimal literals remain valid when hexadecimal communicates the
  value more clearly, including powers of two, full-byte or all-ones patterns
  such as `0xFF` and `0xFFFF`, and values defined by an external hexadecimal
  contract. Do not use hexadecimal merely to shorten a positional bit layout.
- A `.cs` file may live directly at the root of its project when that is the
  clearest home. Prefer one top-level type per `.cs` file; do not group multiple
  records, classes, structs, or interfaces in a protocol, model, command, or
  `Types` file just because they are small.
- Use of the C# `sealed` keyword in repository-owned declarations is banned. Do
  not use it on classes, records, overrides, or any other declaration,
  including generated output, examples, demos, scripts, and tests.
- Use of the C# `checked` keyword is banned in repository-owned code. Do not use
  checked expressions or blocks, including in generated output, templates,
  examples, demos, scripts, and tests. Express any required range contract
  without the keyword.
- Organize class and struct members in this exact category order:
  1. constants;
  2. readonly fields;
  3. non-readonly fields;
  4. properties with only a `get` accessor;
  5. properties with both read and write access, including `set` or `init`;
  6. `ref` and `ref readonly` properties;
  7. constructors;
  8. all remaining members.

  Constants always precede instance members and reverse the ordinary
  accessibility order: `public`, then `internal`, then `private`.
  Within each field or property category, order accessibility as `private`,
  then `internal`, then `public`. Static readonly fields remain readonly fields.
  Static and instance members do not otherwise create separate categories. Keep
  overloads and closely related members together only within these ordering
  constraints. A multiline property with nontrivial accessor logic may be
  placed after all simple properties as the final property block immediately
  before the constructor, or before the remaining members when there is no
  constructor.

- Keep consecutive fields and simple properties compact. Strongly prefer no
  blank lines between members of the same category; add vertical space only
  when it marks a meaningful category boundary or isolates a nontrivial
  multiline property implementation.
- Constants and fields are distinct member categories. Put exactly one blank
  line between the final constant and the first field below it; never declare
  constants and fields as one compact block.
- Keep every class and struct at no more than eight directly retained instance
  fields. Constants and static fields do not count; auto-property backing
  storage and positional-record members do count. When cohesive private state
  would exceed the limit, group it into one or more private nested carrier
  structs. An embedded state-carrier struct must never be `internal` or
  `public`, must not escape its containing type, and must not be returned or
  passed by value. Its fields use `public` PascalCase names so the containing
  type can access them; the carrier's private accessibility keeps those fields
  effectively private. This is the sole exception to the ordinary
  private-field rule. A passive carrier declares no constructor, including no
  primary constructor. Initialize its fields explicitly where the owning state
  is established. Add a carrier constructor only when construction enforces a
  real invariant rather than merely copying values into fields. A standalone
  `internal` passive carrier follows the same shape with `internal` PascalCase
  fields; do not recreate private backing fields and forwarding properties
  inside it. Do not make the carrier `readonly` merely to force
  constructor-based population; the owner may retain the fully initialized
  carrier in a `readonly` field instead.
- A private embedded carrier must reduce the containing type's access surface,
  not merely its direct field count. Never re-expose a carrier by mirroring its
  fields through a block of one-to-one forwarding properties or methods. A
  value that needs its own forwarding member is not private carrier state. Keep
  such values directly on a deliberately small contract, or define a
  standalone collaboration or snapshot type at the narrowest required
  accessibility and expose the cohesive group as one member.
- Default parameter values are banned. Every caller must supply every argument;
  use a distinctly named method or overload only when it represents a genuinely
  different operation rather than recreating an implicit default.
- In every multiline declaration parameter list, put the closing `)` directly
  after the final parameter. A closing parenthesis on its own line is banned.
  This applies to methods, constructors, primary constructors, records,
  delegates, and lambdas.
- An injected service enters a collaborator only through constructor injection.
  Never pass an injected service through an ordinary method, local function,
  delegate, command, record, or other operation parameter. A type is either a
  scope-owned injected service or an explicitly passed ordinary object, never
  both. Per-call parameters contain operation data, not hosted collaborators.
- Strongly prefer private fields for storage. Expose state through the
  narrowest useful property instead of an `internal` or `public` field. When a
  caller genuinely needs by-reference access, keep the backing field private
  and expose a narrowly scoped `ref` or `ref readonly` property. Do not
  disguise unrestricted field access behind trivial
  `get => field; set => field = value;` forwarding. Use a writable `ref`
  property for that contract. Keep get/set accessors only when they validate,
  transform, restrict, or otherwise own behavior. Use an exposed field only
  when a framework, binary layout, generated-code contract, or measured
  hot-path requirement specifically demands one.
- Prefer a `readonly struct` when none of its instance members mutate retained
  state. In a non-readonly struct, explicitly mark every hand-authored instance
  member that does not mutate retained state as `readonly`, including
  expression-bodied properties, get-only properties, ordinary methods, and
  non-mutating getters on behavioral get/set properties. Use an accessor-level
  `readonly get` when the setter must remain mutating. A member that returns a
  writable `ref`, or otherwise mutates the receiver, must remain non-readonly.
- Auto accessors are banned in hand-authored classes and non-record structs. A
  property must compute its value or use explicit accessors over private backing
  fields. Records are the only hand-authored exception: auto-properties and
  positional records are allowed when they clearly express the record's value
  shape. Generated code is also exempt because its source shape belongs to the
  generator. Accessor-only declarations on interfaces and abstract members are
  contracts rather than stored auto-properties and are allowed.
- Avoid the static `Array` API for operations on existing contiguous storage,
  including `Array.Clear`, `Array.Fill`, `Array.Copy`, `Array.IndexOf`,
  `Array.Reverse`, and `Array.Sort`. Obtain an appropriate `Span<T>` or
  `ReadOnlySpan<T>` view and use span-based operations instead.
- Prefer repository-level and project-level global usings over ordinary
  file-level `using` directives. Before adding a file-level import, check
  implicit usings and existing `<Using Include="..." />` entries. Add broadly
  useful namespaces to the area `Directory.Build.props`; add project-only
  namespaces to the `.csproj`. Reserve file-level imports for aliases, rare
  conflicts, or one-off third-party APIs. `using var` and `using (...)`
  disposal statements are allowed and are not import directives.
- Use a primary constructor by default for every class or behavioral struct
  that receives constructor parameters, including public types, facades,
  injected services, and stateful implementation types. New declarations must
  use this shape, and materially edited types should convert assignment-only
  explicit constructors. Passive field-carrier structs are the exception: they
  receive no constructor merely to populate fields and are initialized
  explicitly by their owner.
- Refer to captured parameters directly when no named field is required. Do not
  introduce mirrored private fields and an assignment-only constructor body.
  When a parameter needs named storage, validation, or derived initialization,
  retain the primary constructor and initialize the field or property inline,
  using a focused static helper when necessary. Additional constructor
  overloads must chain to the primary constructor and are not a reason to
  abandon it.
- Use an explicit constructor only when the required contract cannot be
  expressed clearly with a primary constructor, such as when constructor
  accessibility must differ from type accessibility or initialization
  inherently requires statement-level control flow or ordered side effects.
  Ordinary dependency capture, validation, derived values, base-constructor
  arguments, and named backing fields are not exceptions. A ref-like parameter
  that the compiler cannot capture may initialize one explicit ref-like field
  inline while the remaining parameters stay captured. In partial types, first
  verify whether primary constructor parameters are already in scope.
- Trust nullable reference type analysis for non-null contracts. Do not add
  manual null guards or asserts just to recheck a non-nullable value.
- Prefer file-scoped namespaces, nullable-aware code, collection expressions,
  and the style already enforced by `.editorconfig`. Avoid new production
  dependencies unless the task clearly needs them and the tradeoff is explained.
- Prefer functional style where it improves clarity: pure helpers, immutable
  values, small transformations, explicit inputs and outputs, and minimal shared
  mutable state.
- Prefer tuple literals for repository vector types such as `Vec2`, `Vec3`, and
  `Vec4` when the target type is clear. Use constructors when the constructor is
  the point, such as scalar splats, composition constructors, conversion tests,
  or expressions with no target vector type.
- Prefer repository vector casts such as `(Vec2u)image.Size` over converting
  components one by one.
- Treat AlvorKit maths types as first-class API shapes. Accept and pass vectors,
  matrices, quaternions, boxes, and related maths types instead of flattening
  true maths values into scalar overloads.
- Do not silently clamp, coerce, or normalize caller-provided values in property
  setters or state updates. Model the invariant in the type system, or clamp
  explicitly at a platform boundary.
- In AlvorKit's curated library projects, do not create private nested classes
  for helper composition; prefer internal top-level helper types when they are
  intentionally outside the public API. Game repositories override this and
  prefer public game-code types and collaborating members. Avoid partial classes
  for hand-authored code except for generated-code integration or unavoidable
  framework/tooling requirements, and mention the reason in the work summary.
- Avoid generic `Factory`, `Manager`, `Service`, and similarly broad suffixes
  when a constructor, static `Create`, delegate, or domain-specific type name is
  clearer. Generally avoid static helper types and methods in hand-authored
  code; reserve static members for constants, operators, pure domain functions
  with no collaborator dependency, and framework-required entry points.
