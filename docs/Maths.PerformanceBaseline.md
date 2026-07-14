# Maths Performance Baseline

> Status: MSP-00 reference. This records the completed float-vector pass and
> the machine used for its acceptance measurements. MSP-02 now preserves new
> runs under unique artifact directories; earlier MSP-00 numbers were captured
> from the individual BenchmarkDotNet runs before that retention support was
> added.

## Reference Environment

| Property | Value |
| --- | --- |
| OS | Windows 11 25H2 |
| CPU | AMD Ryzen 9 9950X, 16 physical / 32 logical cores |
| SDK | .NET SDK 10.0.301 |
| Runtime | .NET 10.0.9, x64 RyuJIT x86-64-v4 |
| `System.Numerics.Vector` | Hardware accelerated |
| `Vector128` | Hardware accelerated |
| `Vector256` | Hardware accelerated |
| `Vector512` | Hardware accelerated |
| Timed allocation | 0 B for accepted vector cases |

Results from different machines, runtimes, power policies, or ISA availability
must not be compared as if they were same-session A/B measurements.

## Corrected Float Baseline

The first benchmark draft used an unrelated accumulator in some paths. MSP-00
corrected those cases to preallocated independent outputs before accepting any
optimization. Dot has since been split further by MSP-01 into independent
throughput and explicitly named reduction workloads.

Representative original Alvor latencies in nanoseconds per vector operation:

| Operation | Vec2 | Vec3 | Vec4 |
| --- | ---: | ---: | ---: |
| Add | 0.7722 | 1.0198 | 1.2899 |
| Dot | 0.4995 | 0.6677 | 0.8548 |
| Normalize | 1.7293 | 2.1973 | 2.6319 |
| Cross | — | 1.0293 | — |
| Subtract | 0.7676 | 1.0096 | 1.2748 |
| Pair multiply | 0.7721 | 1.0092 | 1.2819 |
| Pair divide | 0.9146 | 1.3727 | 1.8042 |
| Scalar multiply | 0.6696 | 0.8474 | 0.9819 |
| Scalar divide | 0.9147 | 1.3634 | 1.8102 |
| Negate | 0.6351 | 0.8503 | 1.0591 |
| Scalar-left multiply | 0.6626 | 0.8479 | 0.9761 |

## Accepted MSP-00 Gains

Percentages are reductions in mean latency against the corrected original
Alvor implementation on the reference environment.

| Operation family | Vec2 | Vec3 | Vec4 |
| --- | ---: | ---: | ---: |
| Core arithmetic operators | 29–54% | 13–48% | 48–72% |
| Normalize | 23% | 39% | 49% |
| Min, max, and clamp | 40–44% | 29–37% | 59–65% |
| Fused multiply-add | 26% | 29% | 57% |
| Square root / inverse square root | 49% | 65% | 74% |
| Rounding family | 41–46% | 38–46% | 54–66% |
| Lerp and barycentric | 21–29% | 22–31% | 49–58% |
| Saturate and step | 38–63% | 37–67% | 58–75% |
| Cross | — | 31% | — |

Accepted mechanisms:

- equal-size `Unsafe.BitCast` System-vector interop;
- direct packed float arithmetic and negation;
- native-order min, max, and clamp;
- packed cross, FMA, square root, inverse square root, and rounding;
- exact-formula SIMD composition for interpolation;
- packed clamp and comparison/selection for saturate and step.

## Current Relationship to System.Numerics

Most accepted float paths now match direct `System.Numerics.Vector2/3/4`
throughput and native code size within measurement noise. Notable remaining
same-formula gaps on the reference machine were:

- `Vec2` FMA approximately 17% slower;
- `Vec2` exact-formula interpolation approximately 15–16% slower;
- `Vec2` saturate approximately 26% slower;
- `Vec4` saturate approximately 10% slower;
- `Vec4` floor/ceiling approximately 10–15% slower.

Alvor Dot remained roughly 40% faster than the equivalent System path because
the ordered scalar reduction is efficient for two to four lanes. The updated
MSP-01 Dot cases must be used for future comparisons rather than mixing this
throughput result with dependency-chain reduction.

## Rejected or Retained-Scalar Experiments

