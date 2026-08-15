# AlvorKit Maths Reference

Per-family member reference for the public `AlvorKit.Maths` types. For the
package shape, full type inventory, naming scheme, and usage rules, read
[Maths](Maths.md) first.

The generated types are deliberately uniform: every member described for a
family exists on each of its scalar variants with the scalar, vector, and
matrix types substituted. Sections therefore describe one family and then
list only the differences per scalar family or dimension.

## Conventions shared by every type

Every public type is a `partial struct` in the `AlvorKit` namespace
with a primary constructor over its fields. Vectors, `Mat4`, `Mat3x2`, and
`Quat` use explicit layout (for aliases and `System.Numerics` overlays);
other types are sequential.

Every type carries this common contract:

- `public const int ComponentCount` and `public const int SizeInBytes`
  (boxes declare `Dimension` instead of `SizeInBytes`).
- A `ReadOnlySpan<TScalar>` constructor plus matching static `Create`
  overloads for the field forms and the span form.
- A bounds-checked scalar indexer `this[int index] { get; set; }` and
  `static ref TScalar ComponentRef(ref TSelf value, int index)`.
- `Deconstruct` into the fields, and implicit named-tuple conversions in
  both directions (`(Vec3 Min, Vec3 Max)`, `(float X, float Y, float Z)`).
- `CopyTo(Span<TScalar>)` / `TryCopyTo(Span<TScalar>)`; vectors, matrices,
  `Quat`, and `Plane3` also have `CopyTo(TScalar[])` and
  `CopyTo(TScalar[], int)`.
- Scalar `==` / `!=` operators, `Equals`, `GetHashCode`, and a
  lexicographic `CompareTo`.
- Formatting and parsing: `ToString()` plus format/provider overloads,
  `TryFormat` for both `Span<char>` and UTF-8 `Span<byte>`, and
  `Parse`/`TryParse` for `string`, `ReadOnlySpan<char>`, and UTF-8
  `ReadOnlySpan<byte>`. The text form is `"(a, b, c)"`.

Float/double pairing is uniform across all families: `float` → `double` is
an implicit operator, `double` → `float` is explicit, and the double
variants drop all `System.Numerics` interop but are otherwise
member-for-member identical.

## Vectors

42 types: 14 scalar families (`Vec3`, `Vec3d`, `Vec3h`, `Vec3b`, `Vec3i8`,
`Vec3u8`, `Vec3i16`, `Vec3u16`, `Vec3i`, `Vec3u`, `Vec3i64`, `Vec3u64`,
`Vec3i128`, `Vec3u128`) at dimensions 2, 3, and 4.

### Components and aliases

Components are public mutable **fields** with alias fields overlaid at the
same offsets, so writing `R` writes `X`:

| Set | 2D | 3D | 4D |
| --- | --- | --- | --- |
| Positional | `X`, `Y` | `X`, `Y`, `Z` | `X`, `Y`, `Z`, `W` |
| Color | `R`, `G` | `R`, `G`, `B` | `R`, `G`, `B`, `A` |
| Texture | `S`, `T` | `S`, `T`, `P` | `S`, `T`, `P`, `Q` |

### Construction

- `new Vec3i(x, y, z)`, splat `new Vec3i(value)`, and
  `new Vec3i(ReadOnlySpan<int>)`, each with a matching static `Create`.
- Composition constructors from lower dimensions:
  `new Vec3i(Vec2* xy, int z)`, `new Vec3i(int x, Vec2* yz)`,
  `new Vec4i(Vec3* xyz, int w)`, `new Vec4i(Vec2* xy, int z, int w)`, and
  so on — accepting the matching dimension of **any** scalar family.
- Truncating constructors from every higher-dimension family:
  `new Vec3i(Vec4* value)`.

### Constants

- All numeric families: `Zero`, `One`, `UnitX`, `UnitY` (+`UnitZ`,
  +`UnitW` by dimension).
- Float family adds `PositiveInfinity`, `NegativeInfinity`, `NaN`,
  `Epsilon`.
- Bool masks use `False` / `True` instead of `Zero` / `One` and have no
  unit vectors.

### Swizzles

Every vector type has swizzle properties under three separate alphabets —
`XYZW`, `RGBA`, `STPQ`, never mixed. For each alphabet, every sequence of
length 2, 3, and 4 drawn (with repetition) from the type's components
exists as a PascalCase property (`Xy`, `Xzy`, `Rgb`, `Stpq`, `Xxxx`)
returning the same scalar family at the sequence's arity. A swizzle is
settable exactly when its components are all distinct (`Xzy` has a setter;
`Xxy` is get-only).

