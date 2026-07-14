# AlvorKit vectors versus System.Numerics

This audit covers the public single-precision `Vec2`, `Vec3`, and `Vec4`
surface that overlaps .NET's fixed-size `Vector2`, `Vector3`, and `Vector4`.
It records semantic differences separately from performance differences. The
integer, unsigned, double, and half families have no shape-equivalent
`System.Numerics` fixed vector API, so their SIMD implementations use
`Vector64`, `Vector128`, `Vector256`, or scalar code. Floating Min, Max, Clamp,
Saturate, and Abs nevertheless share the regular System vector contract across
float, double, and Half shapes.

## Current result

Float vectors now overlay one private `System.Numerics.Vector2`, `Vector3`, or
`Vector4` view on the existing public component and alias fields. System-backed
operators and functions read and construct through that private intrinsic view.
The public API, all `X/Y/Z/W`, `R/G/B/A`, and `S/T/P/Q` aliases, binary size,
and field offsets are unchanged. `Min`, `Max`, `Clamp`, scalar-bound `Clamp`,
`Saturate`, and `Abs` also use regular `System.Numerics` semantics. The scalar-bound overload
constructs System bounds directly; constructing repository splats and
forwarding to the vector-bound overload made Clamp about 8.4 times slower.

The 4,096-element subtract -> Abs -> Clamp benchmark changed as follows. Times
are nanoseconds per vector, with zero allocation in every case.

| Shape | Original AlvorKit | Current AlvorKit | System.Numerics | Improvement | Current gap |
| --- | ---: | ---: | ---: | ---: | ---: |
| `Vec2` | 9.5586 | 0.9096 | 0.9320 | 10.51x | 2.4% faster |
| `Vec3` | 11.4328 | 1.1210 | 1.1270 | 10.20x | parity |
| `Vec4` | 10.1297 | 1.0200 | 1.0130 | 9.93x | parity |

The latest packed-overlay ShortRun also kept the individual Vec4 leaves at
parity. These sub-nanosecond means are noisy, so they should be read as a
regression check rather than as evidence of a meaningful lead:

| Leaf | AlvorKit | System.Numerics | Result |
| --- | ---: | ---: | --- |
| `Abs` | 0.5009 ns | 0.5929 ns | parity / no regression |
| scalar-bound `Clamp` | 0.6289 ns | 0.6593 ns | parity / no regression |

The former residual gap was a representation boundary, not missing SIMD.
Disassembly showed AlvorKit spilling the 16-byte subtraction result to the
stack and reloading it for Abs, while System kept the value in an XMM register.
Direct `Unsafe.BitCast`, `AggressiveInlining`, `AggressiveOptimization`, and the
combined hints all retained the same 196-byte spill loop.

Representation screening isolated the solution. Removing every alias but
keeping four ordinary sequential float fields still measured 1.1997 ns and
emitted the same 196-byte loop. A sole intrinsic storage field reached 1.0381
ns. Overlaying a private intrinsic field on the existing public aliases removed
the spill while retaining the API and layout; the screened Vec4 loop was 182
bytes, and the generated production Vec2/3/4 pipeline reached System parity.

## Semantics now aligned with System.Numerics

| Surface | Current behavior | Notes |
| --- | --- | --- |
| Layout conversion | Direct read/construction through the private packed overlay | Same 8/12/16-byte float payload; all AlvorKit component aliases remain. |
| Negate and pair add/subtract/multiply/divide | Delegates to the matching System vector | Leaf benchmarks are at or near parity. |
| Typed `Equals`, `==`, and `!=` | Delegates through the exact private System view | Preserves `float.Equals` NaN and signed-zero semantics; Vec2/3/4 improved 42.2%/30.9%/37.7% and reached System parity. |
| Scalar multiply/divide | Delegates where System provides a matching packed operation | Other scalar overloads remain component-wise when System has no equivalent. |
| `Min` / `Max` | Regular `Vector*.Min` / `Vector*.Max` | NaNs, equal values, and signed-zero ties match System. Double and Half use the same generic-math contract. |
| `Clamp(value, vectorMin, vectorMax)` | Regular `Vector*.Clamp` | Replaces `ClampNative`; exceptional values now match System. |
| `Clamp(value, scalarMin, scalarMax)` | Regular `Vector*.Clamp` with direct System splats | Avoids repeated repository-vector reconstruction in hot loops. |
| `Saturate` | Regular System clamp to zero and one | Keeps Saturate consistent with public Clamp. |
| `Abs` | Regular `Vector*.Abs` | `-0` becomes `+0`; the NaN sign bit is cleared exactly as System does. |
| `Cross` (`Vec3`) | System packed operation when hardware acceleration is available | Scalar fallback remains for disabled intrinsics. |
| Fused multiply-add, square root, inverse square root, and rounding | Matching System packed functions where supported | Exact fallback paths remain for other scalar families. |