| Candidate | Decision | Reason |
| --- | --- | --- |
| Packed System Dot | Rejected | Slower and can change reduction order |
| Ordinary System `Min`/`Max` | Rejected for now | Native variants retain the existing NaN/tie/signed-zero selection; changing this remains an explicit semantic decision |
| System `Clamp` and `Abs` | Retained for float vectors | Public behavior now matches System.Numerics; direct System scalar-bound splats remove the former Clamp hot-loop reconstruction |
| System `Lerp` | Rejected | Different formula and rounding |
| System `Reflect` | Rejected | Different multiply-add/reduction behavior |
| Vector transcendental functions | Deferred | Exact result compatibility not established |
| Per-operator hardware guard | Rejected | Improved disabled-intrinsic fallback but materially slowed the accelerated path |
| Scalar `Step` delegating through vector `Step` | Rejected | JIT store-forwarding/call shape caused a 4-to-5-times regression |
| Direct scalar-edge packed `Step` | Accepted | Restored and improved throughput with exact behavior |

## Verification at MSP-00 Completion

- MathsGen tests: 52/52 passed.
- Maths runtime tests: 151/151 passed with intrinsics enabled.
- Maths runtime tests: 151/151 passed with `DOTNET_EnableHWIntrinsic=0`.
- Benchmark Release build: zero warnings and errors.
- Generated float Vec2/3/4 review: 87 SIMD-oriented lines replaced 301
  scalar-oriented generated lines.
- Independent scoped review: no actionable findings.

This verification is a reference, not a substitute for the final MSP-50 gate
after other scalar families and compound types change.

## MSP-10 Integer Quick Screening

Artifact ID:
`out/benchmarks/maths/msp10-int-screen-quick-natural-register`

The first permanent-baseline screen contains 78 methods across 26 operation
categories. Every `Vector128<int/uint>` candidate improved the current Alvor
`Vec4i`/`Vec4u` caller, including complete reinterpretation and output storage.

| Shape | Candidate gain versus current Alvor |
| --- | ---: |
| `Vec4i` | 34–59% across add/subtract/multiply, negate, bitwise, shifts, min/max/clamp |
| `Vec4u` | 46–63% across add/subtract/multiply, bitwise, shifts, min/max/clamp |

This is Quick screening evidence. Production retention still requires exact
tests and a post-change acceptance comparison.

### MSP-10 retained production result

Post-change artifact ID:
`out/benchmarks/maths/msp10-int-post-quick-vec4-production`

The equal-size `Vector128<int/uint>` implementation changed only generated
`Vec4i` and `Vec4u`. Every selected operation improved against its pre-change
Alvor caller:

| Shape | Per-operation gain | Geometric mean ratio | Approximate aggregate gain |
| --- | ---: | ---: | ---: |
| `Vec4i` | 35–58% | 0.515 | 48.5% |
| `Vec4u` | 34–61% | 0.570 | 43.0% |

No operation regressed. Production complement remained about 20% slower than
the standalone intrinsic candidate in the Quick run, while still improving
41–50% over the old caller. It is retained with a focused disassembly follow-up.

## MSP-11 Double Quick Screening

Artifact IDs:

- `out/benchmarks/maths/msp11-vec2d-screen-quick-natural-register`
- `out/benchmarks/maths/msp11-vec4d-screen-quick-natural-register`

Clear candidate improvements versus the current public caller:

| Shape | Operation | Gain versus current Alvor |
| --- | --- | ---: |
| `Vec2d` | Add | 23% |
| `Vec2d` | Pair/scalar divide | 37–39% |
| `Vec2d` | Min | 24% |
| `Vec2d` | Negate | 22% |
| `Vec2d` | Normalize | 27% |
| `Vec2d` | Sqrt / inverse sqrt | 47–49% |
| `Vec4d` | Pair/scalar divide | 39–53% |
| `Vec4d` | Clamp | 30% |
| `Vec4d` | Max | 19% |
| `Vec4d` | Normalize | 50% |
| `Vec4d` | Sqrt / inverse sqrt | 72–74% |
| `Vec4d` | Truncate | 13% |

Add, subtract, multiply, FMA, Dot, ordinary Round, and several scalar
operations were neutral or slower and remain scalar. `Vec4d.Min`, negate, and
scale produced contradictory or threshold-edge results: the candidate improved
the current caller in some cases while a scalar reference was still faster.
They require focused disassembly and repeat measurement rather than immediate
production selection.

### MSP-11 retained production result

Post-change artifact ID:
`out/benchmarks/maths/msp11-double-post-quick-selected-production`

Only the screened winners were emitted through equal-size
`Vector128<double>`/`Vector256<double>` bit-casts. All selected public callers
improved; no double operation regressed:

| Shape | Per-operation gain | Geometric mean ratio | Approximate aggregate gain |
| --- | ---: | ---: | ---: |
| `Vec2d` | 28–56% | 0.542 | 45.8% |
| `Vec4d` | 52–76% | 0.325 | 67.5% |