### Conversions

The generator lifts C#'s scalar conversion table component-wise:

- Same-dimension conversions are **implicit** exactly when the scalar
  conversion is value-preserving widening (`Vec3u8` → `Vec3i`, `Vec3i` →
  `Vec3d`), and **explicit** otherwise — all narrowing, `double` →
  `float`, sign changes (`Vec3i` ↔ `Vec3u`), and anything involving the
  bool mask family.
- Cross-dimension operators are always explicit and always narrowing
  (`(Vec3i)someVec4i` drops `W`). Widening a dimension is done with the
  composition constructors, never a cast.
- Named-tuple conversions are implicit both ways.

### Operators

- All numeric families: unary `+` `-` (signed), `++` `--`, and
  component-wise `+ - * / %` against the same type and against the scalar
  in both orders.
- Mixed-scalar operator overloads exist and **promote following C#'s
  scalar promotion table** (`Vec3i * long` → `Vec3i64`, `Vec3i + float` →
  `Vec3`, `Vec3u + int` → `Vec3i64`). The narrow families (`i8`, `u8`,
  `i16`, `u16`) promote even same-type arithmetic to the `int` family:
  `Vec3i8 + Vec3i8` is `Vec3i`, and only `++`/`--` stay narrow. Unsigned
  unary negation promotes (`-Vec3u` is `Vec3i64`). `Half` and `double`
  vectors never promote.
- Integer families add `~`, `& | ^` (vector and scalar forms, `& | ^`
  promote like arithmetic), and shifts `<< >> >>>` (by `int` or by vector;
  shifts never promote).
- Bool masks have `!`, `~`, `& | ^`, and `operator true`/`false` (true
  when all components are set).
- Ordering operators `< <= > >=` return the dimension's **mask type**
  (`Vec3b`), with scalar and cross-family overloads following the same
  promotion families. `==`/`!=` return plain `bool`; the mask versions are
  the named `Equal`/`NotEqual`.

### Functions

All numeric families:

- `Min`, `Max`, `Clamp(vector, vector, vector)`,
  `Clamp(vector, scalar, scalar)`; `Abs` on signed and floating families
  only (unsigned families have no `Abs`).
- `Dot`, `LengthSquared`, `Length`, `DistanceSquared`, `Distance`.
  Integer families return `float` from `Length`/`Distance` (and the
  element type from `Dot`/`LengthSquared`); `Half` returns `Half`.
- 3D: `Cross(a, b)` returning the vector. 2D instead has scalar
  `Cross(a, b)`, `PerpDot(a, b)`, and the instance properties
  `PerpendicularLeft` / `PerpendicularRight`. 4D has no cross or
  perpendicular members.
- Named mask comparisons: `Equal`, `NotEqual`, `LessThan`,
  `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual` (vector, scalar,
  and cross-family overloads).
- There are **no horizontal reductions** — no `Sum`, `Product`,
  `MinComponent`, or `MaxComponent`.

Floating families (`Vec*`, `Vec*d`, `Vec*h`) add:

- `Normalized`, `NormalizedOrZero`, `NormalizedOr(fallback)`,
  `TryNormalize`, static `Normalize`.
- `Lerp` (scalar and per-component vector amount), `Barycentric`,
  `Reflect`, `FaceForward`, `Refract`.
- `Saturate`, `Floor`, `Ceiling`, `Round` (+`MidpointRounding`),
  `Truncate`, `FractionalPart`, `Modulo`, `Mod`, `Step`, `SmoothStep`
  (vector and scalar edges).
- `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Atan2`, `Exp`, `Log`,
  `Log2`, `Sqrt`, `InverseSqrt`, `Pow` (vector and scalar exponent),
  `FusedMultiplyAdd`.
- Classification masks `IsNaN`, `IsInfinity`, `IsFinite`.
- Instance conversions to the `int` family: `FloorToVec3i`,
  `CeilingToVec3i`, `RoundToVec3i`, `TruncateToVec3i` (per dimension).

Integer families add bit functions, always returning the `int` family
regardless of source width: `BitCount`, `LeadingZeroCount`,
`TrailingZeroCount`, `FindLeastSignificantBit`, `FindMostSignificantBit`,
plus mask-returning `IsPowerOfTwo`.

