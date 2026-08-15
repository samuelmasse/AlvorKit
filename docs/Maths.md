# AlvorKit Maths

This is the map of the AlvorKit maths ecosystem: which packages exist, where
the source actually lives, every public value type, the naming scheme, and the
usage rules. For the per-family member reference, read
[Maths Reference](MathsReference.md).

## Package shape

Consumers reference one facade package:

```text
AlvorKit.Maths                the facade package consumers reference
    AlvorKit.Maths.Core       hand-authored: ScalarMath, generic interfaces,
                              shared enums, internal parse/format helpers
    AlvorKit.Maths.Primitives generated: every concrete vector, matrix,
                              quaternion, box, and geometry struct
```

Every public type lives in the flat `AlvorKit` namespace regardless of which
underlying project declares it.

The concrete primitive structs are generated and are not committed under
`src/`. Searching the repository for `struct Vec3i` finds nothing; that is by
design, not a missing type:

- `scripts/AlvorKit.Script.MathsGen` emits `AlvorKit.Maths.Primitives` from
  its `Catalogs`, `Specs`, and `Emitters`.
- Builds normally consume the pinned `AlvorKit.Maths.Primitives` package.
- `dotnet run --project scripts/AlvorKit.Script.MathsGen` writes an
  inspection-safe copy to `out/generated/mathgen` without activating it.
- `--setup-local` writes to `out/mathgen`, which switches the whole build to
  the local generated project (see the README's maths package development
  mode section).

To read a concrete generated type, generate the inspection copy or read the
hand-authored interfaces in `src/AlvorKit.Maths.Core`, which declare the same
contracts generically. To change the generated surface, change the generator,
never emitted files, and follow
[Generated Output Checks](GeneratedOutputChecks.md).

`AlvorKit.OpenGL.Maths` is a separate package layering allocation-free OpenGL
overloads (uniforms, viewports, clears, buffers) over these types; see
[OpenGL maths overloads](OpenGLMaths.md). `AlvorKit.OpenGL.Maths` and
`AlvorKit.Graphics2D` consume the maths types; the maths packages depend on
nothing.

## Naming scheme

A type name is `<family><dimension><scalar suffix>`. The scalar suffixes are:

| Suffix | Scalar | Suffix | Scalar |
| --- | --- | --- | --- |
| *(none)* | `float` | `i16` | `short` |
| `d` | `double` | `u16` | `ushort` |
| `h` | `Half` | `i` | `int` |
| `b` | `bool` | `u` | `uint` |
| `i8` | `sbyte` | `i64` | `long` |
| `u8` | `byte` | `u64` | `ulong` |
| | | `i128` | `Int128` |
| | | `u128` | `UInt128` |

So `Vec3` is the float 3-vector, `Vec3i` the `int` 3-vector, `Vec2u` the
`uint` 2-vector, `Mat4d` the double 4x4 matrix, `Box3i` the integer 3D box.
The one naming exception is the 1D interval family, where the float type is
`Intervalf` because `Interval` alone would not name a dimension.

Matrices are named `Mat<columns>x<rows>` (`Mat3x2` has three columns and two
rows) with square shapes shortened to `Mat2`, `Mat3`, `Mat4`. Storage is
column-major.

## Public type inventory

Vectors — 42 partial structs, 14 scalar families times dimensions 2, 3, 4:

| Family | 2D | 3D | 4D |
| --- | --- | --- | --- |
| `float` | `Vec2` | `Vec3` | `Vec4` |
| `double` | `Vec2d` | `Vec3d` | `Vec4d` |
| `Half` | `Vec2h` | `Vec3h` | `Vec4h` |
| `bool` mask | `Vec2b` | `Vec3b` | `Vec4b` |
| `sbyte` | `Vec2i8` | `Vec3i8` | `Vec4i8` |
| `byte` | `Vec2u8` | `Vec3u8` | `Vec4u8` |
| `short` | `Vec2i16` | `Vec3i16` | `Vec4i16` |
| `ushort` | `Vec2u16` | `Vec3u16` | `Vec4u16` |
| `int` | `Vec2i` | `Vec3i` | `Vec4i` |
| `uint` | `Vec2u` | `Vec3u` | `Vec4u` |
| `long` | `Vec2i64` | `Vec3i64` | `Vec4i64` |
| `ulong` | `Vec2u64` | `Vec3u64` | `Vec4u64` |
| `Int128` | `Vec2i128` | `Vec3i128` | `Vec4i128` |
| `UInt128` | `Vec2u128` | `Vec3u128` | `Vec4u128` |

Matrices — 18 structs, nine column-by-row shapes for `float` and `double`:
`Mat2`, `Mat2x3`, `Mat2x4`, `Mat3x2`, `Mat3`, `Mat3x4`, `Mat4x2`, `Mat4x3`,
`Mat4`, plus the same names with the `d` suffix.