Production frequently beat the standalone intrinsic candidate because direct
equal-size `Unsafe.BitCast` allowed the JIT to eliminate pack/extract work that
remained visible in the permanent benchmark candidate. Normalize improved
through packed scalar division while retaining its ordered scalar Dot and
square-root calculation.

### Defined Double-to-Int32 follow-on

The raw x86 experiment `20260713-015830255-p6296` is invalid non-evidence
because its exceptional results differed from the public saturating conversion
contract. The probe `20260713-020041497-p44684` established the authoritative
semantics; the valid Screen and candidate Quick retained Vec2d/Vec4d and
rejected Vec3d.

Final production Quick `20260713-021027775-p27052` allocated 0 B and beat the
standalone helper:

| Shape | Ceiling current/original | Floor current/original | Round current/original | Truncate current/original |
| --- | ---: | ---: | ---: | ---: |
| `Vec2d` | 0.8499 / 1.2221 ns | 0.8589 / 1.2199 ns | 0.8331 / 1.2309 ns | 0.9193 / 1.1772 ns |
| `Vec4d` | 0.7466 / 2.0760 ns | 0.7056 / 2.0896 ns | 0.6869 / 2.0530 ns | 0.6788 / 1.9331 ns |

Ceiling, floor, round, truncate, and direct casts share the retained defined
saturating conversion lowering. Vec3d remains scalar/already optimal.

## Broad Int64, Matrix, and Quaternion Wave

Authoritative artifacts:

- before: `out/benchmarks/maths/maths-sweep-pre-quick-scalar-generator`
- retained result: `out/benchmarks/maths/maths-sweep-authoritative-quick-final-retained`
- Vec2 operation triage: `out/benchmarks/maths/maths-regression-triage-screen-vec2-quat-compound`
- quaternion rotation triage: `out/benchmarks/maths/maths-quaternion-transform-triage-screen-final`

Both package sweeps use the same twelve 1,024-item workloads, Quick job, runtime,
and hardware. Every workload remained allocation-free.

| Workload | Before | Retained | Change |
| --- | ---: | ---: | ---: |
| `Vec2i64` mixed natural-register chain | 3.832 ns | 3.784 ns | 1.3% faster |
| `Vec2u64` mixed natural-register chain | 2.845 ns | 3.474 ns | 22.1% slower; see operation triage |
| `Vec4i64` mixed natural-register chain | 8.204 ns | 4.600 ns | 43.9% faster |
| `Vec4u64` mixed natural-register chain | 4.748 ns | 4.868 ns | 2.5% slower/noisy; operation screens win |
| `Mat4` lane chain | 52.849 ns | 6.461 ns | 87.8% faster |
| `Mat4` algebra chain | 33.004 ns | 14.037 ns | 57.5% faster |
| `Mat4d` lane chain | 178.735 ns | 74.157 ns | 58.5% faster |
| `Mat4d` algebra chain | 78.539 ns | 72.597 ns | 7.6% faster |
| `Quat` lane chain | 7.748 ns | 6.367 ns | 17.8% faster |
| `Quatd` lane chain | 10.628 ns | 7.193 ns | 32.3% faster |
| `Quat` compound chain | 37.067 ns | 33.148 ns | 10.6% faster |
| `Quatd` compound chain | 42.954 ns | 38.872 ns | 9.5% faster |

The dependent integer sweep intentionally combines algebraically reducible
operations and is not used alone to accept individual operators. Focused post-
production Vec2 screens show add, multiply, and clamp 24-39% faster than their
scalar baselines and within roughly 1-4% of the standalone Vector128 candidate.
The complete pre-production screens found every selected Vec2/Vec4 signed and
unsigned operation faster. Those operation-level results govern retention.

Quaternion experiments exposed the opposite throughput/latency tradeoff. Packed
component operations win, but forcing packed Hamilton multiplication, padded
double rotation, packed high-level interpolation chains, or blanket aggressive
inlining regressed exact or dependent workloads. The retained design uses SIMD
for lane-independent operators, conjugation, and float rotation; ordered scalar
reductions and flattened scalar ILP remain in Hamilton products, double rotation,
and compound interpolation. This hybrid is faster in both lane and compound
sweeps while preserving exact IEEE behavior.

### Exact-register storage overlay follow-up

A later representation screen tested private packed fields for every vector type
that fits one 64-, 128-, or 256-bit intrinsic register. Generic
`Vector64/128/256<T>` fields did not behave like the JIT-recognized
`System.Numerics.Vector2/3/4` types and were rejected. Representative results were
all allocation-free:

| Workload | Existing bit-cast lowering | Private intrinsic field | Decision |
| --- | ---: | ---: | --- |
| `Vec2i` bitwise compound | 0.5466 ns | 0.8273 ns | Reject; 51.3% slower |
| `Vec4i` add leaf | 0.6065 ns | 0.7260 ns | Reject; 19.7% slower |
| `Vec2d` compound | 2.1395 ns | 2.1689 ns | Reject; neutral/slower |
| `Vec4d` compound | 2.1859 ns | 2.1586 ns | Reject; noise-level difference |
| `Vec2i64` compound | 6.4203 ns | 6.9688 ns | Reject; 8.5% slower |
| `Vec4i64` compound | 6.8400 ns | 7.5155 ns | Reject; 9.9% slower |
| `Vec4i` bounds compound | 9.9377 ns | 10.7593 ns | Reject; 8.3% slower |
| `Vec4u` bounds compound | 9.8205 ns | 11.0179 ns | Reject; 12.2% slower |

`Quat` is the useful exception because it has a matching JIT-recognized
`System.Numerics.Quaternion` representation. Its private explicit-layout view
keeps the public X/Y/Z/W fields, offsets, and 16-byte size unchanged:

| Workload | Before | Private System view | System reference | Change |
| --- | ---: | ---: | ---: | ---: |
| Arithmetic `(left + right) * 0.5f` | 0.9216 ns | 0.6942 ns | 0.8066 ns | 24.7% faster |
| Normalize compound | 2.3114 ns | 2.0025 ns | 1.7143 ns | 13.4% faster |

Every case allocated 0 B. Component-bit and field-offset tests pass with hardware
intrinsics enabled and disabled. `System.Numerics.Quaternion.Conjugate` was not
retained because it changed an exceptional NaN sign bit; the exact sign-bit-XOR
path remains.

## Floating Equality and Hashing Split

The original vector comparison combined typed `Equals` and `GetHashCode`, so it
could not attribute its 8–17% gap. A focused ShortRun measures the same ordinary,
unequal-first-lane 4,096-element population as separate leaves:

| Shape | Alvor Equals | System Equals | Equality result | Alvor hash | System hash | Hash result |
| --- | ---: | ---: | --- | ---: | ---: | --- |
| Vec2 | 1.1369 ns | 0.8932 ns | 27.3% slower | 1.8624 ns | 2.1243 ns | 12.3% faster |
| Vec3 | 1.2657 ns | 0.9179 ns | 37.9% slower | 3.4595 ns | 2.9058 ns | 19.1% slower |
| Vec4 | 0.9870 ns | 0.6792 ns | 45.3% slower | 3.5916 ns | 3.3898 ns | 6.0% slower/noisy |

Every method allocated 0 B. Equality is the only consistent disadvantage.
AlvorKit's generated body calls `float.Equals` component by component and
short-circuits, while .NET 10's fixed vectors use a JIT-intrinsic
`Vector128.Equals` that preserves typed `.Equals` behavior, including NaN
equality. The Vec4 disassembly is 139 bytes plus a 42-byte scalar helper versus
46 bytes for System's packed equality leaf.

Both hash implementations call the same `HashCode.Combine` overload. The mixed
hash results therefore do not identify an algorithmic gap. Representative dry
disassembly shows a smaller System tail-call wrapper, but a packed hash rewrite
is not justified without a repeat run and a production candidate. The exact
private System view is a concrete candidate for float typed equality only.

### Packed equality production result

Vec2, Vec3, and Vec4 typed equality now delegates through the exact private
System.Numerics view already overlaid on their public fields. The identical
12-case ShortRun after regeneration produced:

| Shape | Previous Alvor | Current Alvor | Current System | Raw Alvor gain | Current result |
| --- | ---: | ---: | ---: | ---: | --- |
| Vec2 | 1.1369 ns | 0.6576 ns | 0.6602 ns | 42.2% | parity; 0.4% faster |
| Vec3 | 1.2657 ns | 0.8744 ns | 0.8359 ns | 30.9% | 4.6% slower; parity threshold |
| Vec4 | 0.9870 ns | 0.6149 ns | 0.6148 ns | 37.7% | parity |

Every method allocated 0 B. Unchanged hash controls moved by 2%–26% between
the two independent launches, demonstrating run-level drift; the within-run
comparison to System is therefore the stronger retention evidence. The
normalized Alvor/System ratio improved by about 21.8% for Vec2, 24.1% for Vec3,
and 31.2% for Vec4.

The public API and 8/12/16-byte layouts are unchanged. Exact tests cover NaNs
with different payloads and signs, signed zero, infinity, finite mismatches,
`==`, `!=`, and execution with hardware intrinsics disabled.