Bool masks (`Vec2b`, `Vec3b`, `Vec4b`):

- Reductions `All`, `Any`, `None`.
- `Select(whenTrue, whenFalse)` — one instance overload per scalar family
  (14 per dimension), the component-wise conditional.
- Only `Equal`/`NotEqual` named comparisons; no arithmetic, ordering,
  lengths, or `Min`/`Max`.

### System.Numerics interop

Float vectors only: `Vec2`/`Vec3`/`Vec4` convert **implicitly both ways**
with `Vector2`/`Vector3`/`Vector4` and internally overlay a packed System
vector that hardware-accelerates `Min`, `Max`, `Clamp`, `Abs`, `Saturate`,
`Floor`, `Ceiling`, `Round`, `Truncate`, `Step`, `Sqrt`, `InverseSqrt`,
`FusedMultiplyAdd`, and `Cross`. No other scalar family has System
interop. Semantic notes live in
[AlvorKit vectors versus System.Numerics](Maths.SystemVectorDifferences.md).

## Matrices

18 types: shapes `Mat2`, `Mat2x3`, `Mat2x4`, `Mat3x2`, `Mat3`, `Mat3x4`,
`Mat4x2`, `Mat4x3`, `Mat4` for `float`, plus the `d` doubles.

### Layout

Storage is **column-major**. `Mat<C>x<R>` has `C` columns and `R` rows;
squares shorten to `Mat2`/`Mat3`/`Mat4`. Columns are public fields
(`Column0`..`Column3`) of the column vector type (`Mat3x2.Column0` is
`Vec2`); rows are computed properties (`Row0`..`Row3`) of the row vector
type (`Mat3x2.Row0` is `Vec3`). The scalar constructor takes components in
column-then-row order (`m00, m01, ...` walks column 0 first), the
two-argument indexer is `this[int column, int row]`, and the
single-argument indexer returns a column. `Mat4` and `Mat3x2` overlay
`System.Numerics.Matrix4x4` / `Matrix3x2` internally.

### Common surface (every shape)

- Constructors: columns, diagonal splat, all scalars, column-major span.
- Factories: `CreateColumns`, `CreateRows`, `CreateDiagonal` (scalar and
  vector), `CreateOuterProduct(column, row)`, `FromColumnMajor`,
  `FromRowMajor`, `Lerp`.
- `Zero`; `Diagonal` and each `RowN` are settable properties;
  `ColumnRef`/`ComponentRef` by-ref access.
- `CopyTo`/`TryCopyTo` plus explicit `CopyToColumnMajor` /
  `CopyToRowMajor` variants.
- Component-wise operators `+ - / %` in three forms (matrix∘scalar,
  scalar∘matrix, matrix∘matrix) and `*` for scaling by a scalar.
  Component-wise multiplication is the named `ComponentMultiply` — `*`
  between matrices is genuine composition.
- Multiplication follows the shape law `MatCxR * MatKxC -> MatKxR` (three
  matrix overloads per type), plus both vector conventions:
  `MatCxR * VecC -> VecR` (column vector) and `VecR * MatCxR -> VecC`
  (row vector).
- `Transposed` / `Transpose` returning the transposed shape.
- Mask relations `Equal` / `NotEqual` (exact, scalar-epsilon, and
  matrix-epsilon) returning the column-count mask vector; queries
  `IsNull(epsilon)`, `IsIdentity(epsilon)`.
- The standard parse/format family.

### Square extras (`Mat2`, `Mat3`, `Mat4`)

`Identity`, `Trace`, `Determinant`, `Adjugate`, `Inverted` / `Invert` /
`TryInvert`, `InverseTransposed` / `InverseTranspose`,
`IsNormalized(epsilon)`, `IsOrthogonal(epsilon)`. `Mat3` and `Mat4` add
`AffineInverted` / `AffineInverse`; `Mat3` alone adds `Orthonormalized` /
`Orthonormalize`.

### `Mat4` — 3D transform surface

- Settable `Translation` (`Vec3`); factories `CreateTranslation`,
  `CreateScale` (vector/scalar, with optional center),
  `CreateRotationX/Y/Z` (optional center), `CreateRotation(radians, axis
  [, center])`, `CreateShear`, `CreateScaleBias`,
  `CreateReflection(Plane3)`, `CreateShadow(lightDirection, Plane3)`,
  `CreateWorld(position, forward, up)`, `MatrixCross3`/`MatrixCross4`.
