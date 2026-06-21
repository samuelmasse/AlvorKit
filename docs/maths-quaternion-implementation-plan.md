# Maths Quaternion Implementation Plan

## Goal

Add generated `Quat` and `Quatd` types to `AlvorKit.Maths.Primitives` with the same native C# feel as the generated vector and
matrix families. The public API should make quaternion rotations easy to use with `Vec3`, `Mat3`, and `Mat4`, while still offering
explicit interop with `System.Numerics.Quaternion`.

The implementation should be generated. Hand-authored work belongs in the maths generator, templates, tests, and demo updates. Do not
hand-write checked-in quaternion primitives.

## Naming And Shape

- Generate `Quat` for `float` and `Quatd` for `double`.
- Do not add aliases.
- Store components as `X`, `Y`, `Z`, `W` in sequential layout.
- Treat `(0, 0, 0, 1)` as `Identity` and `(0, 0, 0, 0)` as `Zero`.
- Use radians for all angles.
- Prefer `Create...` factory names and C# property names over GLM names such as `angleAxis`, `mat4_cast`, or `quatLookAt`.
- Prefer enum-selecting overloads for convention differences, such as `LookRotation(direction, up, ProjectionHandedness.Right)`.

## Generated File Targets

Add a quaternion output folder beside `Vec` and `Mat`:

- `out/mathgen/AlvorKit.Maths.Primitives/Quat/IQuat.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/IQuatFloating.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/IQuatRotation.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/IQuatInterpolation.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/IQuatRelationalOperators.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/IQuatQuery.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/IQuatSystemNumerics.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/Quat.g.cs`
- `out/mathgen/AlvorKit.Maths.Primitives/Quat/Quatd.g.cs`

## Task Breakdown

### Task 1: Add Quaternion Model To The Generator

- Add `QuaternionSpec` with scalar, type name, vector type names, matrix type names, and literal helpers.
- Add `QuaternionCatalog` with exactly two entries: `float` and `double`.
- Add `ScalarSpec.QuaternionName()` if useful.
- Add `QuatDirectoryName` to `MathsGenerator`.
- Emit quaternion interfaces before concrete quaternion files.
- Emit concrete quaternion files after vectors and matrices are available in the generated project.

### Task 2: Add Interface Templates

Create interface templates under `res/templates/maths/`:

- `quat-interface.cs.tmpl`
- `quat-floating-interface.cs.tmpl`
- `quat-rotation-interface.cs.tmpl`
- `quat-interpolation-interface.cs.tmpl`
- `quat-relational-operators-interface.cs.tmpl`
- `quat-query-interface.cs.tmpl`
- `quat-system-numerics-interface.cs.tmpl`

Core interface shape:

- `IQuat<TSelf, TScalar, TVector3, TVector4, TMatrix3, TMatrix4>`
- `IQuatFloating<TSelf, TScalar, TVector3>`
- `IQuatRotation<TSelf, TScalar, TVector3, TMatrix3, TMatrix4>`
- `IQuatInterpolation<TSelf, TScalar>`
- `IQuatRelationalOperators<TSelf, TScalar, TMask>`
- `IQuatQuery<TSelf, TScalar>`
- `IQuatSystemNumerics<TSelf>`

Interface inheritance should line up with vectors and matrices:

- `IEquatable<TSelf>`
- `IFormattable`
- `ISpanFormattable`
- `IUtf8SpanFormattable`
- `IParsable<TSelf>`
- `ISpanParsable<TSelf>`
- `IUtf8SpanParsable<TSelf>`
- `IAdditionOperators<TSelf, TSelf, TSelf>`
- `ISubtractionOperators<TSelf, TSelf, TSelf>`
- `IUnaryPlusOperators<TSelf, TSelf>`
- `IUnaryNegationOperators<TSelf, TSelf>`
- `IMultiplyOperators<TSelf, TSelf, TSelf>`
- `IMultiplyOperators<TSelf, TScalar, TSelf>`
- `IDivisionOperators<TSelf, TScalar, TSelf>`
- `IAdditiveIdentity<TSelf, TSelf>`
- `IMultiplicativeIdentity<TSelf, TSelf>`

### Task 3: Add Core Layout And Construction Templates

Create a concrete `quat-file.cs.tmpl` and fragments for:

- Fields `X`, `Y`, `Z`, `W`.
- Constructors from scalar components.
- Constructors from `(TVector3 vector, TScalar scalar)`.
- Static `Create(TScalar x, TScalar y, TScalar z, TScalar w)`.
- Static `Create(TVector3 vector, TScalar scalar)`.
- `Vector` get/set property for `X`, `Y`, `Z`.
- `ComponentCount = 4`.
- `SizeInBytes`.
- Indexer and `ComponentRef`.
- `Zero`, `Identity`, additive identity, multiplicative identity.
- Tuple conversions if they fit the existing vector style.