## Deliberate or remaining differences

| Surface | Difference from System.Numerics | Decision |
| --- | --- | --- |
| `Dot` | Uses an ordered scalar reduction instead of System's packed reduction order. | Retained: it can differ in low floating-point bits, but measured 25-30% faster. |
| `Length`, `LengthSquared`, `Distance`, `DistanceSquared`, `Normalize` | Inherit AlvorKit's ordered Dot/reduction and formula choices. | Retained for established result order; performance varies by shape. |
| `Lerp` | Uses `from + ((to - from) * amount)` instead of System's formula/order. | Retained because the rounding result can differ even though measured speed is near parity. |
| `Barycentric` | Uses explicit ordered vector composition; System has no identical fixed-vector API contract. | Retained for formula and rounding stability. |
| `Reflect`, `Refract`, `FaceForward` | AlvorKit owns formula, reduction, and branch order. | Retained because substituting another composition can change last bits and branch results. |
| `%` and remainder helpers | Follow the repository scalar/operator contract. | System fixed vectors do not provide the same complete public surface. |
| Equality and hashing | AlvorKit uses its own value semantics and hash composition. | Floating Equals + hash measured 8-17% slower; fixed-integer default comparers are at parity. |
| Swizzles and aliases | AlvorKit exposes a much larger generated swizzle surface plus position/color/texture aliases. | This is the reason for the explicit overlapping layout and is not present on System vectors. Four-lane reverse swizzles remain a measured optimization candidate. |
| Generic interfaces | AlvorKit vectors implement the repository's generic maths interfaces across float, double, half, signed, unsigned, and Boolean families. | System's fixed float vectors do not provide this cross-family API. |
| Indexing, spans, copy, parsing, formatting, masks, and conversions | AlvorKit has a broader contract and additional exception/format behavior. | These surfaces have no one-to-one System comparison and are tracked separately in the performance manifest. |

## .NET SIMD facilities used by the maths package

.NET exposes three useful levels:

1. `System.Numerics.Vector2`, `Vector3`, and `Vector4`: fixed float vectors that
   RyuJIT recognizes intrinsically. These are the best bridge for AlvorKit's
   float shapes.
2. `System.Numerics.Vector<T>`: a hardware-width generic vector intended for
   data-parallel loops. Its width varies with the runtime and target CPU, so it
   is not a direct representation for fixed 2/3/4-component public structs.
3. `System.Runtime.Intrinsics.Vector64/128/256/512<T>` and ISA-specific APIs:
   explicit packed operations used for integer, unsigned, double, half, mask,
   and shuffle candidates when their exact semantics and layout permit it.

Private storage was also measured at these three levels. Matching
`System.Numerics` fields are useful representation bridges: the Vec2/3/4
overlays close the float compound-expression gap, and a matching private
`System.Numerics.Quaternion` view improves Quat arithmetic by 24.7% and a
normalize compound by 13.4%. Generic `Vector64/128/256<T>` fields are not the
same optimization: exact-register overlays for double, Int32, UInt32, Int64,
and UInt64 vectors were neutral to 51% slower and were reverted. Those types
retain direct equal-size `Unsafe.BitCast` at the operation boundary instead.

SIMD should be selected per operation and shape. A vector-sized intrinsic can
lose to scalar code when padding, lane extraction, partial-register handling,
exceptional-value repair, or a custom-struct boundary costs more than the
parallel arithmetic saves.