- Post-multiply appliers mirroring each factory: `Translate`, `Scale`,
  `Rotate`, `RotateX/Y/Z`, `Shear`, `ScaleBias` (each returns
  `value * CreateXxx(...)`).
- Views: `LookAt` / `LookTo`, defaulting to right-handed with an explicit
  `ProjectionHandedness` overload.
- Projections — each family has a default overload (right-handed,
  `NegativeOneToOne`, the OpenGL convention) and an explicit
  `(ProjectionHandedness, ProjectionDepthRange)` overload:
  `CreatePerspectiveFieldOfView`, `CreatePerspective`,
  `CreatePerspectiveOffCenter`, `CreateFrustum`, `CreateOrthographic`,
  `CreateOrthographicOffCenter`, `CreateInfinitePerspective`,
  `CreateTweakedInfinitePerspective` (the tweaked form only implements
  right-handed `NegativeOneToOne` and throws for other combinations).
- Viewport and picking: `CreateViewport(Vec4 viewport [, minDepth,
  maxDepth [, depthRange]])`, `PickMatrix`, and `Project` / `UnProject`
  (`viewport` is `(x, y, width, height)`).
- `TransformPoint` (w-divide), `TransformVector` (no translation),
  `ExtractScale()`, `WithoutTranslation()`.
- Quaternion rotation: `CreateRotation(Quat [, center])`, `Rotate(value,
  Quat)`; extraction is on `Quat.CreateFromRotationMatrix`.
- Explicit conversions with `System.Numerics.Matrix4x4` (float only).

### `Mat3` — 2D-affine 3x3

Transform members are suffixed `2D` and operate on `Vec2`:
`CreateTranslation2D`, `CreateScale2D`, `CreateRotation2D`,
`CreateSkew2D`, `CreateShearX2D` / `CreateShearY2D` (each with `Xxx2D`
post-multiply appliers and optional centers), settable `Translation2D`,
`TransformPoint2D`, `TransformVector2D`. Quaternion rotation exists
without a center overload (`CreateRotation(Quat)`, `Rotate`). No
projection, viewport, or `System.Numerics` members.

### `Mat3x2` — compact 2D affine

The storage-compact affine transform (translation in `Column2`). Being
non-square its identity is named `AffineIdentity`, and it is the only
non-square shape with algebra: linear-part `Determinant`, `Inverted` /
`Invert` / `TryInvert`, plus `TransformPoint` / `TransformVector` and
**unsuffixed** 2D factories (`CreateTranslation`, `CreateScale`,
`CreateRotation`, `CreateSkew`, with appliers and centers, settable
`Translation`). `Mat3x2 * Mat3x2` composes via the special-cased
`Compose`. Explicit conversions with `System.Numerics.Matrix3x2` (float
only).

### Non-square shapes

`Mat2x3`, `Mat2x4`, `Mat3x4`, `Mat4x2`, `Mat4x3` carry only the common
surface — no identity, determinant, inverse, transform factories, or
System interop. Composition still follows the shape law
(`Mat3x4 * Mat4x3 -> Mat4`... transposing returns the mirrored shape).

## Quaternions

`Quat` (float) and `Quatd`. Fields `X`, `Y`, `Z`, `W` with `W` the scalar
part stored last; grouped access via settable `Vector` (`Vec3`) and
`Scalar` properties. Constants `Zero` and `Identity`.

- Operators: Hamilton `*` (quat∘quat), `Quat * Vec3` rotates the vector,
  `+ - /` and scalar forms, component-wise ordering operators returning
  `Vec4b`, scalar `==`/`!=`. Named `Add`/`Subtract`/`Multiply`/`Divide`/
  `Negate` mirrors exist.
- Construction: `CreateFromAxisAngle`, `CreateFromEulerAngles(Vec3)`,
  `CreateFromYawPitchRoll`, `CreateFromRotationMatrix(Mat3|Mat4)`,
  `LookRotation(direction, up [, handedness])`,
  `CreateRotationBetween(from, to)`.
- Decomposition: `Angle`, `Axis`, `EulerAngles`, `Pitch`, `Yaw`, `Roll`,
  `ToAxisAngle(out axis, out radians)`, `ToMat3()`, `ToMat4()`, static
  `TransformVector`.