### Task 4: Reuse Or Mirror Value Semantics And Span Interop

Quaternions should feel like vectors and matrices for basic value behavior:

- Equality and hash code.
- `==` and `!=`.
- `ToString` overloads.
- `TryFormat` for `Span<char>` and `Span<byte>`.
- `Parse` and `TryParse` for string, char span, and UTF-8 span.
- `CopyTo`, `TryCopyTo`, `FromComponents`, and span constructors.
- Column-free component order should be `X`, `Y`, `Z`, `W`.

Prefer sharing generator helpers with vectors where the component count and naming model make that practical. Add quaternion-specific
fragments only when reuse would make the vector generator harder to understand.

### Task 5: Add Arithmetic And Metric Operations

Implement:

- Unary `+` and `-`.
- Component-wise `+` and `-`.
- Quaternion Hamilton product `*`.
- Scalar `*` and `/`.
- Static method aliases: `Add`, `Subtract`, `Multiply`, `Divide`, `Negate`.
- `LengthSquared`.
- `Length`.
- `Dot`.
- `DistanceSquared` and `Distance` only if the interface story stays clean. Otherwise omit distance from quaternion interfaces.

Hamilton product should match `System.Numerics`, GLM, and OpenTK conventions for `X/Y/Z/W` storage.

### Task 6: Add Normalization, Inverse, And Query Operations

Implement:

- `Normalized`.
- `NormalizedOrIdentity`.
- `NormalizedOr(TSelf fallback)`.
- `TryNormalize(out TSelf result)`.
- Static `Normalize(TSelf value)`.
- `Conjugated`.
- Static `Conjugate(TSelf value)`.
- `Inverted`.
- Static `Invert(TSelf value)`.
- `TryInvert(TSelf value, out TSelf result)`.
- `IsIdentity`.
- `IsNormalized(TScalar epsilon)`.
- Component masks: `IsNaN`, `IsInfinity`, `IsFinite`.
- Component-wise `Equal`, `NotEqual`, relational operators, and epsilon equality.

Do not silently turn invalid caller input into a valid rotation in the primary APIs. `TryNormalize` and fallback APIs should model
degenerate cases explicitly.

### Task 7: Add Rotation Factories And Extraction

Implement:

- `CreateFromAxisAngle(TVector3 axis, TScalar radians)`.
- `ToAxisAngle(out TVector3 axis, out TScalar radians)`.
- `Angle`.
- `Axis`.
- `CreateFromEulerAngles(TVector3 radians)`.
- `CreateFromEulerAngles(TScalar pitch, TScalar yaw, TScalar roll)`.
- `CreateFromYawPitchRoll(TScalar yaw, TScalar pitch, TScalar roll)`.
- `EulerAngles`.
- `Pitch`.
- `Yaw`.
- `Roll`.
- `CreateFromRotationMatrix(TMatrix3 matrix)`.
- `CreateFromRotationMatrix(TMatrix4 matrix)`.
- `ToMat3()`.
- `ToMat4()`.
- `LookRotation(TVector3 direction, TVector3 up)`.
- `LookRotation(TVector3 direction, TVector3 up, ProjectionHandedness handedness)`.
- `CreateRotationBetween(TVector3 fromUnitDirection, TVector3 toUnitDirection)`.

Default look rotation should use the same OpenGL-style convention as matrix projection defaults: right-handed.

### Task 8: Add Interpolation And Exponential Helpers

Implement:

- `Lerp(TSelf from, TSelf to, TScalar amount)`.
- `Nlerp(TSelf from, TSelf to, TScalar amount)`.
- `Slerp(TSelf from, TSelf to, TScalar amount)`.
- `Slerp(TSelf from, TSelf to, TScalar amount, int spinCount)` if it stays simple and testable.
- `Squad(TSelf q1, TSelf q2, TSelf s1, TSelf s2, TScalar amount)`.
- `CreateSquadControlPoint(TSelf previous, TSelf current, TSelf next)`.
- `Exp(TSelf value)`.
- `Log(TSelf value)`.
- `Pow(TSelf value, TScalar exponent)`.
- `Sqrt(TSelf value)`.

`Nlerp` should normalize the component lerp. `Slerp` should use the short path by default, matching System and GLM's common behavior.

### Task 9: Add Vector And Matrix Integration

Quaternion APIs:

- `TransformVector(TSelf rotation, TVector3 vector)`.
- Operator `TSelf * TVector3` only if it reads naturally and tests make the order obvious.
- `ToMat3()`.
- `ToMat4()`.

