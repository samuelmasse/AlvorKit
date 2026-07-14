# Maths Performance: Theory and Cost Model

> Status: retained checkpoint. The measured vector, matrix, and quaternion
> optimizations are in production generation; remaining candidates are recorded
> as an optional backlog in the
> [Maths Performance epic](Maths.PerformanceEpic.md).

## Objective

Make the generated maths implementation approach the fastest exact-semantics
implementation that .NET 10 can produce on supported hardware while keeping
the published API, type layouts, arithmetic promotion, exception behavior,
and component results unchanged.

Performance work is accepted only when an optimized Release benchmark shows a
repeatable improvement for the complete public operation. A smaller helper,
fewer source lines, or attractive intrinsic instruction is not evidence by
itself.

## Fixed Public Contract

This epic does not change:

- public type names, members, overloads, interfaces, or generic constraints;
- field order, component order, `SizeInBytes`, or unmanaged layout;
- implicit and explicit conversion availability;
- C# numeric promotion and unchecked overflow behavior;
- floating-point operation order where it affects observable results;
- NaN payload, NaN sign, signed-zero, infinity, subnormal, or tie behavior;
- exception type, parameter name, and invalid-enum behavior;
- parsing, formatting, equality, hashing, or serialization results.

An optimization may use `System.Numerics`, `Vector64`, `Vector128`,
`Vector256`, `Vector512`, `Unsafe`, `MemoryMarshal`, stack storage, or explicit
hardware intrinsics internally. Those mechanisms do not justify changing the
contract above.

## Generated Surface

The vector generator emits three dimensions for fourteen scalar families:

| Family | Scalar width | Vector types |
| --- | ---: | --- |
| Float | 32 | `Vec2`, `Vec3`, `Vec4` |
| Double | 64 | `Vec2d`, `Vec3d`, `Vec4d` |
| Half | 16 | `Vec2h`, `Vec3h`, `Vec4h` |
| Boolean | implementation-defined field stride | `Vec2b`, `Vec3b`, `Vec4b` |
| Signed integer | 8, 16, 32, 64, 128 | three dimensions per width |
| Unsigned integer | 8, 16, 32, 64, 128 | three dimensions per width |

Matrices cover every 2-to-4 column and row combination for float and double,
for eighteen generated matrix types. Quaternions cover float and double.
Planes, frusta, spheres, intervals, rays, segments, capsules, triangles,
oriented boxes, axis-aligned boxes, quads, and viewports compose those primary
types.

## Hardware Shape Model

Let:

- `C` be the component count;
- `S` be the scalar size in bytes;
- `P = C * S` be the public payload size;
- `R` be the selected register width in bytes;
- `L` be useful SIMD lanes;
- `U` be unused or synthesized lanes;
- `T_setup` be packing, splatting, conversion, and extraction cost;
- `T_lane` be the scalar per-lane work;
- `T_simd` be the packed operation cost.

A candidate is useful when:

`T_setup + T_simd + T_extract < C * T_lane`

Complete-register values generally minimize `T_setup` and `T_extract`.
Partial-register values can still win for expensive operations, but cheap
integer addition may lose to the packing overhead.

### Natural register mappings

| Public shape | Natural internal candidate | Notes |
| --- | --- | --- |
| 8-byte values | `Vector64<T>` or low `Vector128<T>` | Measure both; 128-bit setup can dominate |
| 12-byte values | `Vector128<T>` with one unused lane | Never read beyond the public value |
| 16-byte values | `Vector128<T>` | Highest-priority exact mapping |
| 24-byte values | split `Vector128<T>` or padded internal `Vector256<T>` | Public layout remains 24 bytes |
| 32-byte values | `Vector256<T>` | Natural for `Vec4d` and some matrices |
| 64-byte values | `Vector512<T>` or multiple narrower registers | Compare code size and downclock risk |

`Vec3`-shaped values must construct or clear the unused lane unless the chosen
operation provably ignores it. Unsafe loads must never over-read a standalone
12- or 24-byte value merely because adjacent memory often exists.

Boolean vectors have a special pre-existing managed/native layout split. Their
fields sit at four-byte explicit offsets and their public/native sizes are
8/12/16, while `Unsafe.SizeOf` and managed-array strides are 5/9/13 because a
managed `bool` occupies one byte. Boolean `SizeInBytes` is therefore not a safe
managed load width. Comparison masks must be materialized as canonical Boolean
fields rather than bit-cast or loaded as an integer mask.

## Operation Classes

### Lane-independent operations

Addition, subtraction, multiplication, negation, bitwise operations, shifts,
min/max, rounding, square root, comparison, and selection are the strongest
SIMD candidates. Each output component depends only on corresponding input
components.

### Reductions