- Normalization and inversion: `Length`/`LengthSquared`, `Normalized`,
  `NormalizedOrIdentity`, `NormalizedOr`, `TryNormalize`, `Conjugated` /
  `Conjugate`, `Inverted` / `Invert` / `TryInvert`, `Dot`,
  `IsIdentity(epsilon)`, `IsNormalized(epsilon)`.
- Interpolation and exponential: `Lerp`, `Nlerp`, `Slerp` (plus an
  `extraSpins` overload), `Squad` with `CreateSquadControlPoint`, `Exp`,
  `Log`, `Pow`, `Sqrt`.
- Classification masks `IsNaN` / `IsInfinity` / `IsFinite` (`Vec4b`).
- `Quat` converts **implicitly both ways** with
  `System.Numerics.Quaternion` (`Quatd` has no System interop).

## Boxes

`Box2`, `Box3` (`float`), `Box2d`, `Box3d`, `Box2i`, `Box3i`.
Axis-aligned, represented as public `Min` / `Max` corner fields.

- `Empty` sentinel (inverted bounds: ±infinity for floats,
  `int.MaxValue`/`int.MinValue` for ints); `IsEmpty`.
- Factories: `Create`, `CreateFromCorners` (sorts),
  `CreateFromCenterHalfSize`, `CreateFromCenterSize`.
- Settable derived properties `Size`, `Center`, `HalfSize` (integer boxes
  truncate); `Width`, `Height`, plus `Depth` and `Volume` (3D) or `Area`
  (2D); `Normalized` / `Normalize`.
- Containment triads for points and boxes: `Contains` (alias of
  `ContainsInclusive`), `ContainsInclusive`, `ContainsHalfOpen`,
  `ContainsExclusive`; `Intersects` / `IntersectsExclusive` plus a static
  `Intersects`.
- `ClosestPoint`, `DistanceSquaredTo`, `DistanceTo` (integer boxes return
  `int` squared distance but `float` distance).
- Mutation pairs (immutable + in-place): `Inflated`/`Inflate`,
  `Translated`/`Translate`, `Scaled`/`Scale` (optional anchor); growth:
  `Including(point|box)`, static `Union`, static `Intersection`.
- `Box3` only: `Contains(Sphere3)`, `Intersects(Sphere3)`,
  `Intersects(Segment3)`. Integer boxes have no sphere/segment overloads
  and no cell iteration.
- Conversions: float→double implicit, float→int explicit, int→float and
  int→double implicit.
- There is no matrix transform on a box; transforming an AABB goes through
  `Obb3.Transform(Box3, Mat4)`.

## Planes

`Plane3`, `Plane3d`. Fields `Normal` (`Vec3`) and `Offset` (`float`), with
the plane equation `dot(Normal, point) + Offset`; `Coefficients` exposes
`(Normal, Offset)` as a settable `Vec4`.

- Factories: from normal+offset, `Vec4` coefficients, span,
  `CreateFromPointNormal`, `CreateFromPoints(p0, p1, p2)` (+`Try` form).
- Normalization: `NormalLength(Squared)`, `Normalized`, `NormalizedOr`,
  `TryNormalize`, `IsNormalized(epsilon)`, `Flipped` / `Flip`, unary `-`.
- Point queries: `Evaluate`, `Classify` → `PlaneIntersectionKind`
  (`Negative`/`Intersecting`/`Positive`), `Dot(Vec4)`, `DotNormal`,
  `SignedDistanceTo`, `DistanceTo`, `ClosestPoint`, `ProjectPoint`,
  `ReflectPoint`.
- Volume classification: `Classify(Box3 | Sphere3 | Obb3)` →
  `PlaneIntersectionKind`.
- `Transform(plane, Mat4)` (+`Try`; throws on singular matrices) and
  `Transform(plane, Quat)`.
- `Plane3` converts implicitly both ways with `System.Numerics.Plane`.

## Frustums

`Frustum3`, `Frustum3d`. Six inward-facing `Plane3` fields — `Left`,
`Right`, `Bottom`, `Top`, `Near`, `Far` (also the canonical span order);
`PlaneCount = 6`, `CornerCount = 8`.

- Factories: `Create(6 planes)`, `CreateFromPlanes(span)` (+`Try`),
  `CreateFromClipTransform(Mat4 [, ProjectionDepthRange])` (+`Try`
  forms).
- Bulk access: `CopyPlanesTo` / `TryCopyPlanesTo` /
  `TryCopyNormalizedPlanesTo`, `CopyCornersTo` / `TryCopyCornersTo`,
  `HasFiniteCorners`, `TryCreateBoundingBox(out Box3)`.
