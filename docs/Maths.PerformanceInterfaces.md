# Maths Performance Interfaces

## Scope and counting rule

This appendix closes the public generic-interface side of the
[Maths Performance Surface Manifest](Maths.PerformanceSurfaceManifest.md). The
audited source contains 73 core public interfaces and 12 generated
marker/composition interfaces. Their declarations occupy 661 declaration lines
and expose 299 unique member names. Overloads remain separate below whenever
their operands, result, failure behavior, arity, or implementation shape differ.

For compactness, signatures omit the repeated `public`, `static abstract`, and
accessor keywords where the member shape is still unambiguous. `get/set` is
shown for mutable properties, and every parameter and result type that
distinguishes an overload is retained. Inherited members are recorded only at
the interface that declares them.

The linked manifest row is the status authority. “Authored/unexecuted” means a
closed generic-interface benchmark caller exists and builds, but no
BenchmarkDotNet result has been recorded. It is inventory evidence, not measured
performance evidence. “No dedicated caller” means concrete implementations may
have benchmark coverage while interface dispatch itself remains unexecuted.

## Generic-dispatch coverage

| Interface family | Representative closed generic callers | Evidence state | Member-specific dispatch still missing |
| --- | --- | --- | --- |
| Vector static dispatch | Vec2/3/4 `Create` for representative float/double/Int32 shapes in [`GenericStaticDispatchBenchmarks`](../demos/AlvorKit.Maths.Demo.Bench/GenericStaticDispatchBenchmarks.cs) | Authored and built; BDN not executed | Other construction, access/copy, axes, conversions, and member-specific dispatch |
| Remaining integer vectors (`sbyte`, `byte`, `short`, `ushort`, `Int128`, `UInt128`; Vec2/3/4) | Arithmetic, bitwise, scalar-count shifts, Min/Max/Clamp, Dot, selected relations, and bit counts in [`RemainingIntegerVectorBenchmarks`](../demos/AlvorKit.Maths.Demo.Bench/RemainingIntegerVectorBenchmarks.cs) | Authored and built; BDN not executed | Construction/access/copy, vector-count shifts, many relation/operator directions, conversions, and other members |
| Half vectors (Vec2/3/4) | Arithmetic, bounds, Dot, Lerp, FMA, selected rounding/functions, relations, and classification in [`HalfVectorBenchmarks`](../demos/AlvorKit.Maths.Demo.Bench/HalfVectorBenchmarks.cs) | Authored and built; BDN not executed | Construction/access/copy, geometry branches, most transcendental/scalar overloads, conversions, and mask operations |
| Interval, segment, ray, capsule, triangle, OBB, and frustum (`float`/`double`) | One representative member per family in [`CompoundGenericDispatchBenchmarks`](../demos/AlvorKit.Maths.Demo.Bench/CompoundGenericDispatchBenchmarks.cs) | Authored and built; BDN not executed | Every other declared member and overload |
| Box, plane, sphere, and quad | Box `Union`, plane `Create`, sphere static `Intersects`, and quad `Create` for representative scalar families in `GenericStaticDispatchBenchmarks` | Authored and built; BDN not executed | Every other declared member and overload |
| Matrices | Mat4/Mat4d `IMat.CreateDiagonal` in `GenericStaticDispatchBenchmarks` | Authored and built; BDN not executed | Every other `IMat*` member and matrix shape |
| Quaternions | Quat/Quatd `IQuatInterpolation.Lerp` in `GenericStaticDispatchBenchmarks` | Authored and built; BDN not executed | Every other `IQuat*` member |
| Viewport | Generic `IEquatable<T>.Equals` only | Authored and built; BDN not executed | Not part of this appendix: Viewport has no custom public maths interface |

## Vector interfaces

### `IVec<TSelf,TScalar>`