Dot products, lengths, determinants, traces, equality reductions, and mask
`All`/`Any` operations combine lanes. Packed reduction order can differ from
the published scalar expression. A reduction is accepted only when its exact
bits match or when the current API already delegates to that same reduction.

### Promoted small integers

C# promotes `byte`, `sbyte`, `short`, and `ushort` arithmetic to `int` before
the generated cast back to the component type. Packed narrow arithmetic can
wrap at the narrow width and therefore may not implement the same intermediate
semantics. Each operator must prove that final narrowing makes the packed form
equivalent; division, remainder, shifts, and mixed-sign promotion require
special care.

### Division and remainder

Ordinary hardware has packed float and double division but not general packed
integer division. Integer division and remainder remain scalar unless a
constant-divisor or other exact specialization is visible at the actual call
site. The generator must not speculate about runtime divisors.

### Transcendentals and estimates

Vector polynomial implementations, reciprocal estimates, reciprocal-square-
root estimates, and multiply-add estimates commonly change result bits. They
are ineligible for existing exact methods unless exhaustive evidence proves
the same contract on every supported path. No `Fast` public API is introduced
by this epic.

## Floating-Point Exactness

Every retained floating-point candidate is compared component by component by
raw bits. Test sets include:

- positive and negative zero;
- smallest and largest subnormals;
- normal boundary values;
- positive and negative infinity;
- quiet and signaling NaNs with varied sign and payload;
- cancellation, intermediate overflow, and underflow cases;
- midpoint and equal-operand ties;
- invalid enum values where applicable.

Formula substitution is forbidden even when numerical error is small. For
example, System `Lerp` and `Reflect` are not interchangeable with AlvorKit's
current formulas. Exact formulas may still be expressed through SIMD-backed
operators without reassociation.

## Integer Exactness

Integer candidates are checked against unchecked scalar reference expressions
for:

- zero, one, minus one, min, and max;
- alternating-bit and single-bit patterns;
- overflow and underflow wraparound;
- signed and unsigned comparison boundaries;
- shift counts at zero, width minus one, width, and larger masked counts;
- mixed-sign and promoted overloads;
- division overflow and division by zero exceptions.

Checked build context must not leak into generated operator behavior.

## Unsafe and Layout Rules

Unsafe mechanisms are tools, not objectives.

- Prefer `Unsafe.BitCast` for equal-size blittable representations.
- Prefer `Unsafe.As` or `MemoryMarshal` only when alignment and lifetime are
  explicit and the JIT produces better code than construction.
- Never over-read a value, depend on neighboring array elements, or expose an
  internal padded representation through the public layout.
- Never create a managed array, box, closure, iterator, or defensive copy in a
  maths hot path.
- Use unaligned loads when alignment is not a public invariant.
- Keep stack storage bounded and justified by measured compound operations.
- Add `AllowUnsafeBlocks` only when pointer syntax produces a retained gain
  that cannot be expressed through safe intrinsic APIs and `Unsafe` helpers.

## JIT Cost Model

The complete caller is the acceptance unit. Important JIT effects include:

- inlining and failure to inline small wrapper overloads;
- hidden struct copies and return-buffer conventions;
- generic sharing versus concrete specialization;
- splat construction and store-forwarding stalls;
- bounds-check elimination;
- register spills caused by overly wide expressions;
- code-size growth and instruction-cache pressure;
- tiered compilation, dynamic PGO, and loop cloning;
- hardware-intrinsic fallback expansion.

`MethodImplOptions.AggressiveInlining`, `AggressiveOptimization`, local helper
extraction, direct expression emission, and unsafe reinterpretation are
experiments. None is retained without complete-caller evidence. The scalar
`Step` regression found during the float pass demonstrates why helper-level
reasoning is insufficient: delegating through an apparently trivial overload
created a 4-to-5-times slowdown until the packed expression was emitted at the
public method.

## Benchmark Model

### Measurement tiers

1. **Primitive latency/throughput:** one named operation per deterministic
   input item, preallocated outputs, `OperationsPerInvoke` set to item count.
2. **Compound operation:** matrices, quaternion transforms, normalization,
   projection, and geometry using complete public callers.
3. **Workload kernels:** repeated transforms, physics integration, camera
   operations, and collision helpers without adding public APIs.
4. **Fallback correctness:** tests and selected timings with hardware
   intrinsics disabled; correctness is mandatory, fallback speed is reported.

The current harness begins with 243 float-vector benchmark methods in 28
categories. MSP-02 must separate independent throughput from dependency-chain
work. In particular, Dot throughput writes one scalar result per input item;
an accumulated Dot loop is retained only as an explicitly named reduction
workload.

### Comparison paths

Each applicable case contains:

- the current AlvorKit public operation as baseline;
- a scalar reference implementing the exact published expression;
- a direct System or intrinsic candidate;
- a full AlvorKit-to-candidate-to-AlvorKit path when representation conversion
  is part of the proposal.

Inputs vary deterministically. Timed loops never allocate, use LINQ, build
strings, or reduce unrelated results into an accumulator. Output arrays are
preallocated so the named operation remains observable.

### Environment record

Every accepted result records:

- OS, architecture, CPU, runtime, SDK, and JIT ISA level;
- hardware acceleration for `Vector`, `Vector64`, `Vector128`, `Vector256`, and
  `Vector512`;
- launch, warmup, iteration, invocation, and unroll settings;
- mean, error, standard deviation, ratio, code size, and allocation count;
- the baseline source revision or generated snapshot identity.

### Exhaustive operation manifest

The benchmark project owns a generated or mechanically audited manifest of the
published maths surface. Every operation is classified as:

- benchmarked and retained;
- benchmarked and rejected;
- semantically ineligible;
- hardware-ineligible;
- intentionally cold with a documented reason.

This manifest is the definition of “benchmark everything.” It prevents a large
overload family from being silently omitted while avoiding meaningless copies
of equivalent benchmark code.

## Acceptance Gates

A candidate is retained only when:

1. exact behavior tests pass with intrinsics enabled and disabled;
2. generated output changes only intended scalar families and shapes;
3. the optimized public caller improves outside the observed noise envelope;
4. no timed-loop allocation appears;
5. code-size growth is justified by the measured gain;
6. partial-register loads are memory-safe for standalone values and arrays;
7. supported x64 and ARM64 semantics are represented by portable APIs or
   explicitly dispatched implementations;
8. the benchmark and result are recorded in the epic.

The ordinary retention threshold is at least a 5% mean/median improvement with
the confidence interval and observed noise clearly below the gain. A smaller
win requires repeat confirmation and a consistently better code shape. A
related operation or representative fallback regression above 3% rejects the
candidate unless the epic records and explicitly accepts the tradeoff.

If a candidate helps one dimension or scalar width and hurts another, the
generator selects only the winning shapes. Uniform emitted source is secondary
to uniform public behavior and measured performance.

## Current Frontier

The accepted float-vector pass already covers bit-cast interop, arithmetic,
negation, min/max/clamp, cross product, FMA, square root, inverse square root,
rounding, interpolation, saturation, and step. Dot remains scalar because it
is faster and preserves reduction order. Several System convenience functions
remain unused because their formulas differ.

The first follow-on mappings were complete-register `Vec4i`/`Vec4u`,
`Vec2d`/`Vec4d`, float `Mat4`, and float `Quat`. Vec2d/Vec4d now also retain
defined saturating Int32 conversions; Vec3d conversion remains scalar because
its candidate lost. Raw x86 conversion is explicitly non-evidence when its
exceptional behavior differs from the public contract. Small integers,
remaining three-component double/integer work, and compound geometry follow
only after their packing cost is measured.

Alvor `Mat4` is column-major while `System.Numerics.Matrix4x4` is numerically
the transposed interpretation of the same physical scalar sequence. Interop
and System candidates therefore require transpose-aware mapping. A direct
bit-cast alone is not a valid matrix conversion, and System multiplication may
reverse scalar operand order after transpose mapping, changing NaN payload
selection even when ordinary finite results agree.

### Retained broad-wave policy

The natural-register frontier now includes `Vec2/4i64`, `Vec2/4u64`, all float
matrix column shapes, selected `Vec2d`/`Vec4d` matrix columns, `Quat`, and
`Quatd`. The implementation follows three measured rules:

- equal-size lane work uses `Unsafe.BitCast` plus `Vector128`/`Vector256` or an
  already optimized public vector column;
- ordered reductions, remainders, unsupported double products, and partial
  `Vec3d` registers stay scalar;
- high-level quaternion chains use scalar instruction-level parallelism when
  packed dependency latency exceeds the packed throughput gain.

Unsafe reinterpretation is useful only when source and register sizes match
exactly. Boolean vectors and three-lane double/integer vectors are not over-read.
Blanket `AggressiveInlining` is rejected: it crossed RyuJIT code-size thresholds
and slowed complete quaternion callers despite improving some isolated bodies.
The original packed Hamilton experiment was rejected because special NaN sign
and payload bits differed from the former scalar formula. Once the package
adopted System.Numerics exceptional-value behavior as its floating-point
contract, `Quat` delegated to System and `Quatd` adopted the same packed
DirectX/System evaluation algorithm at double precision. The resulting `Quatd`
Hamilton product measured 1.361 ns versus 1.785 ns for the former scalar body,
23.8% faster with no allocation in a 4,096-item ShortRun.