- The three-tier query pattern per target — `Contains` (fully inside),
  `Intersects`, and `Classify` → `ContainmentKind`
  (`Disjoint`/`Intersects`/`Contains`) — against `Box3`, `Sphere3`,
  `Obb3`, `Capsule3`, and other `Frustum3`s (frustum-vs-frustum adds
  `TryClassify` for infinite-corner cases), plus point `Contains(Vec3)`.
  Boxes additionally get `IntersectsPrecise` / `ClassifyPrecise`, which
  eliminate the classic conservative false positives.
- `Transform(frustum, Mat4)` (+`Try`) with inverse-transpose semantics.

## Spheres

`Sphere3`, `Sphere3d`. Fields `Center`, `Radius`; `Empty` (radius -1),
`IsEmpty`, `Diameter`, `RadiusSquared`.

- Factories: `Create`, `CreateFromBox(Box3)`,
  `CreateFromPoints(ReadOnlySpan<Vec3>)` (+`Try`).
- Queries: `Contains(Vec3 | Sphere3)`, `Intersects(Sphere3)` (+static),
  `ClosestPoint`, `DistanceTo`, `DistanceSquaredTo`.
- Query placement is asymmetric by design: sphere-vs-box lives on `Box3`,
  sphere-vs-obb on `Obb3`, sphere-vs-frustum on `Frustum3`, sphere-vs-ray
  on `Ray3`, sphere-vs-capsule on `Capsule3`.

## Capsules

`Capsule3`, `Capsule3d`. Fields `Segment` (`Segment3` centerline) and
`Radius`, with proxy properties `Start` / `End` and derived `Center`,
`Direction`, `Length`, `LengthSquared`, `Diameter`, `RadiusSquared`,
`Empty` / `IsEmpty`.

- Factories: `Create(segment, radius)`, `Create(start, end, radius)`.
- Queries: `PointAt(amount)`, `Contains(Vec3 | Sphere3)`,
  `Intersects(Box3 | Sphere3 | Capsule3 | Plane3 | Frustum3 | Ray3)`,
  `TryIntersect(Ray3, out float distance)`, `Classify(Frustum3)`,
  `ClosestPoint`, `DistanceTo`, `DistanceSquaredTo`.

## Oriented boxes

`Obb3`, `Obb3d`. Fields `Center`, `HalfSize`, `Orientation` (`Quat`);
settable `Size`; `Empty` / `IsEmpty`; `CornerCount = 8`.

- Factories: `Create`, `CreateFromBox(Box3)`, and
  `Transform(Box3, Mat4)` — the entry point for transforming an AABB.
- Queries: `CopyCornersTo` / `TryCopyCornersTo`,
  `TryCreateBoundingBox(out Box3)`, `Contains(Vec3 | Sphere3 | Obb3)`,
  `Intersects(Box3 | Sphere3 | Obb3 | Plane3 | Frustum3)` (obb-vs-obb is
  separating-axis), `ClosestPoint`, `DistanceTo`, `DistanceSquaredTo`.

## Rays

`Ray3`, `Ray3d`. Fields `Origin`, `Direction` (not implicitly
normalized).

- `PointAt(distance)`, `Translated(offset)`, `Normalized()` (a method) and
  `TryNormalize`.
- `Intersects(Plane3 | Box3 | Sphere3 | Frustum3)` and `TryIntersect`
  overloads producing the hit distance — `TryIntersect(Box3, out float)`
  for the nearest hit, `TryIntersect(Box3, out Intervalf)` for the
  entry/exit range, `TryIntersect(Plane3 | Sphere3, out float)`,
  `TryIntersect(Frustum3, out Intervalf)`. Ray-vs-triangle is on
  `Triangle3`; ray-vs-capsule on `Capsule3`.

## Segments

`Segment3`, `Segment3d`. Fields `Start`, `End`; derived `Center`,
`Direction` (unnormalized), `Length`, `LengthSquared`.

- `PointAt(amount)` (0..1), `ClosestPoint`, `DistanceTo`,
  `DistanceSquaredTo`, `Intersects(Sphere3 | Box3)`, and
  `TryIntersect(Plane3, out float amount)` where `amount` is the 0..1
  parameter along the segment.

## Triangles