- Layout: `int ComponentCount`, `int SizeInBytes` → [V-C01/V-C04](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Construction: `TSelf Create(TScalar)`; `TSelf Create(ReadOnlySpan<TScalar>)` → [V-C02/V-C07](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Access: `ref TScalar ComponentRef(ref TSelf,int)`; `TScalar this[int] { get/set; }` → [V-C05](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Copy: `void CopyTo(TScalar[])`; `void CopyTo(TScalar[],int)`; `void CopyTo(Span<TScalar>)`; `bool TryCopyTo(Span<TScalar>)` → [V-C08a-d](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Dispatch: representative static `Create` plus Half/remaining-integer callers are authored/unexecuted; other members remain uncovered → [V-C15](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).

### `IVec2<TSelf,TScalar>`

- `TSelf Create(TScalar x,TScalar y)` → [V-C02](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Tuple operators `TSelf((TScalar X,TScalar Y))`, `(TScalar X,TScalar Y)(TSelf)` → [V-C10](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- `void Deconstruct(out TScalar x,out TScalar y)` → [V-C06](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Dispatch: float `Create` is authored/unexecuted; other members remain uncovered.

### `IVec3<TSelf,TScalar>`

- `TSelf Create(TScalar x,TScalar y,TScalar z)` → [V-C02](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Tuple operators `TSelf((TScalar X,TScalar Y,TScalar Z))`, `(TScalar X,TScalar Y,TScalar Z)(TSelf)` → [V-C10](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- `void Deconstruct(out TScalar x,out TScalar y,out TScalar z)` → [V-C06](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Dispatch: representative float/double/Int32 `Create` callers are authored/unexecuted; other members remain uncovered.

### `IVec4<TSelf,TScalar>`

- `TSelf Create(TScalar x,TScalar y,TScalar z,TScalar w)` → [V-C02](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Tuple operators `TSelf((TScalar X,TScalar Y,TScalar Z,TScalar W))`, `(TScalar X,TScalar Y,TScalar Z,TScalar W)(TSelf)` → [V-C10](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- `void Deconstruct(out TScalar x,out TScalar y,out TScalar z,out TScalar w)` → [V-C06](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Dispatch: float `Create` is authored/unexecuted; other members remain uncovered.

### `IVec2Axes<TSelf>`

- `TSelf UnitX`, `TSelf UnitY` → [V-C03/V-C04](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points). Dispatch: no dedicated caller.

### `IVec3Axes<TSelf>`

- `TSelf UnitZ` → [V-C03/V-C04](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points). Dispatch: no dedicated caller.

### `IVec4Axes<TSelf>`

- `TSelf UnitW` → [V-C03/V-C04](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points). Dispatch: no dedicated caller.

### `IVec2FloatingToInteger<TInteger>`

- `TInteger TruncateToVec2i()`, `FloorToVec2i()`, `CeilingToVec2i()`, `RoundToVec2i()` → [V-F14a-d](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points). Dispatch: no dedicated caller.

### `IVec3FloatingToInteger<TInteger>`

- `TInteger TruncateToVec3i()`, `FloorToVec3i()`, `CeilingToVec3i()`, `RoundToVec3i()` → [V-F14a-d](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points). Dispatch: no dedicated caller.

### `IVec4FloatingToInteger<TInteger>`

- `TInteger TruncateToVec4i()`, `FloorToVec4i()`, `CeilingToVec4i()`, `RoundToVec4i()` → [V-F14a-d](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points). Dispatch: no dedicated caller.

### `IVec2Planar<TSelf,TScalar>`

- Properties `TSelf PerpendicularLeft`, `TSelf PerpendicularRight` → [V-N06a](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points).
- `TScalar Cross(TSelf,TSelf)`; `TScalar PerpDot(TSelf,TSelf)` → [V-N06b/c](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points). Dispatch: no dedicated caller.

### `IVec3Cross<TSelf,TScalar>`

- `TSelf Cross(TSelf,TSelf)` → [V-N07](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points). Dispatch: no dedicated caller.

### `IVec2SystemNumerics<TSelf>`

- Implicit `TSelf(System.Numerics.Vector2)` and `System.Numerics.Vector2(TSelf)` → [V-C13](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points). Dispatch: no dedicated caller.

### `IVec3SystemNumerics<TSelf>`

- Implicit `TSelf(System.Numerics.Vector3)` and `System.Numerics.Vector3(TSelf)` → [V-C13](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points). Dispatch: no dedicated caller.

### `IVec4SystemNumerics<TSelf>`

- Implicit `TSelf(System.Numerics.Vector4)` and `System.Numerics.Vector4(TSelf)` → [V-C13](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points). Dispatch: no dedicated caller.

### `IVecFloating<TSelf,TScalar,TMask>`

- Constants `TSelf PositiveInfinity`, `NegativeInfinity`, `NaN`, `Epsilon` → [V-C03/V-C04](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- Clamp/rounding `Saturate(TSelf)`, `Floor(TSelf)`, `Ceiling(TSelf)`, `Round(TSelf)`, `Round(TSelf,MidpointRounding)`, `Truncate(TSelf)`, `FractionalPart(TSelf)` → [V-F05/V-F06a-e/V-F07a](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Mod/step `Modulo(TSelf,TSelf)`, `Mod(TSelf,TSelf)`, `Step(TSelf,TSelf)`, `SmoothStep(TSelf,TSelf,TSelf)` → [V-F07b-c/V-F08a/V-F09](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Unary transcendental `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Exp`, `Log`, `Log2`, `Sqrt`, `InverseSqrt` each `(TSelf)->TSelf` → [V-F10a-i/V-F12a-b](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Binary/ternary math `Atan2(TSelf,TSelf)`, `Pow(TSelf,TSelf)`, `FusedMultiplyAdd(TSelf,TSelf,TSelf)` → [V-F11a-b/V-F13](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Classification `TMask IsNaN(TSelf)`, `IsInfinity(TSelf)`, `IsFinite(TSelf)` → [V-F15a-c](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Dispatch: representative Half members authored/unexecuted; all other member-specific dispatch absent.

### `IVecFloatingGeometry<TSelf,TScalar>`

- Normalization `TSelf Normalized`, `NormalizedOrZero`, `NormalizedOr(TSelf)`, `bool TryNormalize(out TSelf)`, `TSelf Normalize(TSelf)` → [V-F01a-c](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Interpolation `Lerp(TSelf,TSelf,TScalar)`, `Barycentric(TSelf,TSelf,TSelf,TScalar,TScalar)` → [V-F02a/c](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Geometry `Reflect(TSelf,TSelf)`, `FaceForward(TSelf,TSelf,TSelf)`, `Refract(TSelf,TSelf,TScalar)` → [V-F03/V-F04a-b](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- Dispatch: Half `Lerp` is authored/unexecuted; remaining members have no dedicated caller.

### `IVecFloatingScalarFunctions<TSelf,TScalar>`

- `Modulo(TSelf,TScalar)`, `Mod(TSelf,TScalar)` → [V-F07b-c](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- `Step(TScalar,TSelf)`, `SmoothStep(TScalar,TScalar,TSelf)` → [V-F08b/V-F09](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points).
- `Pow(TSelf,TScalar)` → [V-F11c](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points). Dispatch: no dedicated caller.

### `IVecFloatingVectorInterpolation<TSelf>`

- `TSelf Lerp(TSelf,TSelf,TSelf)` → [V-F02b](Maths.PerformanceSurfaceManifest.md#floating-vector-touch-points). Dispatch: no dedicated caller.

### `IVecInteger<TSelf,TScalar,TMask,TCount,TLength,TArithmetic>`

- Scalar-count operators `TArithmetic operator <<`, `>>`, `>>>(TSelf,int)` → [V-O07](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points).
- Counts `TCount BitCount`, `LeadingZeroCount`, `TrailingZeroCount`, `FindLeastSignificantBit`, `FindMostSignificantBit` each `(TSelf)` → [V-I01/V-I02a-b/V-I03a-b](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- `TMask IsPowerOfTwo(TSelf)` → [V-I04](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- Dispatch: these members are authored/unexecuted for remaining integers; other scalar families remain incomplete.

### `IVecIntegerCountShiftOperators<TSelf,TCount,TResult>`

- Vector-count operators `TResult operator <<`, `>>`, `>>>(TSelf,TCount)` → [V-O08a-c](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points). Dispatch: no dedicated caller.

### `IVecMask<TSelf>`

- Constants `TSelf False`, `True` → [V-C03/V-C04](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points); reductions `bool All`, `Any`, `None` → [V-M01a-c](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- Unary operators `TSelf operator !(TSelf)`, `operator ~(TSelf)` → [V-M04a](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- `operator &`, `operator |`, and `operator ^` each have `(TSelf,bool)`, `(bool,TSelf)`, `(TSelf,TSelf)` → [V-M04a](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- Truth operators `bool operator true(TSelf)`, `operator false(TSelf)` → [V-M04b](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- `TSelf Equal(TSelf,TSelf)`, `NotEqual(TSelf,TSelf)` → [V-M02a-b](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- Dispatch: inherited mask result construction appears in Half/integer suites, but these mask members have no dedicated generic caller.

### `IVecMetric<TSelf,TScalar,TLength>`

- Properties `TScalar LengthSquared`, `TLength Length` → [V-N01b/V-N02](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points).
- `TScalar Dot(TSelf,TSelf)`, `DistanceSquared(TSelf,TSelf)`; `TLength Distance(TSelf,TSelf)` → [V-N01a/V-N03a-b](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points).
- Dispatch: Dot is authored/unexecuted for Half/remaining integers; other members are absent.

### `IVecNumeric<TSelf,TScalar,TMask,TLength,TArithmetic>`

- `TSelf Zero`, `One` → [V-C03/V-C04](Maths.PerformanceSurfaceManifest.md#vector-core-and-value-touch-points).
- `TSelf Min(TSelf,TSelf)`, `Max(TSelf,TSelf)`, `Clamp(TSelf,TSelf,TSelf)` → [V-N04a-c](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points).
- Dispatch: these numeric methods are authored/unexecuted for Half/remaining integers.

### `IVecRelationalOperators<TSelf,TMask>`

- Operators `TMask <`, `<=`, `>`, `>=(TSelf,TSelf)` → [V-O09a-b](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points).
- Named comparisons `Equal`, `NotEqual`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual`, each `(TSelf,TSelf)->TMask` → [V-R01a-d](Maths.PerformanceSurfaceManifest.md#integer-relational-and-mask-vector-touch-points).
- Dispatch: selected named comparisons are authored/unexecuted for Half/remaining integers; operators and remaining names are absent.

### `IVecScalarArithmeticOperators<TSelf,TScalar,TResult>`

- Scalar-left operators `TResult +`, `-`, `*`, `/`, `%(TScalar,TSelf)` → [V-O04b/d/f/g](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points).
- `TSelf Clamp(TSelf,TScalar,TScalar)` → [V-N04d](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points). Dispatch: no dedicated caller.

### `IVecScalarIntegerOperators<TSelf,TScalar,TResult>`

- Scalar-left `TResult operator &`, `|`, `^(TScalar,TSelf)` → [V-O06a-c](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points). Dispatch: no dedicated caller.

### `IVecSignedNumeric<TSelf,TScalar,TMask,TLength,TArithmetic>`

- `TSelf Abs(TSelf)` → [V-N05](Maths.PerformanceSurfaceManifest.md#vector-operator-and-numeric-touch-points). Dispatch: Half Abs authored/unexecuted; otherwise absent.

## Matrix interfaces

All links in this section target the manifest’s
[explicit matrix signature map](Maths.PerformanceSurfaceManifest.md#explicit-matrix-signature-map).
Mat4/Mat4d `IMat.CreateDiagonal` direct-versus-generic callers are authored but
unexecuted. Every other `IMat*` member and matrix shape still lacks dedicated
generic-dispatch evidence.

### `IMat<TSelf,TScalar,TColumn,TRow,TTranspose>`

- Layout/constants: `int ColumnCount`, `RowCount`, `ComponentCount`, `SizeInBytes`; `TSelf Zero`.
- Factories/algorithms: `CreateDiagonal(TScalar)`, `CreateOuterProduct(TColumn,TRow)`, `Lerp(TSelf,TSelf,TScalar)`, `FromColumnMajor(ReadOnlySpan<TScalar>)`, `FromRowMajor(ReadOnlySpan<TScalar>)`.
- Access: `ref TColumn ColumnRef(ref TSelf,int)`, `ref TScalar ComponentRef(ref TSelf,int,int)`, `TColumn this[int] { get/set; }`, `TScalar this[int,int] { get/set; }`.
- Algebra: `TTranspose Transpose(TSelf)`, `TSelf ComponentMultiply(TSelf,TSelf)`, `TColumn operator *(TSelf,TRow)`, `TRow operator *(TColumn,TSelf)`, `TTranspose Transposed`.
- Copy: `CopyTo(TScalar[])`, `CopyTo(TScalar[],int)`, `CopyTo(Span<TScalar>)`, `CopyToColumnMajor(Span<TScalar>)`, `CopyToRowMajor(Span<TScalar>)`, `TryCopyTo(Span<TScalar>)`, `TryCopyToColumnMajor(Span<TScalar>)`, `TryCopyToRowMajor(Span<TScalar>)`.

### `IMat2<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn)`; `CreateRows(TRow,TRow)`.

### `IMat2x3<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn)`; `CreateRows(TRow,TRow,TRow)`.

### `IMat2x4<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn)`; `CreateRows(TRow,TRow,TRow,TRow)`.

### `IMat3<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn,TColumn)`; `CreateRows(TRow,TRow,TRow)`.

### `IMat3x2<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn,TColumn)`; `CreateRows(TRow,TRow)`.

### `IMat3x4<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn,TColumn)`; `CreateRows(TRow,TRow,TRow,TRow)`.

### `IMat4<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn,TColumn,TColumn)`; `CreateRows(TRow,TRow,TRow,TRow)`.

### `IMat4x2<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn,TColumn,TColumn)`; `CreateRows(TRow,TRow)`.

### `IMat4x3<TSelf,TScalar,TColumn,TRow,TTranspose>`

- `CreateColumns(TColumn,TColumn,TColumn,TColumn)`; `CreateRows(TRow,TRow,TRow)`.

### `IMatNumeric<TSelf,TScalar,TColumn,TRow,TTranspose>`

- Declares no executable member; it only composes numeric/operator contracts. Own surface: `Ineligible`. Inherited executable members map to their declaring interface rows.

### `IMatQuery<TSelf,TScalar>`

- Static `bool IsNull(TSelf,TScalar)`, `IsIdentity(TSelf,TScalar)`; instance `bool IsNull(TScalar)`, `IsIdentity(TScalar)`.

### `IMatRelationalOperators<TSelf,TScalar,TMask>`

- `TMask Equal(TSelf,TSelf)`, `Equal(TSelf,TSelf,TScalar)`, `Equal(TSelf,TSelf,TSelf)`.
- Matching `NotEqual` overloads with exact, scalar-epsilon, and matrix-epsilon shapes.

### `IMatScalarArithmeticOperators<TSelf,TScalar>`

- Scalar-left `TSelf operator +`, `-`, `*`, `/`, `%(TScalar,TSelf)`.

### `IMatSquare<TSelf,TScalar,TColumn>`

- `TSelf Identity`; static `Invert(TSelf)`, `InverseTranspose(TSelf)`, `Adjugate(TSelf)`, `bool TryInvert(TSelf,out TSelf)`.
- Static `bool IsNormalized(TSelf,TScalar)`, `IsOrthogonal(TSelf,TScalar)`; matching instance forms `(TScalar)`.
- Properties `TScalar Determinant`, `Trace`; `TSelf Inverted`, `InverseTransposed`.

### `IMat3QuaternionRotation<TSelf,TScalar,TVector3,TQuaternion,TMatrix4>`

- `TSelf CreateRotation(TQuaternion)`; `Rotate(TSelf,TQuaternion)`.

### `IMat3Transform2D<TSelf,TScalar,TVector2,TVector3>`

- `TVector2 Translation2D { get/set; }`; `CreateTranslation2D(TVector2)`, `Translate2D(TSelf,TVector2)`.
- Scale overloads `CreateScale2D(TVector2)`, `(TScalar)`, `(TVector2,TVector2 center)`, `(TScalar,TVector2 center)`; `Scale2D(TSelf,TVector2)`.
- Rotation `CreateRotation2D(TScalar)`, `(TScalar,TVector2 center)`; `Rotate2D(TSelf,TScalar)`.
- Skew `CreateSkew2D(TVector2)`, `(TVector2,TVector2 center)`; `Skew2D(TSelf,TVector2)`.
- Shear `CreateShearX2D(TScalar)`, `ShearX2D(TSelf,TScalar)`, `CreateShearY2D(TScalar)`, `ShearY2D(TSelf,TScalar)`.
- `TVector2 TransformPoint2D(TSelf,TVector2)`, `TransformVector2D(TSelf,TVector2)`.

### `IMat3x2SystemNumerics<TSelf>`

- Explicit `TSelf(System.Numerics.Matrix3x2)` and `System.Numerics.Matrix3x2(TSelf)`.

### `IMat3x2Transform2D<TSelf,TScalar,TVector2,TVector3,TTranspose>`

- `TSelf AffineIdentity`; `TVector2 Translation { get/set; }`; `TScalar Determinant`; `TSelf Inverted`.
- Translation `CreateTranslation(TVector2)`, `Translate(TSelf,TVector2)`.
- Scale `CreateScale(TVector2)`, `(TScalar)`, `(TVector2,TVector2 center)`, `(TScalar,TVector2 center)`; `Scale(TSelf,TVector2)`.
- Rotation `CreateRotation(TScalar)`, `(TScalar,TVector2 center)`; `Rotate(TSelf,TScalar)`.
- Skew `CreateSkew(TVector2)`, `(TVector2,TVector2 center)`; `Skew(TSelf,TVector2)`.
- Composition/inversion `Compose(TSelf,TSelf)`, `Invert(TSelf)`, `TryInvert(TSelf,out TSelf)`, `operator *(TSelf,TSelf)`.
- `TVector2 TransformPoint(TSelf,TVector2)`, `TransformVector(TSelf,TVector2)`.

### `IMat4PlaneTransform<TSelf,TScalar,TVector3,TVector4,TPlane3>`

- `TSelf CreateReflection(TPlane3)`; `CreateShadow(TVector3,TPlane3)`.

### `IMat4QuaternionRotation<TSelf,TScalar,TVector3,TVector4,TQuaternion,TMatrix3>`

- `TSelf CreateRotation(TQuaternion)`, `CreateRotation(TQuaternion,TVector3 center)`, `Rotate(TSelf,TQuaternion)`.

### `IMat4SystemNumerics<TSelf>`

- Explicit `TSelf(System.Numerics.Matrix4x4)` and `System.Numerics.Matrix4x4(TSelf)`.

### `IMat4Transform<TSelf,TScalar,TVector2,TVector3,TVector4>`

- Translation/scale: `TVector3 Translation { get/set; }`; `CreateTranslation(TVector3)`, `Translate(TSelf,TVector3)`; `CreateScale(TVector3)`, `(TScalar)`, `(TVector3,TVector3 center)`, `(TScalar,TVector3 center)`; `Scale(TSelf,TVector3)`, `Scale(TSelf,TScalar)`.
- Axis rotation: `CreateRotationX(TScalar)`, `CreateRotationY(TScalar)`, `CreateRotationZ(TScalar)` and each `(TScalar,TVector3 center)`; `CreateRotation(TScalar,TVector3 axis)` and center overload; `Rotate(TSelf,TScalar,TVector3)`, `RotateX(TSelf,TScalar)`, `RotateY(TSelf,TScalar)`, `RotateZ(TSelf,TScalar)`.
- Shear/scale-bias: `CreateShear(TVector3,TVector2,TVector2,TVector2)`, `CreateShear(TVector2,TVector2,TVector2)`, `Shear(TSelf,TVector3,TVector2,TVector2,TVector2)`, `CreateScaleBias(TScalar,TScalar)`, `ScaleBias(TSelf,TScalar,TScalar)`.
- View/world: `LookAt(TVector3,TVector3,TVector3)` plus handedness overload; `LookTo(TVector3,TVector3,TVector3)` plus handedness overload; `CreateWorld(TVector3,TVector3,TVector3)`.
- Extraction/transform: `TVector3 ExtractScale()`, `TSelf WithoutTranslation()`, `TVector3 TransformPoint(TSelf,TVector3)`, `TransformVector(TSelf,TVector3)`.
- Perspective FOV: four `CreatePerspectiveFieldOfView` shapes: `(fovY,aspect,near,far)`, that shape plus handedness/depth, `(fov,width,height,near,far)`, and that shape plus handedness/depth.
- Frustum/off-center: `CreateFrustum(left,right,bottom,top,near,far)` and handedness/depth overload; matching `CreatePerspectiveOffCenter` pair.
- Perspective: `CreatePerspective(width,height,near,far)` and handedness/depth overload; `CreateInfinitePerspective(fovY,aspect,near)` and handedness/depth overload.
- `CreateTweakedInfinitePerspective` overloads: `(fovY,aspect,near,epsilon)`, `(fovY,aspect,near)`, and both corresponding handedness/depth shapes.
- Orthographic: `CreateOrthographicOffCenter(left,right,bottom,top,near,far)` and handedness/depth overload; `CreateOrthographic(width,height,near,far)` and handedness/depth overload.
- Projection: `Project(TVector3,TSelf model,TSelf projection,TVector4 viewport)` and depth overload; matching `UnProject` pair.
- Viewport/picking: `PickMatrix(TVector2,TVector2,TVector4)`; `CreateViewport(TVector4)`, `(TVector4,TScalar minDepth,TScalar maxDepth)`, and that shape plus `ProjectionDepthRange`.

## Quaternion interfaces

All links in this section target the manifest’s
[explicit quaternion signature map](Maths.PerformanceSurfaceManifest.md#explicit-quaternion-signature-map).
Quat/Quatd `IQuatInterpolation.Lerp` direct-versus-generic callers are authored
but unexecuted. Every other `IQuat*` member still lacks dedicated generic-dispatch
evidence.

### `IQuat<TSelf,TScalar,TVector3,TVector4,TMask,TMatrix3,TMatrix4>`

- Layout/constants: `int ComponentCount`, `SizeInBytes`; `TSelf Zero`, `Identity`; `TVector3 Vector { get/set; }`; `TScalar Scalar { get/set; }`.
- Metrics/fallbacks: `TScalar LengthSquared`, `Length`; `TSelf Normalized`, `NormalizedOrIdentity`, `NormalizedOr(TSelf)`, `Conjugated`, `Inverted`; `bool TryNormalize(out TSelf)`.
- Creation/access: `Create(TScalar x,TScalar y,TScalar z,TScalar w)`, `Create(TVector3,TScalar)`, `Create(ReadOnlySpan<TScalar>)`; `ref TScalar ComponentRef(ref TSelf,int)`; `TScalar this[int] { get/set; }`.
- Copy: `CopyTo(TScalar[])`, `CopyTo(TScalar[],int)`, `CopyTo(Span<TScalar>)`, `bool TryCopyTo(Span<TScalar>)`.
- Core algebra: `Normalize(TSelf)`, `TScalar Dot(TSelf,TSelf)`, `Conjugate(TSelf)`, `Invert(TSelf)`, `bool TryInvert(TSelf,out TSelf)`.
- Queries: `bool IsIdentity(TSelf,TScalar)`, `IsNormalized(TSelf,TScalar)`; `TMask IsNaN(TSelf)`, `IsInfinity(TSelf)`, `IsFinite(TSelf)`.
- Relations: `TMask Equal(TSelf,TSelf)`, `Equal(TSelf,TSelf,TScalar)`, `NotEqual(TSelf,TSelf)`, `NotEqual(TSelf,TSelf,TScalar)`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual` each `(TSelf,TSelf)`.
- Relational operators: `TMask operator <`, `<=`, `>`, `>=(TSelf,TSelf)`.
- Scalar arithmetic: `TSelf operator *(TSelf,TScalar)`, `*(TScalar,TSelf)`, `/(TSelf,TScalar)`, `+(TSelf,TScalar)`, `+(TScalar,TSelf)`, `-(TSelf,TScalar)`, `-(TScalar,TSelf)`.

### `IQuatInterpolation<TSelf,TScalar>`

- Linear/spherical: `Lerp(TSelf,TSelf,TScalar)`, `Nlerp(TSelf,TSelf,TScalar)`, `Slerp(TSelf,TSelf,TScalar)`, `Slerp(TSelf,TSelf,TScalar,int extraSpins)`.
- Squad: `Squad(TSelf from,TSelf to,TSelf fromControl,TSelf toControl,TScalar)`; `CreateSquadControlPoint(TSelf previous,TSelf current,TSelf next)`.
- Quaternion functions: `Exp(TSelf)`, `Log(TSelf)`, `Pow(TSelf,TScalar)`, `Sqrt(TSelf)`.

### `IQuatRotation<TSelf,TScalar,TVector3,TMatrix3,TMatrix4>`

- Properties `TScalar Angle`, `Pitch`, `Yaw`, `Roll`; `TVector3 Axis`, `EulerAngles`.
- Factories `CreateFromAxisAngle(TVector3,TScalar)`, `CreateFromEulerAngles(TVector3)`, `CreateFromYawPitchRoll(TScalar,TScalar,TScalar)`, `CreateFromRotationMatrix(TMatrix3)`, and `CreateFromRotationMatrix(TMatrix4)`.
- Direction helpers `LookRotation(TVector3,TVector3)` plus handedness overload; `CreateRotationBetween(TVector3,TVector3)`.
- Conversion/transform `ToAxisAngle(out TVector3,out TScalar)`, `TMatrix3 ToMat3()`, `TMatrix4 ToMat4()`, `TVector3 TransformVector(TSelf,TVector3)`, `TVector3 operator *(TSelf,TVector3)`.

### `IQuatSystemNumerics<TSelf>`

- Implicit `System.Numerics.Quaternion(TSelf)` and `TSelf(System.Numerics.Quaternion)`.

## Compound geometry interfaces

Infrastructure groups below link to
[Compound Value Infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure),
and algorithms link to the
[Explicit Compound Geometry Signature Map](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).

### `IBox<TSelf,TScalar,TVector>`

- Layout/state: `int Dimension`, `ComponentCount`; `TSelf Empty`; `TVector Min`, `Max`, `Size`, `Center`, `HalfSize { get/set; }`; `bool IsEmpty` → [infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factories: `Create(TVector,TVector)`, `Create(ReadOnlySpan<TScalar>)`, `CreateFromCorners(TVector,TVector)`, `CreateFromCenterHalfSize(TVector,TVector)`, `CreateFromCenterSize(TVector,TVector)` → [box construction](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Access/copy: `ref TScalar ComponentRef(ref TSelf,int)`, `TScalar this[int] { get/set; }`, `CopyTo(Span<TScalar>)`, `bool TryCopyTo(Span<TScalar>)` → [infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Normalize/contain: `TSelf Normalized`; point and box overloads of `Contains`, `ContainsHalfOpen`, `ContainsInclusive`, `ContainsExclusive` → [box policies](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Geometry: instance `Intersects(TSelf)`, `ClosestPoint(TVector)`; static `Union(TSelf,TSelf)`, `Intersection(TSelf,TSelf)`, `bool Intersects(TSelf,TSelf)` → [box algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: `Union` for float/double/Int32 Box3 is authored/unexecuted; all other members remain uncovered.

### `IBox2<TSelf,TScalar,TVector>`

- `TScalar Width`, `Height`, `Area` → [box properties](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map). Dispatch: no dedicated caller.

### `IBox3<TSelf,TScalar,TVector>`

- `TScalar Width`, `Height`, `Depth`, `Volume` → [box properties](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map). Dispatch: no dedicated caller.

### `IBox3Sphere<TSelf,TScalar,TVector3,TSphere>`

- `bool Contains(TSphere)`, instance `Intersects(TSphere)`, static `Intersects(TSelf,TSphere)` → [box/sphere algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map). Dispatch: no dedicated caller.

### `IInterval<TSelf,TScalar>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; `TSelf Empty`; `TScalar Min`, `Max { get/set; }`; `bool IsEmpty`; `TScalar Length`, `Center` → [interval infrastructure/properties](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factories/access/copy: `Create(TScalar,TScalar)`, `CreateFromEndpoints(TScalar,TScalar)`, `Create(ReadOnlySpan<TScalar>)`, `ref TScalar ComponentRef(ref TSelf,int)`, indexer, `CopyTo(Span<TScalar>)`, `TryCopyTo(Span<TScalar>)` → [interval factories/infrastructure](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Algorithms: `Contains(TScalar)`, `Contains(TSelf)`, `Intersects(TSelf)`, static `Union(TSelf,TSelf)`, `Intersection(TSelf,TSelf)` → [interval algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: `Length` for float/double is authored/unexecuted; every other member lacks dedicated dispatch.

### `IPlane3<TSelf,TScalar,TVector3,TVector4>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; `TSelf Zero`; `TVector3 Normal { get/set; }`; `TScalar Offset { get/set; }`; `TVector4 Coefficients { get/set; }`; `TScalar NormalLengthSquared`, `NormalLength` → [plane infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Normalization/flip: `TSelf Normalized`, `NormalizedOr(TSelf)`, `Flipped`; `bool TryNormalize(out TSelf)`; static `Normalize(TSelf)`, `TryNormalize(TSelf,out TSelf)`, `Flip(TSelf)` → [plane normalize/flip](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Factories: `Create(TVector3,TScalar)`, `Create(TVector4)`, `Create(ReadOnlySpan<TScalar>)`, `CreateFromPointNormal(TVector3,TVector3)`, throwing `CreateFromPoints(TVector3,TVector3,TVector3)`, Boolean `TryCreateFromPoints(...,out TSelf)` → [plane factories](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Access/copy: `ComponentRef`, indexer, array `CopyTo` with/without index, span `CopyTo`, span `TryCopyTo` → [plane infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Instance evaluation: `Evaluate(TVector3)`, `Classify(TVector3)`, `Dot(TVector4)`, `DotNormal(TVector3)`, `SignedDistanceTo(TVector3)`, `DistanceTo(TVector3)`, `ClosestPoint(TVector3)`, `ProjectPoint(TVector3)`, `ReflectPoint(TVector3)` → [plane algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Static evaluation: `Evaluate(TSelf,TVector3)`, `Classify(TSelf,TVector3)`, `Dot(TSelf,TVector4)`, `DotNormal(TSelf,TVector3)`; static and instance `IsNormalized(...,TScalar epsilon)` → [plane algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: static `Create(normal,offset)` for float/double is authored/unexecuted; all other members remain uncovered.

### `IPlane3SystemNumerics<TSelf>`

- Implicit `System.Numerics.Plane(TSelf)` and `TSelf(System.Numerics.Plane)` → [plane conversions](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map). Dispatch: exact round-trip coverage exists.

### `IPlane3Transform<TSelf,TScalar,TVector3,TVector4,TMatrix4,TQuaternion>`

- Matrix `Transform(TSelf,TMatrix4)` and `TryTransform(TSelf,TMatrix4,out TSelf)`; quaternion `Transform(TSelf,TQuaternion)` → [plane transforms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map). Dispatch: no dedicated caller.

### `ISphere3<TSelf,TScalar,TVector3,TBox3>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; `TSelf Empty`; `TVector3 Center { get/set; }`; `TScalar Radius { get/set; }`, `Diameter`, `RadiusSquared`; `bool IsEmpty` → [sphere infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factories: `Create(TVector3,TScalar)`, `Create(ReadOnlySpan<TScalar>)`, `CreateFromBox(TBox3)`, throwing `CreateFromPoints(ReadOnlySpan<TVector3>)`, Boolean `TryCreateFromPoints(ReadOnlySpan<TVector3>,out TSelf)` → [sphere factories](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Access/copy: `ComponentRef`, indexer, span `CopyTo`, span `TryCopyTo` → [sphere infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Algorithms: `Contains(TVector3)`, `Contains(TSelf)`, instance/static `Intersects(TSelf)`/`Intersects(TSelf,TSelf)`, `ClosestPoint(TVector3)`, `DistanceTo(TVector3)`, `DistanceSquaredTo(TVector3)` → [sphere algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: static sphere `Intersects` for float/double is authored/unexecuted; all other members remain uncovered.

### `ISegment3<TSelf,TScalar,TVector3,TVector4,TPlane3,TBox3,TSphere3>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; `TVector3 Start`, `End { get/set; }`, `Center`, `Direction`; `TScalar Length`, `LengthSquared` → [segment infrastructure/properties](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factory/access/copy: `Create(TVector3,TVector3)`, `Create(ReadOnlySpan<TScalar>)`, `ComponentRef`, indexer, span `CopyTo`, `TryCopyTo` → [segment infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Algorithms: `PointAt(TScalar)`, `ClosestPoint(TVector3)`, `DistanceTo(TVector3)`, `DistanceSquaredTo(TVector3)`, `Intersects(TSphere3)`, `Intersects(TBox3)`, `TryIntersect(TPlane3,out TScalar amount)` → [segment algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: `DistanceSquaredTo` for float/double is authored/unexecuted; every other member lacks dedicated dispatch.

### `IRay3<TSelf,TScalar,TVector3,TVector4,TPlane3,TBox3,TSphere3,TFrustum3,TInterval>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; `TVector3 Origin`, `Direction { get/set; }` → [ray infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factory/access/copy: `Create(TVector3,TVector3)`, `Create(ReadOnlySpan<TScalar>)`, `ComponentRef`, indexer, span `CopyTo`, `TryCopyTo` → [ray infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Motion/normalize: `PointAt(TScalar)`, `Translated(TVector3)`, `Normalized()`, `TryNormalize(out TSelf)` → [ray direct callers](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Boolean intersections: `Intersects(TPlane3)`, `(TBox3)`, `(TSphere3)`, `(TFrustum3)` → [ray Boolean intersections](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- `TryIntersect` overloads: plane/box/sphere nearest `out TScalar`; box/frustum interval `out TInterval` → [ray Try intersections](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: `PointAt` for float/double is authored/unexecuted; every other member lacks dedicated dispatch.

### `ICapsule3<TSelf,TScalar,TVector3,TVector4,TSegment3,TPlane3,TRay3,TBox3,TSphere3,TFrustum3,TInterval>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; `TSelf Empty`; `TSegment3 Segment { get/set; }`; `TScalar Radius { get/set; }`; `TVector3 Start`, `End { get/set; }`, `Center`, `Direction`; `TScalar Length`, `LengthSquared`, `RadiusSquared`; `bool IsEmpty` → [capsule infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factory/access/copy: `Create(TSegment3,TScalar)`, `Create(ReadOnlySpan<TScalar>)`, `ComponentRef`, indexer, span `CopyTo`, `TryCopyTo` → [capsule infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Point/containment: `PointAt(TScalar)`, `Contains(TVector3)`, `Contains(TSphere3)` → [capsule callers](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Intersection/classification: `Intersects(TBox3)`, `(TSphere3)`, `(TSelf)`, `(TPlane3)`, `(TFrustum3)`, `(TRay3)`; `ContainmentKind Classify(TFrustum3)`; `TryIntersect(TRay3,out TScalar)`; static `Intersects(TSelf,TSelf)` → [capsule intersections](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Distance: `ClosestPoint(TVector3)`, `DistanceTo(TVector3)`, `DistanceSquaredTo(TVector3)` → [capsule distance](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: `DistanceSquaredTo` for float/double is authored/unexecuted; every other member lacks dedicated dispatch.

### `ITriangle3<TSelf,TScalar,TVector3,TVector4,TPlane3,TRay3,TBox3,TSphere3,TFrustum3,TInterval>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; `TVector3 A`, `B`, `C { get/set; }`; `EdgeAB`, `EdgeAC`, `EdgeBC`, `UnnormalizedNormal`, `Normal`; `TPlane3 Plane`; `TScalar Area`; `bool IsDegenerate` → [triangle properties](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Factory/access/copy: `Create(TVector3,TVector3,TVector3)`, `Create(ReadOnlySpan<TScalar>)`, `ComponentRef`, indexer, span `CopyTo`, `TryCopyTo` → [triangle infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Normal/plane: `TryGetNormal(out TVector3)`, `TryGetPlane(out TPlane3)` → [triangle failure paths](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Geometry: `Barycentric(TVector3)`, `Contains(TVector3)`, `Intersects(TBox3)`, `(TSphere3)`, `(TRay3)`, `TryIntersect(TRay3,out TScalar)`, `ClosestPoint(TVector3)`, `DistanceTo(TVector3)`, `DistanceSquaredTo(TVector3)` → [triangle algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: `Barycentric` for float/double is authored/unexecuted; every other member lacks dedicated dispatch.

### `IObb3<TSelf,TScalar,TVector3,TVector4,TQuat,TPlane3,TBox3,TSphere3,TFrustum3>`

- Layout/state: `int CornerCount`, `ComponentCount`, `SizeInBytes`; `TSelf Empty`; `TVector3 Center`, `HalfSize`, `Size { get/set; }`; `TQuat Orientation { get/set; }`; `bool IsEmpty` → [OBB infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factories/access/copy: `Create(TVector3,TVector3,TQuat)`, `Create(ReadOnlySpan<TScalar>)`, `CreateFromBox(TBox3)`, `ComponentRef`, indexer, span `CopyTo`/`TryCopyTo`, `CopyCornersTo(Span<TVector3>)`/`TryCopyCornersTo` → [OBB factories/copy](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Containment/intersection: `Contains(TVector3)`, `(TSphere3)`, `(TSelf)`; `Intersects(TBox3)`, `(TSphere3)`, `(TSelf)`, `(TPlane3)`, `(TFrustum3)`; static `Intersects(TSelf,TSelf)` → [OBB algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Distance: `ClosestPoint(TVector3)`, `DistanceTo(TVector3)`, `DistanceSquaredTo(TVector3)` → [OBB distance](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: `DistanceSquaredTo` for float/double is authored/unexecuted; every other member lacks dedicated dispatch.

### `IFrustum3<TSelf,TScalar,TVector3,TVector4,TPlane3,TBox3>`

- Counts/planes: `int PlaneCount`, `CornerCount`, `ComponentCount`; mutable `TPlane3 Left`, `Right`, `Bottom`, `Top`, `Near`, `Far` → [frustum infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factories: six-plane `Create`, throwing `CreateFromPlanes(ReadOnlySpan<TPlane3>)`, Boolean `TryCreateFromPlanes(...,out TSelf)`, scalar-span `Create(ReadOnlySpan<TScalar>)` → [frustum factories](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Access/copy: `ComponentRef`, indexer, scalar `CopyTo`/`TryCopyTo`, plane `CopyPlanesTo`/`TryCopyPlanesTo`, corner `CopyCornersTo`/`TryCopyCornersTo`, `TryCopyNormalizedPlanesTo` → [frustum copy](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Finite bounds: `bool HasFiniteCorners`, `TryCreateBoundingBox(out TBox3)` → [frustum finite-corner callers](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Point/box: `Contains(TVector3)`, `Contains(TBox3)`, `Intersects(TBox3)`, `Classify(TBox3)`, `IntersectsPrecise(TBox3)`, `ClassifyPrecise(TBox3)` → [frustum box algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Frustum/frustum: `Contains(TSelf)`, `Intersects(TSelf)`, `Classify(TSelf)`, `TryClassify(TSelf,out ContainmentKind)` → [frustum algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- Dispatch: point `Contains` for float/double is authored/unexecuted; every other member lacks dedicated dispatch.

### `IFrustum3Sphere<TSelf,TScalar,TVector3,TVector4,TPlane3,TBox3,TSphere3>`

- `bool Contains(TSphere3)`, `Intersects(TSphere3)`; `ContainmentKind Classify(TSphere3)` → [frustum/sphere algorithms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map). Dispatch: no member-specific caller beyond inherited frustum point containment.

### `IFrustum3Transform<TSelf,TScalar,TVector3,TVector4,TMatrix4,TPlane3,TBox3>`

- `CreateFromClipTransform(TMatrix4)` and `(TMatrix4,ProjectionDepthRange)`; `TryCreateFromClipTransform(TMatrix4,ProjectionDepthRange,out TSelf)` → [frustum construction](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map).
- `Transform(TSelf,TMatrix4)`; `TryTransform(TSelf,TMatrix4,out TSelf)` → [frustum transforms](Maths.PerformanceSurfaceManifest.md#explicit-compound-geometry-signature-map). Dispatch: no dedicated caller.

### `IQuad3<TSelf,TScalar,TVector3,TBox3>`

- Layout/state: `int ComponentCount`, `SizeInBytes`; mutable `TVector3 TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`; `TVector3 Center`; `TBox3 Bounds` → [quad infrastructure/properties](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure).
- Factory/access/copy: `Create(TVector3,TVector3,TVector3,TVector3)`, `Create(ReadOnlySpan<TScalar>)`, `ComponentRef`, indexer, span `CopyTo`, `TryCopyTo` → [quad infrastructure](Maths.PerformanceSurfaceManifest.md#compound-value-infrastructure). Dispatch: four-corner `Create` for float/double is authored/unexecuted; all other members remain uncovered.

## Generated marker/composition interfaces

The generated interfaces below declare no executable members. Each is
`Ineligible` for its own performance status: it contributes only inheritance and
generic constraints. Every inherited operation remains mapped at the core
interface that declares it above.

| Dimension | Floating composition | Mask composition | Signed-integer composition | Unsigned-integer composition | Own executable surface |
| --- | --- | --- | --- | --- | --- |
| Vec2 | `IVec2Floating` | `IVec2Mask` | `IVec2SignedInteger` | `IVec2UnsignedInteger` | `Ineligible` — none |
| Vec3 | `IVec3Floating` | `IVec3Mask` | `IVec3SignedInteger` | `IVec3UnsignedInteger` | `Ineligible` — none |
| Vec4 | `IVec4Floating` | `IVec4Mask` | `IVec4SignedInteger` | `IVec4UnsignedInteger` | `Ineligible` — none |

## Closure rule

This appendix proves inventory coverage, not optimization completion. A generic
interface member is “measured” only after a benchmark invokes the member through
the corresponding generic constraint and a BDN result is recorded. Concrete
callers, authored/unexecuted generic callers, inherited marker composition, and
member-specific dispatch evidence must remain distinct in future status updates.