Quaternions — `Quat`, `Quatd`.

Boxes — axis-aligned, `float`, `double`, and `int`: `Box2`, `Box2d`, `Box2i`,
`Box3`, `Box3d`, `Box3i`.

Float and double geometry — one float and one `d` variant each:
`Plane3`, `Frustum3`, `Sphere3`, `Capsule3`, `Obb3`, `Ray3`, `Segment3`,
`Triangle3`, `Quad3`, `Intervalf`/`Intervald`, `Viewport`/`Viewportd`.

Hand-authored public surface in `AlvorKit.Maths.Core`:

- `ScalarMath` — generic scalar functions (`Min`, `Max`, `Clamp`, `Abs`,
  `Lerp`, `SmoothStep`, trigonometry, `Sqrt`, `Pow`, bit counts,
  `IsPowerOfTwo`, `Select`, and more) usable in generic maths code over any
  supported scalar.
- Shared enums — `ContainmentKind` (`Disjoint`/`Intersects`/`Contains`),
  `PlaneIntersectionKind` (`Negative`/`Intersecting`/`Positive`),
  `ProjectionDepthRange` (`NegativeOneToOne`/`ZeroToOne`),
  `ProjectionHandedness` (`Right`/`Left`).
- Generic interfaces — the `IVec*`, `IMat*`, `IQuat*`, `IBox*`, and
  per-geometry interface families that every generated struct implements.
  Use these to write algorithms once across scalar families or dimensions;
  use the concrete structs everywhere else.

The `Maths*Helper` classes in `AlvorKit.Maths.Core/Helpers` are internal
infrastructure for the generated code and are not part of the public surface.

## Usage rules

Using this ecosystem is mandatory. A value that is a position, size, offset,
direction, color, extent, range, rotation, or transform is represented with
the published maths type — everywhere: fields, parameters, returns, records,
components, protocols, and tests.

- Never introduce a local substitute for a maths value: no
  `(int X, int Y, int Z)` tuple storage or parameters, no parallel
  `x`/`y`/`z` scalar members, no hand-written vector, point, size, rect,
  bounds, or range structs. If the exact shape exists (`Vec3i`, `Vec2u`,
  `Box2i`, `Intervalf`), use it; a genuinely domain-specific type wraps or
  exposes maths types rather than re-declaring their components. (Tuple
  *literals* remain the preferred construction syntax for a maths-typed
  target — the ban is on tuples as the declared type.)
- Never re-implement maths the surface already provides. Clamp, lerp,
  saturate, smoothstep, min/max, abs, dot, cross, distance, normalization,
  rounding, remapping, power-of-two tests, and bit counts exist on the
  vector families and `ScalarMath`; call them instead of writing the
  formula inline or adding a private helper.
- A missing reference is not an excuse. When the current project does not
  reference `AlvorKit.Maths`, add the reference instead of smuggling the
  value through tuples, arrays, or custom structs. The maths packages
  depend on nothing and are safe to reference from any layer.
- Treat maths types as first-class API shapes. Accept and return `Vec3i`,
  `Box3i`, `Mat4`, and friends; do not flatten a true maths value into
  `int x, int y, int z` scalar parameters or parallel scalar overloads.
- Prefer tuple literals for vectors when the target type is clear
  (`Vec3 direction = (0.2f, -1f, 0.4f);`). Use constructors when the
  constructor is the point: scalar splats, composition constructors such as
  `new Vec4(xyz, w)`, conversion tests, or expressions with no target type.
- Prefer repository vector casts over per-component conversion:
  `(Vec2u)image.Size`, not `new Vec2u((uint)size.X, (uint)size.Y)`. Widening
  conversions between scalar families are implicit; narrowing or
  sign-changing conversions are explicit casts.
- Pick the scalar family that states the domain: cell coordinates and sizes
  are integer vectors (`Vec3i`, `Vec2u`, `Box3i`), continuous space is float
  (`Vec3`, `Box3`), precision-critical accumulation is double. The `Half`,
  8/16-bit, and 128-bit families exist for storage, packing, and interop
  shapes.
- Matrices are column-major; transform composition follows
  `projection * view * model`, and the OpenGL overloads upload storage
  directly with `transpose: false`.
- In generic maths code, use `ScalarMath` and the `AlvorKit`
  interfaces rather than re-deriving per-scalar arithmetic.
- The float vector and matrix surface is System-backed where `System.Numerics`
  implements the same operation; behavior differences from the historical
  scalar implementations are documented in
  [AlvorKit vectors versus System.Numerics](Maths.SystemVectorDifferences.md).

## Deeper documentation

- [Maths Reference](MathsReference.md) — per-family member reference for
  every public type.
- [OpenGL maths overloads](OpenGLMaths.md) — GL integration surface.
- [AlvorKit vectors versus System.Numerics](Maths.SystemVectorDifferences.md)
  — semantic and performance comparison for the float families.