`Triangle3`, `Triangle3d`. Fields `A`, `B`, `C`; derived `EdgeAB`,
`EdgeAC`, `EdgeBC`, `UnnormalizedNormal`, `Normal`, `Plane`, `Area`,
`IsDegenerate`, with `TryGetNormal` / `TryGetPlane` for degenerate
safety.

- `Barycentric(point)`, `Contains(point)`,
  `Intersects(Box3 | Sphere3 | Ray3)`,
  `TryIntersect(Ray3, out float distance)`, `ClosestPoint`, `DistanceTo`,
  `DistanceSquaredTo`.

## Quads

`Quad3`, `Quad3d`. Corner fields `TopLeft`, `TopRight`, `BottomLeft`,
`BottomRight`; derived `Center` and `Bounds` (`Box3`). Data plus the
common contract only — no containment, intersection, or transform
members.

## Intervals

`Intervalf`, `Intervald` — the 1D range. Fields `Min`, `Max`; `Empty`
(inverted), `IsEmpty`, `Length`, `Center`.

- `Create`, `CreateFromEndpoints` (sorts), `Contains(scalar | interval)`,
  `Intersects`, static `Union`, static `Intersection` (empty when
  disjoint). The overlap test is `Intersects`; there is no `Clamp` or
  `Lerp` on the interval itself.

## Viewports

`Viewport`, `Viewportd`. Composed of `Bounds` (`Box2` screen rectangle)
and `Depth` (`Intervalf`, defaulting to 0..1); derived `Size` and
`Center`.

- `ToViewportVector()` — the `(x, y, width, height)` `Vec4` used by
  `Mat4` viewport/projection helpers.
- `CreateTransform([depthRange])`, `Project(source, clipFromSource
  [, depthRange])`, `UnProject(screen, sourceFromClip [, depthRange])`,
  `CreatePickRay(screen, worldFromClip [, depthRange])`,
  `CreatePickMatrix(Box2 selection)`. Overloads without a
  `ProjectionDepthRange` default to `NegativeOneToOne` (OpenGL).

## ScalarMath and generic programming

`ScalarMath` provides generic scalar functions usable over any supported
scalar in generic maths code: `Min`, `Max`, `Clamp`, `Abs`, `Lerp`,
`Barycentric`, `Saturate`, `Floor`, `Ceiling`, `Round`
(+`MidpointRounding`), `Truncate`, `FractionalPart`, `Modulo`, `Mod`,
`Step`, `SmoothStep`, `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`,
`Atan2`, `Exp`, `Log`, `Log2`, `Sqrt`, `InverseSqrt`, `Pow`,
`FusedMultiplyAdd`, `IsNaN`, `IsInfinity`, `IsFinite`, `BitCount`,
`LeadingZeroCount`, `TrailingZeroCount`, `FindLeastSignificantBit`,
`FindMostSignificantBit`, `IsPowerOfTwo`, and `Select`.

Generic algorithms constrain on the hand-authored interface families
rather than concrete types:

- Vector capability bundles per dimension: `IVec{2,3,4}Floating`,
  `IVec{2,3,4}SignedInteger`, `IVec{2,3,4}UnsignedInteger`,
  `IVec{2,3,4}Mask`, composed from `IVec<TSelf, TScalar>` (component
  count, indexer, spans, parsing) plus `IVecFloating`, `IVecInteger`,
  `IVecSignedNumeric`, `IVecMetric`, `IVec3Cross` / `IVec2Planar`,
  `IVec*Axes`, and the operator interfaces. In the integer bundles,
  `TCount` is the bit-count result family (always the `int` family),
  `TLength` the `Length`/`Distance` scalar (always `float`), and
  `TArithmetic` the component-wise arithmetic result (the `int` family
  for the narrow types, `TSelf` otherwise).
- Matrix interfaces per shape (`IMat2`..`IMat4x3`) plus capability
  add-ons: `IMatSquare`, `IMatQuery`, `IMat4Transform`,
  `IMat3Transform2D`, `IMat3x2Transform2D`, `IMat{3,4}QuaternionRotation`,
  `IMat4PlaneTransform`, `IMat{3x2,4}SystemNumerics`.
- Quaternion, box, and geometry interfaces: `IQuat`,
  `IQuatInterpolation`, `IQuatRotation`, `IQuatSystemNumerics`, `IBox`
  / `IBox2` / `IBox3` / `IBox3Sphere`, and one `I<Family>3` per geometry
  family (with `*Sphere`, `*Transform`, `*SystemNumerics` add-ons).