Matrix APIs:

- `Mat3.CreateRotation(Quat rotation)`.
- `Mat3d.CreateRotation(Quatd rotation)`.
- `Mat4.CreateRotation(Quat rotation)`.
- `Mat4d.CreateRotation(Quatd rotation)`.
- `Mat4.CreateRotation(Quat rotation, Vec3 center)`.
- `Mat4d.CreateRotation(Quatd rotation, Vec3d center)`.
- `Mat4.Rotate(Mat4 value, Quat rotation)`.
- `Mat4d.Rotate(Mat4d value, Quatd rotation)`.

Do not add quaternion overloads for rectangular matrices.

### Task 10: Add System.Numerics Interop

For `Quat` only:

- Explicit conversion to `System.Numerics.Quaternion`.
- Explicit conversion from `System.Numerics.Quaternion`.
- `IQuatSystemNumerics<TSelf>` interface.

Do not add `System.Numerics.Quaternion` interop to `Quatd` except explicit narrowing through `Quat` if a later user request needs it.

### Task 11: Regenerate And Review Output

- Run `dotnet run --project scripts/AlvorKit.Script.MathsGen`.
- Confirm generated `Quat` directory file count.
- Review generated diffs for `Quat`, `Quatd`, and matrix files touched by quaternion integration.
- Keep generated output in `out/mathgen`; do not wire bindgen or ordinary builds to regenerate automatically.

### Task 12: Add Generator Tests

Update `tests/AlvorKit.Script.MathsGen.Test/MathsGeneratorTest.cs`:

- Assert the quaternion directory exists.
- Assert expected quaternion interface files exist.
- Assert `Quat.g.cs` and `Quatd.g.cs` exist.
- Assert no non-float quaternion files exist.
- Assert `Quat` has System interop and `Quatd` does not.
- Assert quaternion API surface snippets for core, rotation, interpolation, parsing, formatting, and matrix integration.

### Task 13: Add Runtime Tests

Add or extend maths tests:

- Identity and zero.
- Constructors and `Vector` property.
- Component indexing and span copy.
- Formatting and parse round-trips.
- UTF-8 formatting and parsing.
- `System.Numerics.Quaternion` round-trip for `Quat`.
- Cross-scalar `Quat` and `Quatd` conversion.
- Hamilton product order.
- Axis-angle creation and extraction.
- Euler and yaw-pitch-roll creation.
- Matrix conversion round-trips for `Mat3`, `Mat4`, `Mat3d`, and `Mat4d`.
- Vector rotation.
- Look rotation right-handed and left-handed.
- Rotation between directions, including same and opposite directions.
- `Lerp`, `Nlerp`, `Slerp`, `Squad`.
- `Exp`, `Log`, `Pow`, `Sqrt`.
- Degenerate normalization and inversion behavior.

### Task 14: Update Demo

Expand `demos/AlvorKit.Maths.Demo/Program.cs` with a short quaternion section:

- Build a rotation from axis-angle.
- Rotate a `Vec3`.
- Convert to `Mat4` and compare transform results.
- Interpolate two rotations with `Slerp`.
- Round-trip through `System.Numerics.Quaternion`.
- Print formatted quaternion output.

Do not add demo tests. Build and run the demo after editing.

### Task 15: Verification

Run focused checks:

- `dotnet run --project scripts/AlvorKit.Script.MathsGen`
- `dotnet test tests/AlvorKit.Script.MathsGen.Test/AlvorKit.Script.MathsGen.Test.csproj --no-restore`
- `dotnet test tests/AlvorKit.Maths.Test/AlvorKit.Maths.Test.csproj --no-restore`
- `dotnet run --project demos/AlvorKit.Maths.Demo/AlvorKit.Maths.Demo.csproj`
- Scoped lint for touched generator, template, test, demo, and docs files.
- Focused coverage for `AlvorKit.Script.MathsGen` because generator source changes.

## Deferred Work

- Dual quaternion support.
- Quaternion splines beyond `Squad`.
- Quaternion compression or packing.
- Direct `Plane` integration, because AlvorKit does not have a `Plane` type yet.
- SIMD-specific implementation paths.
- Public generic `Quat<T>`.
- `Half` quaternion.

## Open Decisions Before Implementation

- Whether `Distance` belongs on quaternion interfaces. It is mathematically just component distance, but it is not a common rotation API.
- Whether vector rotation should have an operator, or only `TransformVector`.
- Whether `Normalize` should throw for zero length or return IEEE infinities like vectors do today.
- Whether to include `Slerp` with spin count in the first pass or defer it as an advanced helper.
