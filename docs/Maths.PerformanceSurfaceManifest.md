# Maths Performance Surface Manifest

## Purpose

This is the controlling inventory for the AlvorKit maths performance epic. The
goal is not complete until every public touch point is present and has one final
status. All generated swizzle properties count as one touch point, `Swizzles`.
Overloads with the same public operation and performance shape may share a row;
different scalar widths, dimensions, layouts, algorithms, or exception behavior
must be recorded separately.

The original optimization checkpoint concluded on 2026-07-12 and was reopened
on 2026-07-13 for System-equivalent backend parity. Rows still marked pending
or unmeasured are the preserved optional backlog, not claims of completed
measurement.

The exhaustive public generic-interface declaration and dispatch appendix is
[Maths Performance Interfaces](Maths.PerformanceInterfaces.md).

## Unified Maths Backend Policy

AlvorKit exposes one maths ecosystem. Scalar family and backend choice must not
change the public operation being requested, overload direction, layout,
success/failure convention, or throwing versus `Try` contract. Runtime-backed
float types and locally implemented double, integer, Half, and compound types
therefore share the same mathematical vocabulary and API expectations.

The implementation is not required to preserve a historical evaluation order,
NaN payload selection, or signed-zero bit when a faster System implementation
performs the same mathematical operation. For an operation represented by
`System.Numerics`, a measured faster System path is the preferred float backend;
the System result behavior becomes the behavior of that operation. Other scalar
families may use intrinsics or scalar code with analogous mathematical and
failure semantics. Bitwise identity across different scalar widths is neither
possible nor part of the ecosystem contract.

System APIs are not substituted when they implement a different operation. For
example, a System helper that normalizes a result cannot replace a non-normalizing
AlvorKit operation merely because the types have the same shape. Layout adapters
such as Mat4's transpose are internal backend details and do not change public
row/column interpretation.

## Status Values

| Status | Meaning |
| --- | --- |
| `Optimized` | A faster implementation is retained with benchmark and exactness evidence. |
| `Already optimal` | The existing implementation won or matched all valid candidates. |
| `Retained scalar` | SIMD/unsafe candidates lost, changed semantics, or had unsafe layout requirements; the row records why. |
| `Ineligible` | No equivalent packed implementation exists without changing the public contract. |
| `Unmeasured` | The surface is inventoried but still requires candidate benchmarks. |
| `Cold path` | Optimization is intentionally unnecessary; the row records the allocation/IO/error-boundary reason. |
| `Blocked` | Evidence cannot currently be obtained; the external blocker is recorded. |

## Generated Type Inventory

| Family | Public generated types | Notes |
| --- | ---: | --- |
| Vectors | 42 | 14 scalar families times dimensions 2, 3, and 4; swizzles are one touch point. |
| Matrices | 18 | Nine column/row shapes for `float` and `double`. |
| Quaternions | 2 | `Quat` and `Quatd`. |
| Boxes | 6 | Dimensions 2 and 3 for `float`, `double`, and `int`. |
| Plane, frustum, sphere, capsule, OBB, interval, ray, segment, triangle, quad, viewport | 22 | Float and double variants of each family. |

Hand-authored `ScalarMath`, generic maths interfaces, enums, parsing helpers,
formatting helpers, and throw helpers are audited separately from generated
value types.

## Master Family Progress

| Family | Inventory | Optimization | Benchmark evidence | Exact/fallback evidence | Final documentation |
| --- | --- | --- | --- | --- | --- |
| `ScalarMath` | Complete | Retained generic scalar surface | All 41 APIs screened; selected candidates Quick-tested | Exact direct baselines and rejected specialized candidates | In progress |
| Float vectors | Complete | Optimized, including direct distance families and Vec3/Vec4 defined Float-to-Int32 conversion | Complete for hot numeric, distance, and conversion families | Exceptional conversion semantics exact-validated | Complete |
| Double vectors | Complete | Selected natural-register paths, Vec2d/Vec4d defined Int32 conversion, all-shape Saturate, and Vec2d/Vec4d vector-edge Step optimized | Defined conversion, Saturate, distance, interpolation, and Step Screen/Quick decisions complete | Saturating exceptional conversion plus NaN payload, signed-zero, and infinity behavior validated | Complete |
| Half vectors | Complete | Vec3h scalar-amount Lerp and Vec2h/3h/4h fallback normalization optimized; other arithmetic retained scalar | Broad characterization, concrete Lerp promotion, and all-shape cached-normalization Screen/Quick complete | Exact generator plus Lerp bit test and two normalization bit/fallback runtime tests passed | In progress |
| 8/16-bit integer vectors | Complete | Narrow `Vector64` bounds rejected; remaining surface pending | Permanent exact Vec2/3/4 suite; safe-layout candidates screened | Exact scalar baselines present | In progress |
| 32-bit integer vectors | Complete | `Vec2i/u` bitwise, selected `Vec3i/u` bounds, and broad Vec4i/u arithmetic, per-lane shifts, and bit functions optimized; partial shifts/bit functions and distance retained current | Vec4 bit-function Screen/Quick/production complete; Vec2/3 padded candidates rejected; distance retained current | Hardware-gated packed paths, scalar fallbacks, shift validation, and exact bit-function tests complete | In progress |
| 64-bit integer vectors | Complete | Natural `Vec2/4` optimized; all per-lane shifts and `Vec3` remain pending | Complete for natural scalar-count shapes; variable-count intrinsic candidates pending | Complete for retained work | In progress |
| 128-bit integer vectors | Complete | Direct SIMD ineligible; scalar implementation unmeasured | Permanent exact Vec2/3/4 suite added; execution pending | Exact scalar baselines present | In progress |
| Boolean vectors and masks | Complete | Vec4 float/Int32 Select and Vec3b false optimized; other mask packing retained scalar | All-payload Screen and selected Quick complete | Exact targeted tests complete | In progress |
| Vector conversions and interop | Complete | Float System interop, Vec3/4 Float-to-Int32, and Vec2d/4d Double-to-Int32 conversions optimized; 21/22 other direct pairs closed | Valid float/double defined-conversion Screen/Quick/production complete; Vec3i64-to-Vec3i128 rerun remains | Exact exceptional/saturating validation complete for retained float and double paths | In progress |
| Vector construction/access/spans | Complete | Mostly already direct; aggregate composition/inlining leads pending | Alternative closure Screen complete for composition/conversion aggregates; permanent all-layout infrastructure execution remains pending | Exact baselines authored; measured evidence partial | In progress |
| `Swizzles` | Complete | Shuffle candidate pending as one touch point | Representative identity, reverse, repeated-lane, and writable-permutation candidates remain to be screened | Generated tests exist | In progress |
| Matrices | Complete | Float System parity for Mat4 multiply/transform/transpose/invert and Mat3x2 inverse/transform; broad direct-column/row Mat4d paths optimized | System parity ShortRun plus valid Mat4 core Screen and selected Quick complete; permanent all-shape and 192 closure categories added | Runtime-backed exactness and fallback tests complete for retained work | Partial |
| Quaternions | Complete | System-backed float and System-style Vector256 double Hamilton products plus packed lanes | Float System parity measured; double semantic transition exact-tested and 23.8% faster than the former scalar formula | Complete for retained work | Complete |
| Boxes | Complete | Candidate suite ready for float/double | Permanent suite pending execution | Existing unit coverage | In progress |
| Planes | Complete | Candidate suite ready | Permanent suite pending execution | Existing unit coverage | In progress |
| Frustums | Complete | Unmeasured directly | Permanent branch, creation, invalid, copy, and transform-failure suites added; execution pending | Existing unit coverage | In progress |
| Spheres | Complete | Unmeasured directly | Permanent containment/distance, point-span, factory, and edge suites added; execution pending | Existing unit coverage | In progress |
| Capsules | Complete | Unmeasured directly | Permanent compound suite added; execution pending | Existing unit coverage | In progress |
| OBBs | Complete | Unmeasured directly | Permanent SAT/corner/failure/static suites added; execution pending | Existing unit coverage | In progress |
| Intervals | Complete | Unmeasured directly | Permanent exact branch-population suite added; execution pending | Existing unit coverage | In progress |
| Rays | Complete | Candidate suite ready | Permanent suite pending execution | Existing unit coverage | In progress |
| Segments | Complete | Unmeasured directly | Permanent branch-population suite added; execution pending | Existing unit coverage | In progress |
| Triangles | Complete | Unmeasured directly | Permanent caller suite added; detailed face/edge/vertex populations remain | Existing unit coverage | In progress |
| Quads | Complete | Unmeasured directly | Permanent geometry plus complete infrastructure suite added; execution pending | Existing unit coverage | In progress |
| Viewports | Complete | Unmeasured directly | Permanent project/unproject/picking plus complete infrastructure suite added; execution pending | Existing unit coverage | In progress |
| Parsing and formatting | Complete | Cold path | Permanent 1,800-case allocation-aware suite added; execution pending | Existing unit coverage | In progress |
| Equality and hashing | Complete | Float typed equality optimized; broader audit pending | Float Vec2/3/4 equality/hash split and production rerun complete; partial evidence across compounds, quaternions, and matrices | Exact exceptional float equality tests plus existing unit coverage | In progress |
| Compound value infrastructure | Complete | Authored closure; execution pending | All 28 layouts, 16 allocation-free categories each | Existing generated/unit coverage | In progress |
| Public generic interfaces | Complete | Representative dispatch callers authored; execution pending | All 73 core and 12 marker/composition interfaces mapped in the appendix | Declaration and marker audit complete | Complete |

## Authored Closure Inventory

These suites are permanent inventory evidence. Except for the separately named
Mat4 and Boolean runs in the Evidence Log, they have not been executed with
BenchmarkDotNet and therefore do not establish an optimization decision.

| Closure | Authored coverage | Execution status |
| --- | --- | --- |
| Vector closure | 2,180 closed cases spanning all 42 layouts, infrastructure, promoted arithmetic/relations, remaining conversions, Half/narrow/128-bit work, transcendental gaps, generic dispatch, and swizzles | Authored and build-validated; the 172-method alternative Screen and Half broad/concrete subsets are complete, while the remaining authored closure awaits execution |
| Non-vector audit gaps | 108 categories spanning matrix, quaternion, compound, and constrained generic-dispatch omissions | Authored and build-validated; BDN execution pending |
| Cold text | 90 public value types times 20 allocation-aware formatting/parsing categories = 1,800 cases | Authored and build-validated; intentionally cold; BDN execution pending |
| Compound infrastructure | 28 compound layouts times 16 allocation-free construction/access/copy/conversion/equality/hash categories = 448 cases | Authored and build-validated; BDN execution pending |
| Generic interface appendix | 73 core interfaces plus 12 generated marker/composition interfaces; 661 declaration lines and 299 unique names | Documentation/link audit complete; representative callers remain unexecuted |

## Vector Shape Matrix

| Scalar family | `Vec2` | `Vec3` | `Vec4` | Retained optimized work |
| --- | --- | --- | --- | --- |
| `float` | Optimized; Float-to-Int32 retained scalar | Optimized, including defined Float-to-Int32 | Optimized, including defined Float-to-Int32 | Arithmetic, bounds, roots, FMA, rounding, interpolation, saturation/step, normalization, distance, conversions; Vec3 Cross. |
| `double` | Selective optimized, including Int32 conversion/Saturate/Step | Retained scalar plus Saturate; conversion retained scalar | Selective optimized, including Int32 conversion/Saturate/Step | Vec2/4 retain defined saturating Int32 conversions and selected arithmetic/Step paths; Vec3 conversion stays scalar. All dimensions retain exact Saturate behavior. |
| `Half` | NormalizedOr optimized | Vec3h Lerp and NormalizedOr optimized | NormalizedOr optimized | .NET 10 has no supported managed packed FP16 arithmetic/conversion path. The retained exact scalar formulas cache normalization length once and use selective inlining; Vec3h Lerp uses exact operator composition. |
| `bool` | Retained scalar | Vec3 false optimized | Vec4 Select optimized for float/Int32 payloads | Public Boolean storage is not a native mask; explicit conversion is required. |
| `sbyte` / `byte` | Unmeasured | Unmeasured | Narrow bounds retained scalar | Safe Vec4 `Vector64` bounds were 6.0x–7.8x slower; arithmetic promotion returns Int32 vectors. |
| `short` / `ushort` | Unmeasured | Unmeasured | Narrow bounds retained scalar | Safe Vec4 `Vector64` bounds were 2.9x–5.0x slower; arithmetic promotion returns Int32 vectors. |
| `int` / `uint` | Bitwise optimized; shifts, bit functions, and distance retained where partial | Partial-register SIMD, shifts, bit functions, and distance retained | Optimized, including shifts and packed bit functions; unsigned IsPowerOfTwo retained scalar | Vec2/3 padded bit-function candidates lose to pack/pad/extract overhead. Vec4 retains hardware-gated packed leaves with exact scalar fallbacks; unsigned power-of-two remained below threshold. |
| `long` / `ulong` | Optimized; per-lane shifts pending | Retained scalar; per-lane shifts pending | Optimized; per-lane shifts pending | Natural-register arithmetic, bitwise, scalar-count shifts, and bounds are retained; all Int64 variable-count shifts still need focused intrinsic candidates. |
| `Int128` / `UInt128` | Unmeasured/ineligible direct | Unmeasured/ineligible direct | Unmeasured/ineligible direct | No native 128-bit lane arithmetic. |

### Half Vector Decision

.NET 10 exposes no supported managed packed FP16 arithmetic or Half/float
conversion facility suitable for these value types. Arithmetic, reductions,
rounding, and transcendental operations therefore remain scalar so that Half
intermediate rounding, NaN behavior, and public reduction/order semantics stay
exact. Scalar implementation work can still remove redundant computation:
`NormalizedOrZero` and `NormalizedOr(fallback)` now cache `LengthSquared` once
and selectively inline across Vec2h, Vec3h, and Vec4h. Raw-bit classification
and layout-safe wide span load/store candidates remain pending and are not
covered by this arithmetic decision.

| Stage | Artifact | Measured result | Decision |
| --- | --- | --- | --- |
| Broad Screen characterization | `20260712-230740441-p47844-screen-half-vectors-full-screen-rerun` | 144 methods / 72 current-versus-generic-loop pairs, 0 B; current materially faster in 65 pairs, within ±5% in 4, and slower by more than 5% in 3 | The generic scalar loop is characterization only, not an optimization baseline; do not infer 65 optimized members from it |
| Concrete division/remainder/Lerp Screen | `20260712-232349309-p27168-screen-half-concrete-candidates-screen` | Division and remainder produced no wins; direct Lerp was provisionally 9% faster for Vec3h and 26% for Vec4h | Retain scalar division/remainder; promote only Lerp candidates |
| Concrete Lerp Quick | `20260712-232559349-p2772-quick-half-lerp-concrete-quick` | Vec4h direct Lerp was 7% slower; Vec3h direct Lerp remained about 6% faster; 0 B | Reject Vec4h; promote Vec3h to Full |
| Vec3h direct Lerp Full | `20260712-232747360-p47740-full-half-lerp3-concrete-full` | 44.36 ns current to 41.64 ns direct, 6.1% faster, 0 B | Direct body was a valid lead but was superseded by the operator-composed formula |
| Vec3h composed Lerp Full | `20260712-233828554-p36768-full-half-lerp3-composed-full` | 43.84 ns current to 31.92 ns composed, 27.2% faster, 0 B | Promote the operator-composed Half formula; reject intervening body-only and broad-inlining shapes that failed to reproduce this ceiling |
| Vec3h composed production Full | `20260712-234038745-p15620-full-half-lerp3-composed-production-full` | Production current 33.42 ns, composed ceiling 31.89 ns, direct 41.61 ns; all 0 B | Retain operator composition plus selective `AggressiveInlining`; exact bit test passed. Production improves the immediately prior 43.84 ns by 23.77% (23.8%) and the original 44.36 ns Full by 24.66% (24.7%) |
| Cached normalization Screen | `20260712-235843285-p42992-screen-vector-normalized-cached-screen` | 72 methods across all nine floating vector shapes; float and double were neutral/slower, while only Vec2h/3h/4h advanced | Retain float/double as already optimal; promote only Half cached-length candidates |
| Half cached normalization pre-production Quick | `20260713-000514112-p41020-quick-half-normalized-cached-quick` | Cached `LengthSquared` plus selective inlining confirmed the Half candidate shape | Retain the exact scalar cached-length implementation; generator and two runtime bit/fallback tests passed |
| Half cached normalization production Quick | `20260713-001051752-p25840-quick-half-normalized-production-quick` | Vec2h fallback/zero improved 35.0%/37.4%, Vec3h 26.8%/32.8%, and Vec4h 39.0%/37.0%; all 0 B | Retain production for all Half dimensions. Vec4h failure paths also improved about 39%; other failure paths were neutral/small and require no separate change |

## Vector Core and Value Touch Points

These rows cover all 42 vector types unless restricted.

| ID | Touch point | Status | Evidence or reason |
| --- | --- | --- | --- |
| V-C01 | Layout constants, component fields, aliases, and packed overlays | Float optimized; other vector layouts retained | Float Vec2/3/4 overlay a private matching System.Numerics vector on the existing explicit-layout fields. All coordinate/color/texture aliases and 8/12/16-byte sizes remain unchanged. Removing aliases alone emitted the same spill loop; the packed overlay removed it and brought the compound Bounds pipeline to System parity. Exact-register `Vector64/128/256<T>` overlays were screened for double, Int32, UInt32, Int64, and UInt64 vectors and rejected: unlike the special System.Numerics vector types, the generic intrinsic fields added register/stack movement and were neutral to 51% slower. Existing equal-size `Unsafe.BitCast` lowering remains optimal for those types. |
| V-C02 | Primary, splat, and `Create` construction | Already optimal | Direct field initialization; consolidated benchmark pending. |
| V-C03 | Zero/one/unit and special floating constants | Already optimal | Default or direct splat construction. |
| V-C04 | Static-interface constants and identities | Already optimal | Constant forwarding. |
| V-C05 | Indexer and `ComponentRef` | Already optimal | Checked `Unsafe.Add`; bool uses its explicit public stride. Bounds contract precludes unchecked replacement. |
| V-C06 | `Deconstruct` | Already optimal | Direct field-to-`out` assignments contain no intermediate value or component-parallel work to remove. This is a structural decision, not benchmark evidence. |
| V-C07 | Span constructor and `Create(ReadOnlySpan<T>)` | Candidate pending | The current helper repeats the same length check for every component. Screen one upfront guard plus direct loads, and an unaligned whole-struct load for numeric vectors only; Boolean spans are byte-packed while Boolean vector fields use a four-byte public stride. |
| V-C08a | Array `CopyTo(array)` and `CopyTo(array, index)` | Already-optimal wrapper | Null checking and `AsSpan(index)` preserve the public array/index exception contract. Any success-path improvement belongs in the span-copy leaf and automatically benefits these wrappers. |
| V-C08b | Span `CopyTo` | Candidate pending for numeric success | The short-span branch and throw helper are already minimal. Screen component stores against one guard plus an unaligned whole-struct store for numeric vectors; Boolean vectors must retain component stores. |
| V-C08c | Span `TryCopyTo` success | Candidate pending | Screen the same numeric bulk-store leaf as V-C08b across representative widths and dimensions. Boolean layout is not compatible with a raw span store. |
| V-C08d | Span `TryCopyTo` short-destination failure | Already optimal | One length comparison returns false before any write; no compatible work can be removed. This is a structural decision, not benchmark evidence. |
| V-C09 | Composition constructors | Candidate/inlining backlog | Aggregate composition/conversion callers were 6% to 9% slower than direct forms in the alternative Screen, but each aggregate combines several constructors or conversions and does not isolate a leaf cause. |
| V-C10 | Tuple conversions | Already optimal | Direct construction or extraction of two to four fields has no conversion, allocation, or temporary storage to remove. This is a structural decision, not benchmark evidence. |
| V-C11 | 22 screened cross-scalar conversions | 21 already optimal; one focused rerun pending | Twenty-one current/direct pairs were within ±5%. Vec3i64-to-Vec3i128 was 6% slower than direct and needs a focused rerun; 6% to 9% aggregate composition/conversion gaps remain candidate/inlining evidence only. |
| V-C12 | Higher-dimension truncation/conversion | Retained scalar | Safe component extraction; whole-register approaches can over-read or retain unwanted lanes. |
| V-C13 | Float `System.Numerics` conversions | Optimized | Direct reads and private packed-constructor writes through the equal-layout System view; exact layout/bit tests pass. A static bit-cast factory was rejected because RyuJIT left helper calls in matrix/vector loops. |
| V-C14 | `Swizzles` | Candidate pending | One touch point for all generated properties. Screen representative identity, reverse, repeated-lane getters, and writable permutations for float, Int32, and double against hardware shuffle forms. |
| V-C15 | Generic `IVec*` dispatch | Partially measured | Half generic call sites executed in the 72-pair Screen as characterization only; static `Create` and remaining-integer callers remain authored/unexecuted, and other interface members remain Unmeasured. |
| V-V01 | `ToString` overloads | Cold path | Culture-aware formatting dominates; string return intentionally allocates. |
| V-V02a | UTF-16 `TryFormat` | Cold path | Variable-length text, separators, culture, and short-destination failure dominate. |
| V-V02b | UTF-8 `TryFormat` | Cold path | Transcoding and byte-destination failure are distinct from UTF-16 formatting. |
| V-V03 | `CompareTo` | Retained scalar | Lexicographic early-exit semantics are not component-parallel. Benchmark pending. |
| V-V04a | String `Parse` | Cold path | String input and throwing invalid-input behavior are part of the contract. |
| V-V04b | Nullable string `TryParse` | Cold path | Null handling and Boolean failure differ from throwing parse. |
| V-V04c | UTF-16 span `Parse` | Cold path | Allocation-free input with throwing failure is a separate shape. |
| V-V04d | UTF-16 span `TryParse` | Cold path | Allocation-free input with Boolean failure is a separate shape. |
| V-V04e | UTF-8 span `Parse` | Cold path | Byte parsing with throwing failure is distinct. |
| V-V04f | UTF-8 span `TryParse` | Cold path | Byte parsing with Boolean failure is distinct. |
| V-V05 | Typed `Equals` | Float optimized; other families retained current | Vec2/3/4 now use the existing exact private System view. Production improved 1.1369→0.6576 ns, 1.2657→0.8744 ns, and 0.9870→0.6149 ns (42.2%/30.9%/37.7%) and is at within-run System parity. NaN payload/sign, signed zero, infinity, operator wrappers, and the disabled-intrinsic fallback are exact-validated. |
| V-V06 | Object `Equals` | Ineligible | Type-test/unbox behavior is part of the object contract. |
| V-V07 | `GetHashCode` | Measured; retain current pending stronger evidence | Both implementations call the same `HashCode.Combine` overload. AlvorKit is 12.3% faster for Vec2, 19.1% slower for Vec3, and 6.0% slower/noisy for Vec4; the mixed ShortRun does not support a blanket hash rewrite. |
| V-V08 | Whole-vector `==` / `!=` | Float optimized by typed-equality composition; other families retained wrapper | Delegates typed equality and Boolean negation. Float wrappers inherit packed exact equality and are covered by exceptional-value tests. |

## Vector Operator and Numeric Touch Points

| ID | Touch point | Status | Evidence or reason |
| --- | --- | --- | --- |
| V-O01a | Unary `+` and narrow promotion | Permanent evidence added for Half and remaining integers | Other scalar families lack consolidated evidence. |
| V-O01b | Increment stored result | Permanent evidence added for remaining integers | Floating and existing 32/64-bit shapes remain incomplete. |
| V-O01c | Decrement stored result | Permanent evidence added for remaining integers | Floating and existing 32/64-bit shapes remain incomplete. |
| V-O02 | Unary `-` | Selective optimized | Float all; Vec2d; Vec4i; Vec2/4i64 retained. Padded Vec2i/Vec3i SIMD lost the representative screen; other signed shapes remain pending or scalar. |
| V-O03a | Same-type pair addition/subtraction | Selective optimized; Half retained scalar | Half has no supported managed packed FP16 arithmetic and exact intermediate rounding must remain; other natural winners are unchanged. |
| V-O03b | Same-type pair multiplication | Selective optimized; Half retained scalar | Half has no supported managed packed FP16 arithmetic; integer overflow/promotion and other floating exceptional behavior remain width-specific. |
| V-O03c | Same-type pair division | Selective optimized; Half retained scalar | Concrete Half Screen found no direct win; float/double winners remain shape-specific and integer division has no ordinary packed equivalent. |
| V-O03d | Same-type pair remainder `%` | Retained scalar/unmeasured | Concrete Half Screen found no direct win; IEEE remainder and integer exceptional behavior are distinct from division. |
| V-O04a | Vector-right-scalar addition/subtraction | Unmeasured | Scalar conversion/splat and narrow promotion vary by width and dimension. |
| V-O04b | Scalar-left-vector addition/subtraction | Unmeasured | Subtraction order and promotion differ from vector-right-scalar. |
| V-O04c | Vector-right-scalar multiplication | Selective optimized | Float retained paths exist; remaining widths/layouts are incomplete. |
| V-O04d | Scalar-left-vector multiplication | Selective optimized | Splat/conversion cost is a distinct caller shape. |
| V-O04e | Vector-right-scalar division | Selective optimized | Float and selected double paths retained; integer behavior remains scalar. |
| V-O04f | Scalar-left-vector division | Unmeasured | Reciprocal direction and divide-by-zero behavior require separate evidence. |
| V-O04g | Vector/scalar and scalar/vector remainder | Retained scalar/ineligible packed | Managed SIMD exposes no packed integer or IEEE remainder operation. Reciprocal/floor substitutions change divide-by-zero, NaN, infinity, overflow, or rounding behavior. |
| V-O05a | Cross-scalar promoted arithmetic | Candidate/inlining backlog | The unsigned/signed directional aggregate was 3.09x slower and the floating-promotion aggregate 12% slower than direct forms. Both combine many operator directions, so neither identifies a retained leaf gain. |
| V-O05b | Cross-vector promoted arithmetic | Candidate/inlining backlog | The narrow promoted-pair aggregate was 4.25x slower and floating promotions were 12% slower than direct forms. Result widths and mixed operations must be isolated before any implementation decision. |
| V-O06a | Integer `&` | Selective optimized | Result width is promoted for 8/16-bit vectors and native-width for 32/64/128-bit vectors. |
| V-O06b | Integer bitwise OR | Selective optimized | Vec2 Int32/UInt32 has retained `Vector64`; other promotion/layout shapes remain distinct. |
| V-O06c | Integer `^` | Selective optimized | Vec2 Int32/UInt32 has retained `Vector64`; other promotion/layout shapes remain distinct. |
| V-O06d | Integer complement `~` | Selective optimized | Unary promotion and sign extension differ from binary bitwise operations. |
| V-O07 | Scalar-count shifts | Selective optimized | Natural Vec4 Int32 and Vec2/4 Int64 paths retain exact count masks. Vec2/3 Int32 padded candidates were 30% to 55% slower in the representative screen. |
| V-O08a | Per-lane left shift | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal; Int64 pending | Portable Vec4 AVX2 plus component fallback retains exact `& 31` masking and improves 35.9%/41.1%. Every padded Vec2i/u and Vec3i/u candidate lost: Vec2 was 84% to 112% slower and Vec3 57% to 75% slower because pack/pad/extract overhead exceeds the two or three shifted lanes. |
| V-O08b | Per-lane arithmetic right shift | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal; Int64 pending | Vec4 production improves 37.3%/41.9% with exact signed/count behavior. Padded Vec2/3 AVX2 candidates lost across signed and unsigned shapes for the same pack/pad/extract overhead and are retained scalar/already optimal. |
| V-O08c | Per-lane logical right shift | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal; Int64 pending | Vec4 production improves 38.1%/37.2% with exact count masking. Padded Vec2/3 AVX2 candidates lost every exact comparison and are retained scalar/already optimal; only Int64 variable-count candidates remain pending. |
| V-O09a | Same-type `<`/`<=` operator masks | Partially measured | Mask materialization is covered for representative and remaining-integer shapes. |
| V-O09b | Same-type `>`/`>=` operator masks | Partially measured | Direction/equality work differs from less-than-only screens. |
| V-O10a | Cross-scalar promoted comparisons | Unmeasured | Scalar width and C# promotion rules vary by pair. |
| V-O10b | Cross-vector promoted comparisons | Retained scalar/ineligible packed | Each lane requires C# promotion and materialization into four-byte Boolean fields. There is no compatible packed Boolean result, and adding cross-type conversion cannot improve on the same-type Int32 mask candidates that already lost. |
| V-N01a | `Dot` | Retained scalar | Ordered reduction is public behavior; packed float Dot was slower, and Half must preserve per-step intermediate rounding/order. |
| V-N01b | `LengthSquared` | Retained scalar | Self-Dot property/delegation is a distinct public caller; Half inherits the exact ordered scalar reduction. |
| V-N02 | `Length` | Already optimal | Scalar composition over retained Dot and root. |
| V-N03a | `DistanceSquared` | Float optimized; double/Int32 already optimal; other scalar families pending | Float production matches the direct ceiling, all 0 B: Vec2 0.7364→0.6964 ns (5.4%), Vec3 1.1619→0.8487 ns (27.0%), Vec4 1.2266→1.0845 ns (11.6%). Double pairs were within ±5% or current faster. Int32 Vec2/3 were neutral; Vec4 direct was 3% worse. |
| V-N03b | `Distance` | Float optimized; double/Int32 already optimal; other scalar families pending | Float production matches direct, all 0 B: Vec2 0.9143→0.8684 ns (5.0%), Vec3 1.1621→0.9304 ns (19.9%), Vec4 1.2228→1.0897 ns (10.9%). Double retained current within ±5% or faster. Int32 Vec2/3 were neutral; Vec4 direct was 22% worse. |
| V-N04a | Component `Min` | Floating families System-aligned; selective packed paths | Float delegates to regular System.Numerics Min. Double uses regular fixed-width intrinsics where measured, and scalar double/Half use the same generic-math NaN/tie contract. Integer signedness remains separate. |
| V-N04b | Component `Max` | Floating families System-aligned; selective packed paths | Float delegates to regular System.Numerics Max. Double uses regular fixed-width intrinsics where measured, and scalar double/Half use the same generic-math NaN/tie contract. |
| V-N04c | Vector-bounds `Clamp` | Floating families System-aligned; selective packed paths | Float uses regular System.Numerics Clamp. Double packed and scalar shapes and scalar Half use the same regular Min(Max()) semantics, including invalid bounds and exceptional values. |
| V-N04d | Scalar-bounds `Clamp` | Float specialized and optimized; other families selective | Float constructs System.Numerics bound splats directly so RyuJIT hoists them: Vec4 improved from 6.6334 to 0.7929 ns and reached System parity. Repository-vector splats inside the wrapper caused the former 8.4x regression. A direct Vec3i expansion remains rejected. |
| V-N05 | `Abs` | Floating families System-aligned; other families selective | Float uses System.Numerics Abs and matches its signed-zero/NaN sign-bit behavior: Vec4 improved from 1.6638 to 0.5750 ns, within 2.5% of System. Double and Half scalar paths use the same sign-clearing IEEE contract; integer MinValue behavior remains unchanged. |
| V-N06a | Vec2 `PerpendicularLeft`/`PerpendicularRight` | Unmeasured | Rearrangement/sign operation with no reduction. |
| V-N06b | Vec2 scalar `Cross` | Unmeasured | Ordered multiply/subtract reduction. |
| V-N06c | Vec2 `PerpDot` | Already-optimal wrapper | The inlineable wrapper delegates to the exact two-multiply/one-subtract `Cross` body and performs no independent work. This is a structural decision, not benchmark evidence. |
| V-N07 | Vec3 Cross | Float optimized; others unmeasured | Other widths have overflow/order or partial-register constraints. |

## Floating Vector Touch Points

| ID | Touch point | Status | Evidence or reason |
| --- | --- | --- | --- |
| V-F01a | `Normalized`/`Normalize` | Selective optimized; Half retained scalar | Float and natural double divide paths inherit SIMD; Half and ordered Dot/root remain scalar for exact reduction/intermediate semantics. |
| V-F01b | `NormalizedOrZero`/`NormalizedOr(fallback)` | Half optimized; float/double already optimal | Half now caches `LengthSquared` once and selectively inlines. Production Quick, all 0 B: Vec2h fallback 55.455→36.075 ns (35.0%) and zero 56.117→35.103 ns (37.4%); Vec3h fallback 78.91→57.79 ns (26.8%) and zero 84.52→56.83 ns (32.8%); Vec4h fallback 109.88→67.04 ns (39.0%) and zero 103.86→65.41 ns (37.0%). The all-nine-shape Screen retained float/double as neutral/slower. |
| V-F01c | `TryNormalize` success/failure | Already optimal | The implementation already caches `lengthSquared`, performs the required zero branch, computes one square root, and writes one out-result. This is a structural decision, not benchmark evidence. |
| V-F02a | Scalar-amount `Lerp` | Float and Vec3h optimized; double already optimal; other Half retained scalar | Double composition is closed/current retained, all 0 B: Vec2d current 0.7972 ns versus composed 3.8658 ns (4.85x slower); Vec3d 1.0822/1.0767 ns (ratio 0.99); Vec4d 1.4384/1.3855 ns (ratio 0.96, below the 5% threshold). Vec3h retains exact operator composition plus selective inlining. |
| V-F02b | Vector-amount `Lerp` | Float optimized; double already optimal; Half retained scalar | Reject double operator composition: Vec2d was 4.82x slower, Vec3d was neutral at ratio 0.98, and Vec4d neutral at 0.97. Retain current double component calls; Half has no supported packed FP16 arithmetic. |
| V-F02c | `Barycentric` | Float optimized; double/Half retained scalar/already optimal | Reject double operator composition: Vec2d was 8.75x slower, Vec3d 15% slower, and Vec4d 9% slower. Half retains exact intermediate rounding. |
| V-F03 | `Reflect` | Retained scalar; Half inlining candidate pending | The Half geometry aggregate was 14% slower than a direct combined Reflect/FaceForward/Refract body. It does not isolate Reflect, and substituting the System formula can change reduction or multiply-add behavior. |
| V-F04a | `FaceForward` | Candidate/inlining backlog | The 14% Half geometry aggregate combines three geometry helpers; Dot, branch, and negate must be isolated before changing this leaf. |
| V-F04b | `Refract` | Candidate/inlining backlog | The 14% Half geometry aggregate combines three helpers and does not isolate the total-internal-reflection branch, square root, or compound vector work. |
| V-F05 | `Saturate` | All floating families System-aligned; double packed where measured | Float uses regular System.Numerics Clamp. Vec2d/3d/4d retain their packed shapes with regular Vector128/256 Clamp, and Half uses the same scalar generic-math contract. Exact normal and intrinsics-disabled tests cover NaNs, signed zero, and infinities; the prior native-clamp performance figures require a focused semantic-transition rerun. |
| V-F06a | `Floor` | Selective optimized; Half retained scalar | Half rounding remains scalar for exact behavior; other width/layout decisions are unchanged. |
| V-F06b | `Ceiling` | Selective optimized; Half retained scalar | Separate rounding behavior and exceptional results must remain exact. |
| V-F06c | Default `Round` | Selective optimized; Half retained scalar | Default midpoint behavior and Half result rounding are distinct from explicit mode. |
| V-F06d | `Round(mode)` | Selective optimized; Half retained scalar | Midpoint mode is runtime input and a separate exact call shape. |
| V-F06e | `Truncate` | Selective optimized; Half retained scalar | Float and Vec4d retained paths remain; Half has no supported packed rounding/conversion path. |
| V-F07a | `FractionalPart` | Float/double already optimal; Half retained scalar | Focused Vec4 float Screen was neutral: current 1.931 ns versus direct 1.940 ns, 0 B. The earlier 2.51x combined FractionalPart/Modulo/SmoothStep gap was therefore a caller-composition artifact, not a leaf gain. Half retains exact subtract-after-floor rounding. |
| V-F07b | `Modulo` | Float/double already optimal; Half retained scalar | Focused Vec4 float Screen was neutral: current 2.011 ns versus direct 2.017 ns, 0 B. The earlier combined gap was a caller-composition artifact; concrete Half remainder/modulo screening also produced no direct win. |
| V-F07c | `Mod` alias | Already optimal | `ScalarMath.Mod` and `Modulo` are aggressively inlineable exact aliases, so delegation leaves no retained runtime work. This is a structural decision, not benchmark evidence. |
| V-F08a | Vector-edge `Step` | Float and Vec2d/Vec4d optimized; Vec3d/Half retained scalar | Retain packed Vec2d/Vec4d compare/select. Versus old production, final current improves Vec2d 0.8744→0.5477 ns (37.4%) and Vec4d 1.6543→0.7458 ns (54.9%), 0 B, matching candidate parity at 0.5452/0.7436 ns. |
| V-F08b | Scalar-edge `Step` | Float optimized; double/Half retained scalar/already optimal | Reject packed double scalar-edge Step: Vec2d was 5.78x slower and Vec4d 2.91x slower because scalar splat/packed setup overwhelms the two/four comparisons. Half lacks supported packed FP16 comparison. |
| V-F09 | `SmoothStep` | Float/double already optimal; Half retained scalar | Focused Vec4 float Screen was neutral: current 2.486 ns versus direct 2.475 ns, 0 B. The earlier 2.51x aggregate gap was caller composition rather than clamp/polynomial leaf cost; Half retains exact intermediate rounding. |
| V-F10a | `Sin` | Retained scalar/already optimal | The alternative Screen closed its float/double/Half current-direct pairs within ±5% or with current faster; exact runtime Sin semantics remain. |
| V-F10b | `Cos` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; the distinct Cos range-reduction/runtime entry point remains scalar. |
| V-F10c | `Tan` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; exact pole behavior remains scalar. |
| V-F10d | `Asin` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; exact domain behavior remains scalar. |
| V-F10e | `Acos` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; exact domain behavior remains scalar. |
| V-F10f | `Atan` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; its runtime entry point remains distinct from Atan2. |
| V-F10g | `Exp` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs within ±5% or with current faster. |
| V-F10h | `Log` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; exact domain and exceptional behavior remain scalar. |
| V-F10i | `Log2` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; its runtime lowering remains distinct from Log. |
| V-F11a | `Atan2` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; two-input exceptional behavior and operand order remain exact. |
| V-F11b | Vector-exponent `Pow` | Retained scalar runtime/already optimal | The provisional 8% Float Screen lead did not reproduce in focused Quick: current was 16.63 ns versus 16.67 ns direct scalar (ratio 1.00), 0 B. Float, double, and Half therefore retain their exact scalar runtime calls with no pending candidate. |
| V-F11c | Scalar-exponent `Pow` | Retained scalar/already optimal | The alternative Screen closed its three current-direct pairs; the scalar-exponent call shape remains exact. |
| V-F12a | `Sqrt` | Float and Vec2/4d optimized; Half retained scalar | Half has no supported packed FP16 transcendental path and retains exact scalar behavior. |
| V-F12b | `InverseSqrt` | Float and Vec2/4d optimized; others scalar | Reciprocal composition and exceptional results differ from Sqrt. |
| V-F13 | `FusedMultiplyAdd` | Float optimized; double/Half scalar | Half keeps exact fused/intermediate semantics without a supported packed FP16 path; double candidates were neutral/slower. |
| V-F14a | Truncate/cast to Int32 vectors | Float Vec3/4 and double Vec2/4 optimized; float Vec2 and double Vec3 scalar/already optimal; Half ineligible | Retain exact defined/saturating paths, 0 B. Double production: Vec2 1.1772→0.9193 ns and Vec4 1.9331→0.6788 ns; final current beats the standalone helper. The raw x86 candidate is invalid non-evidence because its exceptional semantics differed. |
| V-F14b | Floor to Int32 vectors | Float Vec3/4 and double Vec2/4 optimized; float Vec2 and double Vec3 scalar/already optimal; Half ineligible | Double production, exact and 0 B: Vec2 1.2199→0.8589 ns and Vec4 2.0896→0.7056 ns. Vec3d was rejected and retains current scalar behavior. |
| V-F14c | Ceiling to Int32 vectors | Float Vec3/4 and double Vec2/4 optimized; float Vec2 and double Vec3 scalar/already optimal; Half ineligible | Double production, exact and 0 B: Vec2 1.2221→0.8499 ns and Vec4 2.0760→0.7466 ns. Saturating validation passed; Vec3d remains scalar. |
| V-F14d | Round to Int32 vectors | Float Vec3/4 and double Vec2/4 optimized; float Vec2 and double Vec3 scalar/already optimal; Half ineligible | Double production, exact and 0 B: Vec2 1.2309→0.8331 ns and Vec4 2.0530→0.6869 ns. Production current beats the standalone helper; Vec3d was rejected. |
| V-F15a | `IsNaN` | Float/double already optimal; Half raw-bit candidate pending | Focused current/direct Screen was within 1% and 0 B: Vec4 float 1.192/1.188 ns and Vec4d 1.206/1.190 ns. The earlier classification aggregate gap was a caller-composition artifact; Half raw-bit classification remains distinct. |
| V-F15b | `IsInfinity` | Float/double already optimal; Half raw-bit candidate pending | Focused current/direct Screen was within 1% and 0 B: Vec4 float 1.464/1.474 ns and Vec4d 1.534/1.529 ns. Retain current; Half infinity-bit classification remains pending. |
| V-F15c | `IsFinite` | Float/double already optimal; Half raw-bit candidate pending | Focused current/direct Screen was within 1% and 0 B: Vec4 float 1.469/1.470 ns and Vec4d 1.496/1.477 ns. Retain current; Half combined raw-bit classification and Boolean materialization remain pending. |

## Integer, Relational, and Mask Vector Touch Points

| ID | Touch point | Status | Evidence or reason |
| --- | --- | --- | --- |
| V-I01 | `BitCount` | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal | Final packed Vec4 production, 0 B: signed 1.0117→0.8590 ns (15.1%), unsigned 1.0151→0.8450 ns (16.8%). All padded Vec2/3 candidates lost or were neutral because pack/pad/extract overhead exceeded four-lane work. Hardware gates and exact scalar fallback are retained. |
| V-I02a | Leading-zero count | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal | Final packed Vec4 production, 0 B: signed 1.0020→0.5569 ns (44.4%), unsigned 1.0077→0.5673 ns (43.7%). Vec2 candidates were 8% to 144% slower and Vec3 11% to 69% slower across the partial bit-function Screen. |
| V-I02b | Trailing-zero count | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal | Final packed Vec4 production, 0 B: signed 1.0086→0.8373 ns (17.0%), unsigned 1.0084→0.8182 ns (18.9%). Partial-register candidates lost or were neutral and retain exact scalar lowering. |
| V-I03a | Find least-significant bit | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal | Final packed Vec4 production, 0 B: signed 1.2243→0.6997 ns (42.8%), unsigned 1.2224→0.7146 ns (41.5%). Exact zero-to-minus-one semantics are covered by the retained hardware gate/fallback tests. |
| V-I03b | Find most-significant bit | Vec4i/u optimized; Vec2/3 Int32 retained scalar/already optimal | Final packed Vec4 production, 0 B: signed 1.6074→0.6052 ns (62.4%), unsigned 1.5987→0.6010 ns (62.4%). Vec2/3 candidates were otherwise slower; their only 3% to 4% FindMSB lead was below the retention threshold. |
| V-I04 | `IsPowerOfTwo` | Vec4i optimized; Vec4u and all Vec2/3 Int32 retained scalar/already optimal | Signed Vec4 production improves 2.2717→1.8816 ns (17.2%), 0 B. Unsigned Vec4 candidate was only 3.7% faster in Quick and was rejected below threshold. Partial-register candidates lost or were neutral; signed-positive and Boolean-mask semantics remain exact. |
| V-R01a | Same-type `Equal`/`NotEqual` methods | Partially measured; Half retained scalar | Half Equal generic-loop results are characterization only and no supported packed FP16 comparison exists; NotEqual and remaining widths remain incomplete. |
| V-R01b | Same-type less/greater relational methods | Half and representative Int32 retained scalar; remaining matrix pending | Half Less/GreaterEqual characterization does not supply a packed candidate; native Int32 mask materialization also lost. |
| V-R01c | Scalar promoted relational methods | Unmeasured | Scalar width and operand direction alter promotion. |
| V-R01d | Cross-vector promoted relational methods | Scalar representation retained; inlining candidate pending | The combined cross-vector comparison caller was 13% slower than direct scalar materialization. The aggregate does not isolate a leaf and no compatible packed Boolean result exists, so only focused lowering/inlining—not a retained SIMD gain—is pending. |
| V-M01a | Boolean `All` | Retained scalar | Short-circuit false search; native mask packing lost. |
| V-M01b | Boolean `Any` | Retained scalar | Short-circuit true search is a distinct branch direction. |
| V-M01c | Boolean `None` | Retained scalar | Public wrapper/negated reduction remains a distinct caller. |
| V-M02a | Boolean component `Equal` | Retained scalar | Native packing/materialization lost all screened dimensions. |
| V-M02b | Boolean component `NotEqual` | Retained scalar | Opposite comparison/materialization and wrapper cost. |
| V-M03a | Boolean `Select` for float and Int32 payloads | Selective optimized | Vec4 float improves 22.1% and Vec4 Int32 14.5% in Quick; Vec2/3 remain scalar. |
| V-M03b | Boolean `Select` for double, Half, 8/16/64/128-bit and unsigned payloads | Retained scalar/ineligible | All supported native candidates were 3.7x–9.9x slower. `Vector256<Half>` throws `NotSupportedException`; Boolean and Int128/UInt128 lanes have no valid native candidate. |
| V-M04a | Boolean NOT, AND, OR, XOR, and complement | Retained scalar | Native candidates lost direct Boolean materialization. |
| V-M04b | Boolean `true`/`false` operators | Vec3b false optimized; others retained | `20260712-222956249-p31736-quick-boolean-truth-operators-optimized-quick`: Vec3b false fell from 1.2087 ns to 0.8752 ns (27.6%), matched the direct ceiling, and allocated 0 B; true operators and Vec2/4 false did not improve. |

## Matrix Touch Points

The following rows cover all eighteen matrix types unless a shape is named.

| Touch point | Shapes | Status | Evidence or reason |
| --- | --- | --- | --- |
| Layout constants and column fields | All | Already optimal; System shapes packed | Mat4 and Mat3x2 use explicit offsets with a private equal-layout System matrix overlay; public columns, offsets, sizes, and row/column meaning are unchanged. Other matrices retain sequential direct fields. |
| Column, diagonal-scalar, component, row, and diagonal-vector construction | All | Already optimal | Direct nonallocating struct construction from fields or parameters; no intermediate representation or packed work remains. |
| Outer-product construction | All | Unmeasured | A per-column vector-times-row-component candidate remains; current-only construction callers do not decide it. |
| `Lerp` | Float | Optimized | Retained per-column vector composition. |
| `Lerp` | Double | Mat4d optimized; other shapes pending | Direct public-column Mat4d improves about 38.9%; the same indexer-free public-column shape remains a complete-caller candidate for the other double matrices. |
| `Zero`, `Identity`, diagonal and row properties | Applicable shapes | Already optimal | Direct values and field assignments; no meaningful packed work. |
| Column and component indexers, `ColumnRef`, `ComponentRef` | All | Already optimal checked access | A single checked `Unsafe.Add` selects a column; component access composes that operation with the vector's required row check. The exception contract rules out unchecked substitution and there is no packed work. |
| Span constructors and `FromColumnMajor` | All | Unmeasured | One upfront length check plus a sequential unaligned whole-struct load remains a candidate; the short-span exception must stay exact. |
| `FromRowMajor` | All | Unmeasured | One upfront check plus exact shape-specific row/column permutation remains a candidate. |
| `CopyTo`, `CopyToColumnMajor`, `TryCopyToColumnMajor` | All | Unmeasured | Try failure is already one check with no partial write; successful paths retain a sequential whole-struct store candidate. |
| `CopyToRowMajor`, `TryCopyToRowMajor` | All | Unmeasured | Failure is structurally optimal; successful paths retain exact transpose/shuffle plus bulk-store candidates. |
| Float/double same-shape conversions | All | Unmeasured | Per-column widening/narrowing candidates missing. |
| `Mat3x2` / `System.Numerics.Matrix3x2` conversions | `Mat3x2` | Optimized | Physical field order is compatible; conversion to System reads the private packed view and conversion back writes through the private packed constructor. |
| `Mat4` / `System.Numerics.Matrix4x4` conversions | `Mat4` | Optimized | Conversion to System transposes the private physical view and conversion back transposes into the private packed constructor, preserving AlvorKit's public row/column interpretation. |
| Unary `+` | All | Already optimal | Pass-through/copy semantics. |
| Unary `-` | Float and selected double columns | Optimized | Per-column retained vector operations. |
| Pair `+`, `-`, `/`; matrix-right scalar `*`, `/`; scalar-left `*` | Float | Optimized | Per-column vector operations across all nine shapes. |
| Pair `+`, unary `-`, pair/right-scalar `/` | Two-row double shapes | Optimized | Inherits retained `Vec2d` natural-register paths. |
| Pair/right-scalar `/` | Four-row double shapes | Optimized | Inherits retained `Vec4d` natural-register paths. |
| Mat4d pair `+`/`-`, unary `-`, and right-scalar `*` | `Mat4d` | Optimized | Direct public-column paths capture about 35%–44% with exact component order. |
| Other double pair/scalar arithmetic | Double | Pending direct-column candidate | Underlying vector candidates were rejected, but complete-matrix public-column composition can independently remove indexing and code-size barriers; current-only callers do not decide it. |
| Scalar `+`/`-`, scalar-left `+`/`-`/`/` | All | Unmeasured | Direction-specific public-column vector/scalar composition and register-splat candidates remain; current-only broad callers do not decide them. |
| Remainder `%` in all directions | All | Retained scalar | No equivalent ordinary packed hardware operation; IEEE and operand order must remain exact. |
| `ComponentMultiply` | Float | Optimized | Per-column retained vector multiply. |
| `ComponentMultiply` | Double | Mat4d optimized; other shapes retained/pending | Direct Mat4d columns improve about 35%; other shapes retain prior evidence. |
| Matrix * column vector | Float | System-backed for Mat4 and Mat3x2 transforms; other shapes optimized by vector composition | Mat4 uses `Vector4.Transform` over the transposed physical backend layout. Mat3x2 point/direction transforms use `Vector2.Transform`/`TransformNormal`. Their System evaluation behavior is accepted under the unified backend policy. |
| Row vector * matrix | Float/double | Already optimal | Delegates exact ordered vector `Dot`; reductions remain scalar. Caller coverage incomplete. |
| Matrix * matrix | Float | Mat4 System-backed; Mat3x2 already faster than System; other shapes optimized by composition | Mat4 bit-casts its transposed physical layout and reverses backend operand order so public column-vector multiplication remains unchanged. Mat3x2 retains its faster AlvorKit composition. |
| Matrix/vector and matrix/matrix products | Double | Mat4d optimized; other shapes retained/pending | Mat4d matrix-vector improves about 51%; direct ordered field product improves matrix-matrix about 75.1% and matches scalar ceiling. |
| `Transpose` / `Transposed` | `Mat4`, `Mat4d`; other shapes | Optimized for Mat4/Mat4d; other shapes Unmeasured | Mat4 reads its private packed view and uses the runtime's intrinsic `Matrix4x4.Transpose` shuffle; Mat4d retains direct row construction. Both preserve component bits exactly. |
| Exact/epsilon `Equal`, `NotEqual` | All | Optimized by composition | Exact and epsilon overloads use retained vector comparisons per column and the required `.All` reduction; the public result is one Boolean per column, so a whole-component mask would not preserve the contract. |
| `IsNull` | Applicable shapes | Optimized by composition | Uses retained vector absolute-value, comparison, and `.All` paths per column with exact short-circuit behavior. |
| `IsIdentity` | Applicable shapes | Unmeasured | A per-column `Equal(value, Identity, epsilon).All` candidate preserves NaN and tolerance behavior; current-only callers do not decide it. |
| `IsNormalized`, `IsOrthogonal` | Square | Optimized by composition; ordered reduction retained | Uses vector `LengthSquared` and `Dot`; the required row-and-column checks and Boolean short-circuit order prevent dropping or reassociating reductions. |
| `Trace`, `Determinant`, `Adjugate` | Square | Retained scalar | Trace and generated cofactor formulas preserve fixed reduction order and NaN propagation; packed alternatives require shuffles and reassociation with no natural matrix result layout. |
| `Invert`, `TryInvert`, `Inverted`, inverse transpose | Square | Mat4 System-backed at parity; other square shapes pending | Mat4 calls `Matrix4x4.Invert` directly through its private packed input/output view and restores AlvorKit's default `out` result on failure. The final same-run result was 16.81 ns versus System 16.34 ns (ratio 1.03); non-System shapes retain their pivoted implementation. |
| Affine inverse and orthonormalization | Applicable square shapes | Optimized by composition | Affine inverse composes smaller inverse and matrix-vector primitives; orthonormalization composes ordered vector `Normalize`/`Dot`. Improvements inherit from those primitives. |
| `Mat3x2` translation/scale/rotation/skew/compose/point/vector surfaces | `Mat3x2`, `Mat3x2d` | Already optimal or System-backed where faster | Float inversion bit-casts through the physically compatible System layout while restoring AlvorKit's default failure result; 6.428→1.606 ns. Float point/direction transforms use System. Mat3x2 composition remains local because it already beats System; double remains scalar/vector composition. |
| `Mat3` 2D transforms and quaternion rotation surfaces | `Mat3`, `Mat3d` | Already optimal or optimized by composition | Factories construct fields directly; modifiers and point/vector transforms use matrix-vector primitives; quaternion creation delegates the quaternion conversion primitive. |
| `Mat4` translation/scale/rotation/shear/look/world surfaces | `Mat4`, `Mat4d` | Direct/inherited; sparse products pending | Properties and factories are direct; translation, scale, look, and world paths compose retained vector primitives. Centered rotation and sparse rotation/shear/scale-bias products still have explicit candidates. |
| `Mat4` point/vector, reflection, shadow, plane and quaternion transforms | `Mat4`, `Mat4d` | Optimized by composition; sparse products pending | Point/vector transforms inherit matrix-vector work; reflection/shadow and quaternion creation inherit plane/vector/quaternion primitives; plane transforms inherit inverse-transpose. Centered quaternion and sparse rotate products remain candidates. |
| Projection, viewport, project/unproject, and pick-matrix surfaces | `Mat4`, `Mat4d` | Retained scalar or optimized by composition | Projection factories are fixed scalar formulas, switches, and transcendental calls; float project/pick/unproject inherit the System-backed Mat4 transform/inversion primitives while double paths retain local composition. |
| Formatting and parsing | All | Cold path | Text processing dominates; exact culture/UTF-8/failure contracts apply. Allocation behavior still documented by tests. |
| `CompareTo`, typed/object equality, `==`, `!=`, hashing | All | Retained semantic paths | `CompareTo` must short-circuit lexicographically; equality composes exact column semantics and object type testing; floating bitwise equality would change NaN/signed-zero behavior; hashing preserves the established column composition. |

### Mat4 Production Measurement Closure

The retained rows below use the valid Mat4 Screen and promoted Quick runs. The
first whole-vector Mat4d matrix-product implementation is excluded because its
production rerun regressed; the final ordered direct/inlined implementation has
its own valid Quick run. All retained and baseline methods allocated 0 B.

| Production touch point | Original | Retained | Gain | Evidence |
| --- | ---: | ---: | ---: | --- |
| Mat4 transpose | 6.063 ns | 3.786 ns | 37.6% | `20260712-224120779-p44108-quick-mat4-core-candidates-quick` → `20260712-225057096-p13940-quick-mat4-core-optimized-quick` |
| Mat4 transpose, System parity batch | 7.994 ns | 0.993 ns | 87.6% | Final packed-constructor focused ShortRun; `System.Numerics.Matrix4x4.Transpose` measured 1.007 ns |
| Mat3x2 inverse, System parity batch | 6.428 ns | 1.606 ns | 75.0% | `SystemTypeParityBenchmarks` ShortRun; `System.Numerics.Matrix3x2.Invert` measured 1.579 ns |
| Quat Hamilton product, System parity batch | 4.182 ns | 0.897 ns | 78.6% | `SystemTypeParityBenchmarks` ShortRun; `System.Numerics.Quaternion` measured 1.058 ns in the same run |
| Mat3x2 point transform, System parity batch | 0.776 ns | 0.669 ns | 13.8% | Final full `SystemTypeParityBenchmarks` ShortRun; System measured 0.663 ns |
| Mat4 matrix-matrix, System parity batch | 4.362 ns | 2.847 ns | 34.7% | Final packed-constructor focused ShortRun; System measured 2.630 ns (AlvorKit wrapper ratio 1.08) |
| Mat4 matrix-vector, System parity batch | 1.529 ns | 0.893 ns | 41.6% | Final packed-view focused ShortRun; System measured 0.850 ns (AlvorKit wrapper ratio 1.05) |
| Mat4 inverse, packed-overlay parity | 49.973 ns | 16.81 ns | 66.4% | Focused final ShortRun; System measured 16.34 ns in the same run (ratio 1.03). Absolute inversion time varied with machine state, so same-run ratio is controlling. |
| Quat normalize, System parity batch | 1.577 ns | 1.483 ns | 6.0% | System-backed final focused ShortRun; System measured 1.459 ns |
| Mat4 add, packed-constructor parity | 2.273 ns | 2.118 ns | 6.8% | Final focused ShortRun; System measured 2.226 ns, so AlvorKit was 4.9% faster in the same run |
| Mat4d pair add | 10.330 ns | 6.243 ns | 39.6% | Same valid candidate/production Quick pair |
| Mat4d pair subtract | 9.816 ns | 5.471 ns | 44.3% | Same valid candidate/production Quick pair |
| Mat4d unary negate | 7.639 ns | 4.977 ns | 34.8% | Same valid candidate/production Quick pair |
| Mat4d right-scalar multiply | 7.365 ns | 4.378 ns | 40.6% | Same valid candidate/production Quick pair |
| Mat4d component multiply | 9.725 ns | 6.326 ns | 35.0% | Same valid candidate/production Quick pair |
| Mat4d Lerp | 11.715 ns | 7.161 ns | 38.9% | Same valid candidate/production Quick pair |
| Mat4d matrix-vector | 6.826 ns | 3.358 ns | 50.8% | Same valid candidate/production Quick pair |
| Mat4d transpose | 28.202 ns | 3.142 ns | 88.9% | Same valid candidate/production Quick pair |
| Mat4d matrix-matrix | 44.178 ns | 11.00 ns | 75.1% | `20260712-225933368-p44576-quick-mat4d-matrix-matrix-inline-quick`; retained path matches the 11.12 ns scalar ceiling |

Aggressive-inlining hints on the final Mat4 System wrappers were measured and
rejected. Matrix-matrix moved only 2.847→2.828 ns while its System control moved
2.630→2.649 ns, and matrix-vector regressed 0.893→0.904 ns while its control was
stable at 0.850/0.845 ns. The changes are noise or negative, so the ordinary
small wrappers remain and RyuJIT chooses their inlining policy.

### Explicit Matrix Signature Map

The preceding rows summarize decisions. The rows below are the controlling
signature map where those summaries combine distinct call shapes.

| Touch point | Shapes | Status | Evidence or reason |
| --- | --- | --- | --- |
| Primary/component, column, row, diagonal-scalar, and diagonal-vector construction | Applicable shapes separately | Already optimal | Each body directly constructs the public columns from fields or parameters; the Mat4/Mat3x2 packed overlays occupy the same bytes and add no runtime representation. |
| Outer-product construction | Applicable shapes separately | Unmeasured | A per-column vector-times-row-component candidate remains; the permanent current-only caller provides characterization, not a decision. |
| `AffineIdentity` | `Mat3x2`, `Mat3x2d` | Already optimal | Direct affine constant; distinct from square `Identity`. |
| `Translation` and `Translation2D` get/set | `Mat3x2`, `Mat3x2d`, `Mat3`, `Mat3d`, `Mat4`, `Mat4d` | Already optimal | Affine access is a direct column load/store; 3x3 and 4x4 access directly constructs or replaces only the exposed translation components. |
| Column indexer get/set | All | Already optimal checked access | One unsigned bounds check followed by `Unsafe.Add` preserves the exception contract and selects the sequential column directly. |
| Component indexer get/set | All | Already optimal checked access | Composes the checked column selection with the vector's required row check; there is no packed operation to substitute. |
| `ColumnRef` | All | Already optimal checked access | One bounds check and `Unsafe.Add` return the mutable field reference without copying or allocation. |
| `ComponentRef` | All | Already optimal checked access | Composes `ColumnRef` with the vector's checked component reference and returns the scalar by reference. |
| Span constructor and `FromColumnMajor` | All by complete byte size | Pending bulk-load candidate | One upfront length check plus a sequential unaligned whole-struct load can preserve the exact short-input exception while removing repeated per-component checks. |
| `FromRowMajor` | All by row/column shape | Pending shuffle candidate | One upfront check plus shape-specific transpose/permutation remains a competing implementation. |
| `CopyTo`/`CopyToColumnMajor` | All | Pending bulk-store candidate | One required destination check followed by a sequential whole-struct store can preserve the throwing contract. |
| `TryCopyTo`/`TryCopyToColumnMajor` success and failure | All | Success pending bulk-store; failure optimal | Failure is already one check with no partial write; the successful path retains the sequential whole-struct store candidate. |
| `CopyToRowMajor` | All | Pending shuffle/store candidate | The throwing check remains unchanged; shape-specific transpose plus bulk store is a valid success-path alternative. |
| `TryCopyToRowMajor` success and failure | All | Success pending shuffle/store; failure optimal | Failure is already one check with no partial write; the successful row-major permutation still has an alternative. |
| Float-to-double conversion | All nine shapes | Pending intrinsic conversion candidate | Direct public-column widening with `Vector128`/`Vector256` where the column width fits remains to be compared. |
| Double-to-float conversion | All nine shapes | Pending intrinsic conversion candidate | Direct public-column narrowing remains to be compared separately because exceptional values and register widths differ from widening. |
| `Mat3x2` to/from `System.Numerics.Matrix3x2` | Float only | Optimized | Both types are 24-byte layouts in identical physical field order; the packed overlay is read directly and the reverse direction uses the packed constructor. |
| `Mat4` to/from `System.Numerics.Matrix4x4` | Float only | Optimized | Public semantic conversion transposes the private physical System view; reverse conversion transposes into the packed constructor. |
| Unary `+` | All | Already optimal | Pass-through/copy semantics. |
| Unary `-` | Each scalar and row count | Selective optimized | Per-column vector dependency differs for 2/3/4-row columns. |
| Matrix-pair addition/subtraction | Each compatible same shape | Selective optimized; remaining double candidates pending | Float and measured double shapes inherit retained vector/direct-column paths; remaining double complete callers still need direct-column alternatives. |
| Matrix-pair component division | Each same shape | Selective optimized or retained scalar | Float and two-/four-row double shapes inherit retained vector division; three-row double preserves the rejected scalar vector decision. |
| Matrix-right-scalar addition/subtraction | All | Unmeasured | Public-column scalar composition remains a shape- and direction-specific candidate; the current-only broad caller does not decide it. |
| Scalar-left-matrix addition/subtraction | All | Unmeasured | Public-column scalar composition remains a candidate and subtraction order must remain exact. |
| Matrix-right-scalar multiplication | All | Selective optimized; remaining double candidates pending | Float and Mat4d use retained column paths; other double shapes still need complete-caller direct-column alternatives. |
| Scalar-left-matrix multiplication | All | Float optimized; double pending | Float uses retained per-column multiplication; double public-column composition remains unmeasured. |
| Matrix-right-scalar division | All | Selective optimized or retained scalar | Float and two-/four-row double shapes inherit retained vector division; three-row double retains scalar order. |
| Scalar-left-matrix division | All | Unmeasured | Reciprocal direction, exceptional behavior, and public-column splat alternatives require a competing implementation. |
| Remainder `%` for matrix/matrix, matrix/scalar, and scalar/matrix directions | All directions separately | Retained scalar | No matching ordinary packed hardware operation exists; IEEE behavior and operand direction must remain exact. |
| `ComponentMultiply` | All | Selective optimized; remaining double candidates pending | Float and Mat4d inherit retained/direct column multiplication; other double shapes still need complete-caller direct-column alternatives. |
| Matrix-times-column-vector | Every compatible shape | Selective optimized; remaining double candidates pending | Float and Mat4d use ordered column linear combinations; other double shapes still need direct-column alternatives. |
| Row-vector-times-matrix | Every compatible shape | Optimized by composition | Each output component delegates the exact ordered vector `Dot`; the remaining work is the required result-width construction. |
| Matrix-times-matrix | Every compatible shape pair | Selective optimized; remaining double candidates pending | Float composes optimized matrix-vector columns and Mat4d has a retained direct field product; other double pairs still need direct-column alternatives. |
| `Transpose` and `Transposed` | Each shape/direction | Mat4/Mat4d optimized; other shapes Unmeasured | Direct row construction is retained for 4x4 and remains a valid alternative for every other shape. |
| Exact `Equal`/`NotEqual` and epsilon overloads | Each shape | Optimized by composition | Uses retained vector comparisons per column and the required `.All` reduction to produce one Boolean per column. |
| `IsNull` | Applicable shapes separately | Optimized by composition | Uses retained vector absolute-value, comparison, and `.All` primitives with short-circuit evaluation. |
| `IsIdentity` | Applicable shapes separately | Unmeasured | Per-column equality against `Identity` remains a valid alternative to scalar indexed comparisons. |
| `IsNormalized` and `IsOrthogonal` | Square shapes separately | Optimized by composition; ordered reduction retained | Uses vector `LengthSquared`/`Dot`; the row-and-column populations and Boolean order are part of the tolerance contract. |
| `Trace` | Square shapes | Retained scalar | Direct fixed-order diagonal reduction has no natural packed result. |
| `Determinant` | Square and affine shapes separately | Retained scalar | Fully expanded fixed-order formulas preserve floating rounding and NaN propagation; alternate algorithms are not semantics-equivalent. |
| `Adjugate` | Square shapes | Retained scalar | Fully expanded cofactors preserve component order; packing requires cross-cofactor shuffles without a natural lane layout. |
| Throwing `Invert`/`Inverted` | Square and affine shapes separately | Pending exact row-kernel candidate | Throwing behavior delegates the same inversion primitive; vectorized augmented-row arithmetic remains possible without changing singular handling. |
| `TryInvert` success/failure | Square and affine shapes separately | Pending exact row-kernel candidate | Pivot selection, Boolean failure, and output must remain unchanged; only row normalization/elimination is eligible for packing. |
| `InverseTranspose`/`InverseTransposed` | Square shapes | Inherits pending inversion; transpose selective optimized | This is exact composition of the inversion primitive and shape-specific transpose. |
| `AffineInverse`/`AffineInverted` | Applicable shapes | Optimized by composition | Decomposes into the smaller inverse and retained matrix-vector operations; gains inherit from those primitives. |
| `Orthonormalize`/`Orthonormalized` | Applicable square shapes | Optimized by composition; ordered reduction retained | Ordered Gram-Schmidt composes vector `Normalize` and `Dot`; reassociation would change floating semantics. |
| `Mat3x2` create/compose/modify/transform operations | Float and double separately | Already optimal or optimized by composition | Factories and specialized inverse are direct scalar bodies; modifiers, composition, and point/vector transforms use retained column-vector primitives; trig dominates rotation/skew factories. |
| `Mat3` 2D factories/modifiers and point/vector transforms | Float and double separately | Already optimal or optimized by composition | Factories construct fields directly; modifiers and homogeneous point/vector transforms compose retained matrix/vector paths. |
| `Mat3` quaternion rotation create/modify | Float and double separately | Optimized by composition | Creation delegates quaternion-to-matrix conversion and modification composes the matrix product primitive. |
| `Mat4` `Translation`, `CreateTranslation`, `Translate`, and `WithoutTranslation` | Float and double separately | Already optimal or optimized by composition | Property/factory/clearing paths directly touch the translation column; `Translate` is the exact retained column linear combination. |
| `Mat4` scale factories/modifiers and `ExtractScale` | Float and double separately | Already optimal or optimized by composition | Factories directly construct sparse columns, modifiers scale existing columns, and extraction composes vector length primitives. |
| `Mat4` axis/general rotation factories and modifiers | Float and double separately | Direct factories; sparse products pending | Uncentered factories are direct scalar/trigonometric construction. Centered factories and rotation modifiers currently multiply sparse matrices and retain explicit specialized-column candidates. |
| `Mat4` shear factories/modifiers | Float and double separately | Direct factory; sparse product pending | The factory directly constructs the sparse matrix; the modifier still has a specialized-column alternative to general matrix multiplication. |
| `LookAt`, `LookTo`, and `CreateWorld` | Float and double, handedness overloads separately | Optimized by composition; ordered branches retained | Bodies compose vector Normalize/Cross/Dot and direct construction; handedness switches and operation order are public floating behavior. |
| `CreateScaleBias` and `ScaleBias` | `Mat4`, `Mat4d` | Direct factory; sparse product pending | `CreateScaleBias` directly constructs the sparse result; `ScaleBias` retains a specialized-column alternative to general multiplication. |
| `MatrixCross3` and `MatrixCross4` | `Mat4`, `Mat4d` | Already optimal | Direct field construction performs only the required signs, copies, and zero fills. |
| `CreateReflection` and `CreateShadow` | `Mat4`, `Mat4d` | Optimized by composition; scalar formula retained | Plane normalization and vector operations use their retained primitives; the remaining component formula has no independent packed layout. |
| Quaternion `CreateRotation` and `Rotate` | `Mat3/4`, `Mat3d/4d` | Optimized by composition; sparse products pending | Creation delegates retained quaternion conversion; centered creation and modification inherit matrix multiplication but retain sparse specialized-column candidates. |
| Orthographic factories and off-center overloads | `Mat4`, `Mat4d` by depth/handedness | Retained scalar | Fixed small formulas directly construct sparse columns; depth/handedness switches and exceptional behavior dominate and admit no independent packed result. |
| Perspective/frustum factories, including infinite and tweaked infinite | `Mat4`, `Mat4d` by depth/handedness | Retained scalar | Fixed formulas, validation, switches, and scalar transcendental/divide calls directly construct sparse columns. |
| `CreateViewport`, `Project`, and `UnProject` | `Mat4`, `Mat4d` by depth range | Retained scalar or optimized by composition | Viewport is a direct scalar/switch formula; Project composes matrix-vector and vector division; UnProject inherits matrix product and the pending exact inversion primitive. |
| `PickMatrix` | `Mat4`, `Mat4d` | Already optimal or optimized by composition | Required invalid-delta branch precedes direct translation/scale construction and retained transform composition. |
| `ToString` overloads | All | Cold path | String result allocates and culture dominates. |
| UTF-16 `TryFormat` | All | Cold path | Short-char-destination false return is distinct. |
| UTF-8 `TryFormat` | All | Cold path | Transcoding and byte destination are distinct. |
| String, UTF-16 span, and UTF-8 span `Parse` | All inputs separately | Cold path | Throwing failure and encoding are distinct contracts. |
| String, UTF-16 span, and UTF-8 span `TryParse` | All inputs separately | Cold path | Boolean failure and encoding are distinct contracts. |
| `CompareTo` | All | Retained semantic path | Lexicographic early exit is required; SIMD comparison would evaluate later columns and still require the same ordered first-difference reduction. |
| Typed/object equality, `==`, and `!=` | All separately | Retained semantic paths | Typed/operators compose exact column equality; object equality adds the required type test. Bitwise floating comparison would change NaN and signed-zero semantics. |
| `GetHashCode` | All | Retained semantic path | Preserves the established `HashCode.Combine` column composition and equality contract; matrix packing is not an independent hash primitive. |
| Generic `IMat*` dispatch | All 23 public matrix interfaces | Already optimal for closed generic callers | Static-abstract calls are specialized to the concrete matrix type by closed generic instantiation; there is no separate SIMD body to substitute at the interface layer. |

## Quaternion Touch Points

| Touch point | Shapes | Status | Evidence or reason |
| --- | --- | --- | --- |
| Layout, fields, constructors, `Zero`, `Identity` | `Quat` | Optimized | Explicit layout overlays a private `System.Numerics.Quaternion` on the unchanged public X/Y/Z/W fields and 16-byte size. |
| Layout, fields, constructors, `Zero`, `Identity` | `Quatd` | Already optimal | Direct sequential struct state; no matching System.Numerics double quaternion exists. |
| Vector/scalar properties, indexer, `ComponentRef`, deconstruction | Both | Unmeasured | Direct/ref access; bounds contract applies to indexer. |
| Span construction, `CopyTo`, `TryCopyTo` | Both | Unmeasured | Allocation-free scalar load/store; bulk candidate missing. |
| `LengthSquared`, `Length`, `Dot` | Both | Retained scalar | Ordered horizontal reductions are faster and preserve exact IEEE order. |
| Normalize, inverse, conjugate | Both | Float normalize System-backed; other paths optimized | Float normalize uses `System.Numerics.Quaternion.Normalize` and measured 1.483 ns versus System 1.459 ns. Conjugate retains its faster exact sign-bit XOR; double has no System counterpart. |
| Fallback/try normalization and inversion, identity/normalized queries | Both | Unmeasured | Branch behavior is exactness-covered; dedicated caller benchmarks missing. |
| Lane-independent pair/scalar arithmetic and negate | Both | Optimized | `Quat` reads and constructs through its private System quaternion view; `Quatd` uses equal-layout `Vector256<double>` operations. |
| Unary `+` | Both | Already optimal | Pass-through. |
| Hamilton multiplication and quaternion division | Both use the System evaluation family | Measured and exact-tested | Float Hamilton uses the private packed System representation and a zero-cost bit-cast result: 4.182 to 0.897 ns versus System's 1.058 ns in the same run. `Quatd` now applies the same packed DirectX/System shuffle-and-multiply-add algorithm with Vector256 doubles; a 4,096-item ShortRun measured 1.361 ns versus 1.785 ns for the former scalar formula, 23.8% faster with 0 B allocated. Both widths share exceptional-value propagation policy. Quaternion division inherits the selected Hamilton product. |
| Named arithmetic wrappers | Both | Already optimal | Directly inherit the retained operator implementation. |
| `TransformVector` | `Quat` | Optimized | Exact direct `System.Numerics.Vector3` path. |
| `TransformVector` | `Quatd` | Retained scalar | Padded Vector256 was 2.4x slower; flattened scalar ILP retained. |
| Axis/euler/yaw-pitch-roll/matrix/look/between factories | Both | Unmeasured | Transcendental and branch-heavy complete callers missing. |
| Angle/axis/euler/pitch/yaw/roll extraction and matrix conversion | Both | Unmeasured | Complete-caller benchmarks missing. |
| `Lerp`, `Nlerp`, `Slerp` | Both | Optimized/local semantics | AlvorKit `Lerp` deliberately does not normalize while System's helper does, so it is not an equivalent backend substitution. The local interpolation family retains the AlvorKit operation definitions. |
| Extra-spin Slerp, Squad, squad control | Both | Unmeasured | Branch/transcendental caller coverage missing. |
| `Exp`, `Log`, `Pow`, quaternion `Sqrt` | Both | Unmeasured | Exact transcendental formulas; no equivalent approximate substitution allowed. |
| Exact/tolerance relations and relational operators | Both | Unmeasured | Delegated vector masks and scalar tolerance formulas; caller materialization cost missing. |
| `IsNaN`, `IsInfinity`, `IsFinite` | Both | Unmeasured | Delegated masks; caller benchmark missing. |
| Float/double conversion | Both | Unmeasured | Widen/narrow cost missing. |
| `Quat` / `System.Numerics.Quaternion` conversion | `Quat` | Optimized and implicit | Direct read/construction through the private exact-layout view in both directions. |
| Tuple conversion | Both | Unmeasured | Direct construction/extraction; caller benchmark missing. |
| Formatting and parsing | Both | Cold path | Text/culture/UTF-8 work dominates and exact failure behavior applies. |
| `CompareTo`, equality, `==`, `!=`, hashing | Both | Unmeasured | Exact value semantics; consolidated caller benchmark missing. |

### Explicit Quaternion Signature Map

| Touch point | Shapes | Status | Evidence or reason |
| --- | --- | --- | --- |
| Primary/component/span construction and `Create` | `Quat`, `Quatd` separately | `Quat` layout exact-validated; infrastructure suite otherwise pending | `Quat` public fields retain offsets 0/4/8/12 and size 16 around its private System view. Span short-input exception remains distinct from direct construction. |
| `Vector`/`Scalar` and X/Y/Z/W property get/set | Both | Infrastructure suite added; execution pending | Vector copy and scalar field access are distinct. |
| Indexer get/set | Both | Infrastructure suite added; execution pending | Bounds and mutable access differ from property access. |
| `ComponentRef` | Both | Infrastructure suite added; execution pending | By-ref return is a distinct generic/concrete call shape. |
| `Deconstruct` | Both | Infrastructure suite added; execution pending | Direct four-field copy. |
| Array `CopyTo` overloads | Both | Infrastructure suite added; execution pending | Array bounds/offset behavior differs from span. |
| Span `CopyTo` | Both | Infrastructure suite added; execution pending | Throwing short destination differs from Try. |
| Span `TryCopyTo` success/failure | Both | Infrastructure suite added; execution pending | Boolean failure and no-partial-write contract are explicit. |
| `LengthSquared` | Both | Retained scalar | Ordered four-term reduction. |
| `Length` | Both | Retained scalar | Root after ordered reduction. |
| `Dot` | Both | Permanent reduction/throughput suite exists | Retained ordered reduction evidence unchanged. |
| `Normalize`/`Normalized` | Both | Optimized | Unconditional division path. |
| `NormalizedOr`/`NormalizedOrIdentity` | Both | Permanent fallback suite added; execution pending | Zero-length branch and fallback copy are distinct. |
| `TryNormalize` success/failure | Both | Permanent fallback suite added; execution pending | Boolean/out-result contract. |
| `Invert`/`Inverted` | Both | Optimized | Unconditional inverse path. |
| `TryInvert` success/failure | Both | Permanent fallback suite added; execution pending | Zero-norm Boolean failure is distinct. |
| `Conjugate`/`Conjugated` | Both | Optimized | Exact sign-bit operation, separate from inverse. |
| `IsIdentity` | Both | Permanent fallback suite added; execution pending | Exact identity comparison. |
| `IsNormalized` | Both | Permanent fallback suite added; execution pending | Tolerance/reduction query. |
| Unary `+` | Both | Already optimal | Pass-through. |
| Unary `-` | Both | Optimized | Four independent lane negations. |
| Pair addition/subtraction | Both | Optimized | Lane-independent pair operations. |
| Quaternion/scalar and scalar/quaternion multiply | Both directions | Optimized | Splat direction and overload dispatch remain distinct callers. |
| Quaternion/scalar division | Both | Optimized | Packed lane division. |
| Scalar/quaternion division | Both | Unmeasured | Reciprocal direction is a separate formula. |
| Hamilton quaternion multiplication | `Quat` | Optimized | Packed System Hamilton path is at throughput parity: 0.897 ns versus System's 1.058 ns in the same ShortRun, with 0 B allocated. Exceptional IEEE inputs now follow System's component-bit behavior. |
| Hamilton quaternion multiplication | `Quatd` | System-style packed path | Vector256 uses the same shuffle, sign, and multiply-add evaluation sequence as System.Numerics at double width. Exact behavior passes with hardware intrinsics enabled and disabled. A 4,096-item ShortRun measured 1.361 ns versus 1.785 ns for the former scalar formula, 23.8% faster with 0 B allocated. |
| Quaternion division | `Quat` | Optimized by composition | Inverse plus the optimized packed Hamilton product; separate from scalar division. |
| Quaternion division | `Quatd` | System-style by composition | Inverse plus the packed System-style Hamilton product; separate from scalar division. |
| Named `Add`, `Subtract`, `Negate`, `Multiply`, `Divide` wrappers | Both per overload | Wrapper evidence partial | Delegation should be verified without conflating scalar and quaternion overloads. |
| `TransformVector` | `Quat` | Optimized | Exact System.Numerics path. |
| `TransformVector` | `Quatd` | Retained scalar | Padded candidate lost. |
| `CreateFromAxisAngle` | Both | Permanent factory suite added; execution pending | Normalize/trigonometric caller. |
| `CreateFromEulerAngles` | Both | Permanent factory suite added; execution pending | Euler ordering is distinct. |
| `CreateFromYawPitchRoll` | Both | Permanent factory suite added; execution pending | Parameter/order wrapper remains a separate public call. |
| `CreateFromRotationMatrix` Mat3/Mat4 overloads | Both matrix sizes separately | Permanent factory suite added; execution pending | Branch/formula differs by source matrix shape. |
| `CreateRotationBetween` | Both | Permanent factory suite added; execution pending | Parallel/opposite vector branches. |
| `LookRotation` default/right/left overloads | Both handedness populations | Permanent factory suite added; execution pending | Handedness and fallback branches are explicit. |
| `Angle`, `Axis`, and `ToAxisAngle` | Both separately | Permanent extraction suite added; execution pending | Property extraction and tuple/out extraction differ. |
| `EulerAngles`, `Pitch`, `Yaw`, and `Roll` | Both separately | Permanent extraction suite added; execution pending | Each has a distinct formula/branch. |
| `ToMat3` and `ToMat4` | Both target sizes | Permanent conversion suite added; execution pending | Result size and homogeneous row/column work differ. |
| `Lerp` | Both | Optimized | Permanent compound suite exists. |
| `Nlerp` | Both | Optimized | Normalize-after-lerp dependency differs. |
| Standard `Slerp` | Both | Optimized | Branch/transcendental caller. |
| Extra-spin `Slerp` | Both | Permanent advanced suite added; execution pending | Extra-spin angle formula is distinct. |
| `CreateSquadControlPoint` | Both | Permanent advanced suite added; execution pending | Log/Exp compound caller. |
| `Squad` | Both | Permanent advanced suite added; execution pending | Multiple Slerp dependency chain. |
| `Exp` | Both | Permanent advanced suite added; execution pending | Quaternion transcendental formula. |
| `Log` | Both | Permanent advanced suite added; execution pending | Domain/normalization differs from Exp. |
| `Pow` | Both | Permanent advanced suite added; execution pending | Scalar exponent plus Log/Exp composition. |
| Quaternion `Sqrt` | Both | Permanent advanced suite added; execution pending | Branch and half-angle formula. |
| Exact `Equal`/`NotEqual` methods | Both | Permanent relation suite added; execution pending | Component mask reduction. |
| Tolerance `Equal`/`NotEqual` overloads | Both | Permanent relation suite added; execution pending | Epsilon arithmetic is distinct. |
| Relational methods and `< <= > >=` operators | Both families separately | Permanent relation suite added; execution pending | Public Boolean mask materialization and operator wrapper cost. |
| `IsNaN`, `IsInfinity`, `IsFinite` | Both separately | Permanent classification suite added; execution pending | Three distinct component classifications. |
| Float/double conversion | Both directions | Permanent conversion suite added; execution pending | Narrowing and widening differ. |
| `Quat` to/from `System.Numerics.Quaternion` | Float, both directions | Optimized | Direct private-view read/construction; exact component bits and public offsets are validated. |
| Tuple conversion | Both directions and scalar widths | Permanent conversion suite added; execution pending | Construction and extraction are distinct. |
| `ToString` overloads | Both | Cold path | Allocating string/culture path. |
| UTF-16 and UTF-8 `TryFormat` | Both encodings separately | Cold path | Encoding and destination-failure contracts differ. |
| String/UTF-16/UTF-8 `Parse` | Both inputs separately | Cold path | Throwing failure and encoding differ. |
| String/UTF-16/UTF-8 `TryParse` | Both inputs separately | Cold path | Boolean failure and encoding differ. |
| `CompareTo` | Both | Permanent value suite added; execution pending | Lexicographic early exit. |
| Typed/object equality and `==`/`!=` | Both calls separately | Permanent value suite added; execution pending | Object type-test/boxing differs. |
| `GetHashCode` | Both | Permanent value suite added; execution pending | Four-component hash composition. |
| Generic `IQuat*` dispatch | All four public quaternion interfaces | Representative caller authored; remaining Unmeasured | Quat/Quatd `IQuatInterpolation.Lerp` direct-versus-generic categories await execution; every other interface/member shape remains. |

## Compound Value Infrastructure

These rows apply to all 28 compound types unless a family has no matching
member. Boxes use `Dimension`; most other families expose `SizeInBytes`.

| Touch point | Status | Evidence or reason |
| --- | --- | --- |
| Layout constants, public fields, primary/component constructors | Already optimal | Compile-time metadata, sequential public state, and direct initialization leave no runtime transformation. |
| Span constructor | Pending bulk-load candidate | One upfront length check plus a sequential unaligned whole-value load can preserve the exact short-input exception while removing repeated component checks. |
| `Create(ReadOnlySpan<T>)` | Inherits span-constructor decision | The static body is a direct wrapper; inlining removes any independent work. |
| Scalar indexer get/set | Already optimal checked access | Required bounds check plus direct sequential component access; unchecked substitution would change the exception contract. |
| `ComponentRef` | Already optimal checked access | Returns the same checked sequential component by reference without copying or allocation. |
| `Deconstruct` | Already optimal | Direct field copies to caller-owned `out` values; output arity is the public contract. |
| Span `CopyTo` | Pending bulk-store candidate | One required destination check plus a sequential whole-value store can preserve throwing behavior. |
| Span `TryCopyTo` success | Pending bulk-store candidate | Successful paths retain a sequential whole-value store alternative. |
| Span `TryCopyTo` short-destination failure | Already optimal | One length check returns false before any write; no partial-write work remains. |
| Tuple conversion into value | Already optimal | Direct field construction with no intermediate representation. |
| Tuple conversion out of value | Already optimal | Direct field extraction/copy with no intermediate representation. |
| Float-to-double conversion | Pending intrinsic conversion candidate | Per-vector widening may use retained vector conversion paths; complete layouts require a competing candidate. |
| Double-to-float conversion | Pending intrinsic conversion candidate | Narrowing is direction-specific and must preserve exceptional values. |
| Integer/float/double box conversions | Pending vector-conversion candidate | Direct Min/Max vector conversion may inherit packed conversion work; signedness and narrowing remain direction-specific. |
| `CompareTo` | Retained semantic path | Lexicographic first-difference and early exit are required; packed comparison still needs the same ordered reduction. |
| Typed equality | Retained semantic composition | Exact field equality composes nested value semantics; bitwise floating equality would change NaN and signed-zero behavior. |
| Object equality | Ineligible/Cold contract | Required object type test and caller boxing are not removable by the value implementation. |
| `==` and `!=` | Already optimal wrappers | Directly delegate typed equality and its negation; inlining removes independent work. |
| `GetHashCode` | Retained semantic composition | Preserves nested-value hashing and the equality contract; packing is not an independent hash primitive. |
| `ToString` overloads | Cold path | String allocation and culture formatting dominate. |
| UTF-16 `TryFormat` | Cold path | Character destination and short-buffer failure. |
| UTF-8 `TryFormat` | Cold path | Transcoding and byte destination are distinct. |
| String `Parse` | Cold path | Allocated input and throwing failure. |
| String `TryParse` | Cold path | Null handling and Boolean failure. |
| UTF-16 span `Parse` | Cold path | Allocation-free input and throwing failure. |
| UTF-16 span `TryParse` | Cold path | Allocation-free input and Boolean failure. |
| UTF-8 span `Parse` | Cold path | Byte parsing and throwing failure. |
| UTF-8 span `TryParse` | Cold path | Byte parsing and Boolean failure. |
| Generic `IBox*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete box type; there is no separate packed body at the interface layer. |
| Generic `IInterval*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete interval type. |
| Generic `IPlane*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete plane type. |
| Generic `ISphere*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete sphere type. |
| Generic `ISegment*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete segment type. |
| Generic `IRay*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete ray type. |
| Generic `ICapsule*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete capsule type. |
| Generic `ITriangle*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete triangle type. |
| Generic `IObb*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete OBB type. |
| Generic `IFrustum*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete frustum type. |
| Generic `IQuad*` dispatch | Already optimal for closed generic callers | Static-abstract calls specialize to the concrete quad type. |

## Compound Geometry Touch Points

Family summaries below distinguish direct construction, optimized primitive
composition, contract-bound scalar control flow, and genuine competing
candidates. Authored current-only suites are characterization coverage, not
proof of optimality. A surface can nevertheless close structurally when its
body is only direct fields, construction, an inline wrapper, or required branch
policy; vector/matrix/quaternion composition inherits the documented decision
of those primitives.

| Family | Public geometry touch points | Current status and next evidence |
| --- | --- | --- |
| `Box2/3`, `Box2/3d`, `Box2/3i` | Empty/min/max/size/center/half-size/is-empty; width/height/depth/area/volume; corner/center-size factories; normalize; inclusive/half-open/exclusive point and box containment; inclusive/exclusive intersection; sphere and segment tests; closest point/distance; inflate/translate/scale; including/union/intersection | Already optimal direct construction or optimized by vector composition. Inclusive/half-open/exclusive policies require their existing ordered comparisons and branches; compound wrappers add no independent packed work. |
| `Intervalf/d` | Empty/min/max/is-empty/length/center; endpoint factory; value/interval containment; intersection; union/intersection | Retained scalar. Two scalar endpoints, ordered Min/Max, empty sentinels, and short-circuit branches provide no useful packed width. |
| `Plane3/d` | Normal/offset/coefficients/zero; length/normalize/fallback/try/flipped; construction from coefficients/point/three points; evaluate/classify/dot/distance/closest/project/reflect; box/sphere/OBB classification; matrix/quaternion transform; unary signs; array copy; float System Plane conversion | Direct or optimized by vector/matrix/quaternion composition; ordered reductions and classification branches are retained scalar. Equal-layout `Plane3`/System conversions are implicit direct whole-value bit-casts in both directions. |
| `Sphere3/d` | Empty/center/radius/diameter/radius-squared/is-empty; create from box/point span; try-create; point/sphere containment and intersection; closest point/distance | Direct or optimized by vector composition; containment/distance branches are retained. Point-span min/max accumulation remains the only family-level competing candidate. |
| `Segment3/d` | Start/end/center/direction/length; point-at; closest/distance; sphere/AABB/plane intersection and returned amount | Optimized by vector composition with retained scalar Dot/slab/parallel branches. Boolean wrappers delegate the same Try primitive and add no independent work. |
| `Ray3/d` | Origin/direction; point-at/translate/normalize/try; plane/box/sphere/frustum intersections; nearest distance and interval overloads | Optimized by vector composition with retained scalar slab, quadratic, plane, and frustum early-exit policies; Boolean wrappers delegate Try results. |
| `Capsule3/d` | Segment/start/end/center/direction/length/radius/empty; point-at; point/sphere containment; box/sphere/capsule/plane/frustum/ray intersections; frustum classification; ray distance; closest/distance; capsule/capsule static test | Optimized by segment/vector composition with retained scalar clamp, closest-feature, plane-loop, and hit/miss branches; static/instance wrappers add no independent work. |
| `Triangle3/d` | Vertices/edges; normals/plane/area/degeneracy/try-get; barycentric/containment; box/sphere/ray intersection; ray distance; closest/distance | Optimized by vector composition with retained scalar Voronoi-region, degeneracy, SAT, and hit/miss branches whose order determines the selected feature and outputs. |
| `Obb3/d` | Center/half-size/orientation/size/empty; AABB creation/transform; corner copy; point/sphere/OBB containment; AABB/sphere/OBB/plane/frustum intersection; closest/distance; enclosing AABB; static OBB test | Optimized by vector/quaternion composition with retained scalar SAT axis order, early exits, and stack-buffer corner traversal; wrappers add no independent work. |
| `Frustum3/d` | Six planes/counts; create/try from planes or clip transform; plane/normalized-plane/corner copy; finite corners/bounds; AABB conservative/precise containment/classification; frustum/sphere/OBB/capsule classification; transform/try | Optimized by plane/matrix composition with retained scalar six-plane loops, conservative/precise policies, finite-corner validation, SAT order, and early exits. |
| `Quad3/d` | Four corners, center, AABB bounds | Optimized by composition. Center and bounds directly compose retained vector add/min/max; no independent algorithm remains. |
| `Viewport/d` | Bounds/depth/size/center; viewport vector/transform; project/unproject; pick ray; pick matrix; explicit depth conventions | Direct properties/conversions or optimized by matrix/vector composition; depth-range switches, perspective divide, and failure branches are retained contract work. |

### Explicit Compound Geometry Signature Map

The family summaries above do not merge the following distinct algorithms or
failure contracts.

| Family | Touch point | Status and evidence |
| --- | --- | --- |
| Boxes | Empty/min/max/size/center/half-size and dimension/measure properties | Infrastructure only; integer and exceptional populations remain. |
| Boxes | Corner, center-size, and center-half-size factories | Already optimal direct construction. |
| Boxes | `Normalize` and `Normalized` | Optimized by vector Min/Max composition; mutation and returned-copy forms share the same primitive. |
| Boxes | Inclusive point containment | Permanent float/double inside/outside categories added. |
| Boxes | Half-open and exclusive point containment | Retained ordered comparisons; boundary policy and short-circuit semantics are the work. |
| Boxes | Inclusive, half-open, and exclusive box containment | Retained ordered comparisons by policy; vector predicates and required reductions are already composed directly. |
| Boxes | Inclusive and exclusive box intersection | Inclusive float/double hit/miss measured; exclusive and Int32 remain. |
| Boxes | Sphere and segment intersection | Optimized by sphere/segment/vector composition with retained slab and empty-shape branches. |
| Boxes | Closest point, squared distance, and distance | Permanent float/double inside/outside categories added; Int32 boxes remain. |
| Boxes | Inflate/Inflated, Translate/Translated, and Scale/Scaled | Optimized by direct vector arithmetic; mutation/return wrappers add no independent work. |
| Boxes | `Including` point/box, `Union`, and `Intersection` | Union float/double measured; other combination bodies remain. |
| Intervals | Empty/is-empty/min/max/length/center | Already optimal or retained scalar; direct fields plus one required empty-sentinel branch. |
| Intervals | `CreateFromEndpoints` | Already optimal scalar Min/Max endpoint construction. |
| Intervals | Value containment | Permanent hit/miss/empty exact categories added. |
| Intervals | Interval containment and intersection test | Permanent hit/miss/empty exact categories added. |
| Intervals | `Union` and `Intersection` results | Permanent overlap/disjoint/empty exact categories added. |
| Planes | Normal/offset/coefficients/length properties and `Zero` | Direct fields/constants or vector length composition; no independent packed work. |
| Planes | Coefficient and point-normal construction | Already optimal direct construction; point-normal offset is the required ordered Dot reduction. |
| Planes | Throwing three-point creation | Inherits `TryCreateFromPoints`; the degenerate exception is required failure-path contract work. |
| Planes | `TryCreateFromPoints` success/failure | Optimized by vector Cross/Normalize/Dot composition with the required degeneracy branch and `out` result. |
| Planes | `Normalize`/`Normalized` | Optimized by vector length/division composition. |
| Planes | `NormalizedOr` and `TryNormalize` success/failure | Retained fallback/Boolean branch around the same optimized normalization primitive. |
| Planes | `Flip`/`Flipped` and unary signs | Already optimal direct signs; mutation, copy, and operator forms inline to the same field negation. |
| Planes | `Evaluate`, signed/absolute distance, `Dot`, and `DotNormal` | Evaluate/classify/distance suites added; remaining reduction wrappers incomplete. |
| Planes | Point `Classify` | Permanent negative/on/positive categories added. |
| Planes | Box, sphere, and OBB classification | Optimized by volume/vector composition with retained signed-distance classification branches. |
| Planes | Closest/project/reflect point | Optimized by vector Dot and arithmetic composition; ordered reduction remains scalar. |
| Planes | Matrix transform and `TryTransform` success/failure | Current matrix transform measured; singular Try path remains. |
| Planes | Quaternion transform | Permanent float/double category added. |
| Planes | Float/System and float/double conversions | `Plane3`/System is optimized as a direct equal-layout bit-cast. Cross-scalar vector conversions remain candidates; widening and exceptional narrowing are direction-specific. |
| Spheres | Empty/center/radius/diameter/radius-squared/is-empty | Geometry infrastructure only. |
| Spheres | `CreateFromBox` | Optimized by direct box-center and vector-length composition. |
| Spheres | Throwing `CreateFromPoints` | Inherits the pending point-span accumulator; empty-input exception is required failure-path work. |
| Spheres | `TryCreateFromPoints` success/failure | Pending point-span min/max/reduction candidate; Boolean failure and `out` result must remain exact. |
| Spheres | Point/sphere containment | Permanent inside/outside/empty and hit/reject categories added. |
| Spheres | Sphere intersection | Permanent hit/miss/empty categories added. |
| Spheres | Closest point, squared distance, and distance | Permanent branch categories added. |
| Segments | Center/direction/length-squared/length and `PointAt` | Permanent float/double exact categories added. |
| Segments | Closest point and distance face/interior/clamped/degenerate paths | Permanent branch categories added. |
| Segments | Sphere `Intersects` and `TryIntersect`/amount | Hit/miss/empty categories added; wrapper/out forms remain aggregated. |
| Segments | Box `Intersects` and `TryIntersect`/amount | Hit/miss/empty categories added; slab output forms remain aggregated. |
| Segments | Plane `Intersects` and `TryIntersect`/amount | Hit/outside/parallel categories added; wrapper/out forms remain aggregated. |
| Rays | Point-at, translate, normalize, and normalized properties | Direct fields or optimized vector arithmetic/Normalize composition. |
| Rays | `TryNormalize` success/failure | Retained degeneracy branch around vector normalization with exact Boolean/`out` behavior. |
| Rays | Plane Boolean intersection vs nearest-distance Try overload | Hit/miss/parallel categories added, but public forms remain aggregated. |
| Rays | Box Boolean intersection vs interval-return Try overload | Hit/miss/parallel categories added, but interval output requires direct evidence. |
| Rays | Sphere Boolean intersection vs nearest-distance Try overload | Hit/miss categories added; tangent/origin-inside remain. |
| Rays | Frustum Boolean intersection vs interval-return Try overload | Retained six-plane early-exit loop; Boolean wrapper delegates the interval-return Try primitive. |
| Capsules | Segment-derived properties, radius measures, empty query, and `PointAt` | Permanent float/double property categories added. |
| Capsules | Point and sphere containment | Permanent categories added; boundary/empty populations remain incomplete. |
| Capsules | Box intersection | Permanent category added; hit/miss/empty populations remain aggregated. |
| Capsules | Sphere intersection | Permanent category added; hit/miss/tangent populations remain aggregated. |
| Capsules | Instance/static capsule intersection | Permanent category added; static wrapper and branch populations remain aggregated. |
| Capsules | Plane intersection | Permanent category added; side/tangent populations remain aggregated. |
| Capsules | Frustum intersection and classification | Permanent categories added; contains/intersects/disjoint populations remain incomplete. |
| Capsules | Ray Boolean intersection vs nearest-distance Try overload | Permanent categories added; public forms remain aggregated. |
| Capsules | Closest point, squared distance, and distance | Permanent categories added; endpoint/interior populations remain incomplete. |
| Triangles | Vertex/edge/normal/plane/area/degenerate properties | Permanent caller categories added. |
| Triangles | `TryGetNormal` and `TryGetPlane` success/failure | Permanent categories added; degenerate failure population must remain explicit. |
| Triangles | Barycentric and point containment | Permanent categories added; face/edge/vertex populations remain incomplete. |
| Triangles | Box SAT intersection | Permanent category added; hit/miss/degenerate populations remain aggregated. |
| Triangles | Sphere intersection | Permanent category added; face/edge/vertex contact remains aggregated. |
| Triangles | Ray Boolean intersection vs distance-return Try overload | Permanent categories added; hit/miss/backface/degenerate populations remain incomplete. |
| Triangles | Closest point, squared distance, and distance | Permanent categories added; Voronoi-region populations remain incomplete. |
| OBBs | Center/half-size/orientation/size/empty properties and `CreateFromBox` | Factory category added; property callers are infrastructure-only. |
| OBBs | Static `Transform(Box, matrix)` | Permanent float/double category added. |
| OBBs | `CopyCornersTo` vs `TryCopyCornersTo` success/failure | Success category added; throwing and short-destination failure remain. |
| OBBs | Point and sphere containment | Exact scalar inside/outside/empty and hit/miss categories added. |
| OBBs | OBB containment | Contains/disjoint categories added; rotated/intersecting containment remains. |
| OBBs | Box/sphere/plane/frustum intersection wrappers | Permanent current categories added; branch populations remain incomplete. |
| OBBs | Instance OBB intersection vs static SAT | Static/instance wrappers share the retained ordered SAT primitive; no independent call-shape optimization remains. |
| OBBs | Closest point, squared distance, and distance | Exact inside/outside categories added. |
| OBBs | `TryCreateBoundingBox` success/failure | Success category added; empty/failure path remains. |
| Frustums | Plane/count/component properties and direct six-plane creation | Already optimal direct fields/constants and construction. |
| Frustums | Throwing `CreateFromPlanes` vs `TryCreateFromPlanes` success/failure | Retained one-pass span validation/copy; throwing wrapper delegates Try and preserves required failure contracts. |
| Frustums | Throwing clip-transform creation vs `TryCreateFromClipTransform` | Optimized by matrix inverse/plane composition with retained depth-range and singular failure branches. |
| Frustums | `CopyPlanesTo` vs `TryCopyPlanesTo` | Success Try category added; throwing and short-destination failure remain. |
| Frustums | `TryCopyNormalizedPlanesTo` success/failure | Success category added; zero-normal failure remains. |
| Frustums | `CopyCornersTo` vs `TryCopyCornersTo` and `HasFiniteCorners` | Success/finite categories added; nonfinite and short destination remain. |
| Frustums | `TryCreateBoundingBox` success/failure | Success category added; nonfinite failure remains. |
| Frustums | Point containment | Exact inside/outside categories added. |
| Frustums | Conservative box Contains/Intersects/Classify | Contains/intersects/disjoint categories added. |
| Frustums | Precise box Intersects/Classify | Intersects/disjoint categories added; contains shortcut remains aggregated. |
| Frustums | Frustum Contains/Intersects/Classify/TryClassify | Contains/disjoint categories added; Try failure/nonfinite corners remain. |
| Frustums | Sphere, OBB, and capsule Contains/Intersects/Classify | Contains/intersects/disjoint category families added. |
| Frustums | Throwing `Transform` vs `TryTransform` success/failure | Success categories added; singular failure/throw remain. |
| Quads | Center | Permanent float/double category added. |
| Quads | Bounds | Permanent float/double category added. |
| Viewports | Bounds/depth/size/center properties and viewport-vector conversion | Size/center/vector categories added; Bounds/depth direct access remains infrastructure-only. |
| Viewports | Default vs explicit-depth `CreateTransform` | Both explicit depth-range categories added; default wrapper remains. |
| Viewports | Default vs explicit-depth `Project` | Both explicit depth-range categories added; default wrapper remains. |
| Viewports | Default vs explicit-depth `UnProject` | Both explicit depth-range categories added; default wrapper remains. |
| Viewports | Default vs explicit-depth `CreatePickRay` | Explicit caller category added; default wrapper remains. |
| Viewports | `CreatePickMatrix` | Permanent float/double category added. |
| Viewports | Float/double and tuple conversions | Tuple paths are already optimal direct field construction/extraction; float/double paths inherit the pending compound conversion candidate. |

## `ScalarMath` Touch Points

All single-scalar APIs are SIMD-ineligible by shape. The audit still requires
concrete generic-width disassembly/throughput evidence before marking the whole
class final.

| Touch point | Status | Evidence or reason |
| --- | --- | --- |
| `Min` | System semantics exact-tested | Generic `T.Min`; floating NaN/tie behavior matches regular System vectors. |
| `Max` | System semantics exact-tested | Generic `T.Max`; floating NaN/tie behavior matches regular System vectors. |
| `Clamp` | System semantics exact-tested | Regular `T.Min(T.Max())` composition matches System vector invalid-bound and exceptional-value policy. |
| `Abs` | System semantics exact-tested | Floating values use sign-clearing `T.Abs`; integer MinValue behavior remains unchanged. |
| `Lerp` | Permanent scalar suite added; execution pending | Exact dependency order. |
| `Barycentric` | Permanent scalar suite added; execution pending | Three inputs and two weights. |
| `Saturate` | System semantics exact-tested | Zero/one regular System clamp with exact NaN and signed-zero policy. |
| `Step` | Permanent scalar suite added; execution pending | Ordered comparison/materialization. |
| `SmoothStep` | Permanent scalar suite added; execution pending | Clamp plus polynomial dependency. |
| `Select` | Permanent scalar suite added; execution pending | Branch/conditional-move lowering depends on payload type. |
| `Floor` | Permanent scalar suite added; execution pending | Delegates generic floating math. |
| `Ceiling` | Permanent scalar suite added; execution pending | Separate rounding instruction/runtime call. |
| Default `Round` | Permanent scalar suite added; execution pending | Default midpoint semantics. |
| `Round(mode)` | Permanent scalar suite added; execution pending | Runtime midpoint mode is distinct. |
| `Truncate` | Permanent scalar suite added; execution pending | Toward-zero rounding. |
| `FractionalPart` | Permanent scalar suite added; execution pending | Floor then subtract. |
| `Modulo` | Permanent scalar suite added; execution pending | Floor-based modulo formula. |
| `Mod` | Permanent scalar suite added; execution pending | Public wrapper over Modulo. |
| `Sin` | Permanent scalar suite added; execution pending | Distinct runtime transcendental entry point. |
| `Cos` | Permanent scalar suite added; execution pending | Distinct runtime transcendental entry point. |
| `Tan` | Permanent scalar suite added; execution pending | Pole behavior and distinct entry point. |
| `Asin` | Permanent scalar suite added; execution pending | Domain behavior and distinct entry point. |
| `Acos` | Permanent scalar suite added; execution pending | Domain behavior and distinct entry point. |
| `Atan` | Permanent scalar suite added; execution pending | One-input arctangent. |
| `Atan2` | Permanent scalar suite added; execution pending | Operand order and two-input exceptional behavior. |
| `Exp` | Permanent scalar suite added; execution pending | Distinct runtime entry point. |
| `Log` | Permanent scalar suite added; execution pending | Domain behavior and distinct entry point. |
| `Log2` | Permanent scalar suite added; execution pending | Separate base-two lowering. |
| `Pow` | Permanent scalar suite added; execution pending | Two-input libm behavior. |
| `Sqrt` | Permanent scalar suite added; execution pending | Hardware-lowerable generic math. |
| `InverseSqrt` | Permanent scalar suite added; execution pending | Reciprocal-after-root composition. |
| `FusedMultiplyAdd` | Permanent scalar suite added; execution pending | Explicit fused semantics. |
| `IsNaN` | Permanent scalar suite added; execution pending | NaN classification. |
| `IsInfinity` | Permanent scalar suite added; execution pending | Infinity classification. |
| `IsFinite` | Permanent scalar suite added; execution pending | Combined finite classification. |
| `BitCount` | Permanent scalar suite added; execution pending | Delegates width-specific integer primitive. |
| `LeadingZeroCount` | Permanent scalar suite added; execution pending | Width-specific leading count. |
| `TrailingZeroCount` | Permanent scalar suite added; execution pending | Width-specific trailing count. |
| `FindLeastSignificantBit` | Permanent scalar suite added; execution pending | Zero branch plus trailing count. |
| `FindMostSignificantBit` | Permanent scalar suite added; execution pending | Zero branch plus width-minus-leading formula. |
| `IsPowerOfTwo` | Permanent scalar suite added; execution pending | Signed-positive rule plus primitive check. |

Public control enums (`ContainmentKind`, `PlaneIntersectionKind`,
`ProjectionDepthRange`, and `ProjectionHandedness`) contain no executable
surface and are `Ineligible` for performance optimization.

## Evidence Log

| Run | Scope | Decision |
| --- | --- | --- |
| `msp-int32-partial-screen-screen-representative` | Vec2/3 signed and unsigned Int32 add, multiply, signed negate, shift-left, and clamp; 1,024-item allocation-free batches | Reject equal-size `Vector64` and padded `Vector128` production paths. `Vector64` was 3.9x to 7.5x slower. Best padded `Vector128` candidates were 30% to 113% slower than current code except Vec3i negate, which was still 13% slower. Current and candidates allocated 0 B. |
| `20260712-205548164-p46568-screen-boolean-mask-full-screen` | 36 Boolean categories and six signed/unsigned Int32 relational-mask categories, 126 cases | Reject native packing for every reduction, relation, Boolean logical operation, and Vec2/3 selection. Only Vec4 float and Int32 Select advanced; all cases allocated 0 B. |
| `20260712-210528383-p55184-quick-boolean-vec4-select-quick` | Vec4 Boolean Select for float and Int32 payloads | Retain native conditional selection: 22.1% faster for float and 14.5% for Int32 versus current, 0 B. |
| `20260712-211045493-p44352-screen-int32-vec2-vec3-full-screen` | All 54 Vec2/3 Int32/UInt32 arithmetic, bitwise, shift, and bound categories; 200 cases | Reject padded SIMD broadly. Advance equal-size Vec2 bitwise and selected exact scalar-helper candidates; 0 B throughout. |
| `20260712-212805614-p53636-quick-vec2-int32-bitwise-quick` | Vec2i/Vec2u AND, OR, XOR, and complement | Retain equal-size `Vector64`: 31.8%–62.6% faster than current across all eight operations, 0 B. |
| `20260712-213434357-p45304-quick-int32-scalar-helper-quick` | Selected Vec2/3 Int32/UInt32 Min, Max, Clamp, and Abs direct expressions | Advance Vec3i Min/Clamp and Vec3u Max to Full; reject the other direct candidates or retain current pending Full. |
| `20260712-214042036-p26140-full-int32-scalar-helper-full` | Vec2i Max, Vec3i Min/Clamp, Vec3u Max | Retain direct Vec3i Min (43.0%) and Vec3u Max (46.0%). Keep current Vec2i Max; reject Vec3i Clamp at 2.9%. All cases 0 B. |
| `20260712-215724047-p53340-screen-scalar-math-full-screen` | All 41 public `ScalarMath` methods/overloads, 82 cases | Current won or matched 32 direct baselines; nine advanced to Quick. All cases 0 B. |
| `20260712-220628675-p25072-quick-scalar-math-candidates-quick-retry` | Nine ScalarMath Screen leads | Seven reverted to neutral; only Select and FMA direct float baselines remained faster. |
| `20260712-221009638-p28284-quick-scalar-math-specialization-quick` | Generic-compatible Select and FMA specialization candidates | Reject both: specialized candidates matched current generic performance, proving the direct float lead cannot be retained without changing the generic public contract. |
| `20260712-221241514-p57056-screen-narrow-vector-intrinsics-screen` | Vec4 sbyte/byte/short/ushort Min, Max, Clamp | Reject every safe-layout `Vector64` candidate: 2.9x–7.8x slower than current, 0 B. |
| `20260712-221600910-p38800-screen-boolean-mask-select-closure-screen` | All remaining Boolean Select payloads and explicit truth operators, 108 cases | Reject every additional supported native Select candidate at 3.7x–9.9x slower. Advance truth-operator scalar lowering. |
| `20260712-222438929-p55596-quick-boolean-truth-operators-quick` | Vec2b/Vec3b/Vec4b true and false operators | Direct Vec3b false and all true callers appeared faster; experiment with direct/inlined bodies. |
| `20260712-222956249-p31736-quick-boolean-truth-operators-optimized-quick` | Truth operators after direct/inlining experiment | Retain only Vec3b false: 1.2087 ns to 0.8752 ns (~27.6%), matching its direct ceiling. Revert unproven true and Vec2/4 false changes. |
| `20260712-223518976-p40852-screen-mat4-core-valid-screen` | Core Mat4/Mat4d lane and algebra suite, 72 cases | Valid replacement for the old failed Mat4 attempt. Advance float transpose and broad Mat4d direct-column/row candidates; all cases 0 B. |
| `20260712-224120779-p44108-quick-mat4-core-candidates-quick` | Thirteen Mat4/Mat4d Screen leads | Confirm Mat4 transpose and nine Mat4d paths at 38%–87%; float scale/vector-matrix remain unretained pending stronger evidence. |
| `20260712-225057096-p13940-quick-mat4-core-optimized-quick` | Production direct-column/row Mat4 and Mat4d paths | Production captures 35%–89% for transpose, add/subtract/negate/scale/component multiply/lerp/matrix-vector. Initial whole-vector Mat4d matrix product regressed and was replaced. |
| `20260712-225713021-p28864-quick-mat4d-matrix-matrix-direct-quick` | Direct ordered field-based Mat4d matrix product | Improves original production from 44.18 ns to 15.59 ns (~64.7%) but retains an inlining gap. |
| `20260712-225933368-p44576-quick-mat4d-matrix-matrix-inline-quick` | Final direct/inlined Mat4d matrix product | Retain at 11.00 ns versus 44.18 ns original (~75.1%), matching the 11.12 ns scalar ceiling, 0 B. |
| `20260712-230158376-p11712-quick-package-sweep-after-exhaustive-wave` | Twelve-case cross-package regression sweep | Versus the prior authoritative retained sweep, Mat4d algebra improves 83.2% and Mat4d lanes 29.8%; the other ten vector/quaternion/float-matrix signals remain within -4.0% to +2.7%. Every case allocates 0 B. |
| `20260712-230740441-p47844-screen-half-vectors-full-screen-rerun` | Broad Vec2h/Vec3h/Vec4h Screen, 144 methods / 72 current-versus-generic-loop pairs | 0 B throughout; current was materially faster in 65 pairs, within ±5% in four, and slower by more than 5% in three. Treat the generic scalar loop as characterization only, not as an optimization baseline or evidence of 65 optimized members. |
| `20260712-232349309-p27168-screen-half-concrete-candidates-screen` | Concrete Vec2h/Vec3h/Vec4h division, remainder, and scalar-amount Lerp candidates | Division and remainder produced no direct wins. Vec3h Lerp led provisionally by 9% and Vec4h by 26%; only the Lerp candidates advanced. |
| `20260712-232559349-p2772-quick-half-lerp-concrete-quick` | Concrete Vec3h and Vec4h scalar-amount Lerp | Reject Vec4h: direct was 71.90 ns versus 67.29 ns current (~7% slower). Keep Vec3h for Full: 41.50 ns versus 44.17 ns current (~6% faster). All methods allocated 0 B. |
| `20260712-232747360-p47740-full-half-lerp3-concrete-full` | Concrete Vec3h scalar-amount Lerp direct-body Full | Direct improved to 41.64 ns versus 44.36 ns current (6.1%), 0 B, but was superseded by the faster operator-composed formula. Intervening body-only and broad-inlining shapes were rejected because they did not reproduce the composed ceiling. |
| `20260712-233828554-p36768-full-half-lerp3-composed-full` | Vec3h operator-composed scalar-amount Lerp candidate Full | Composed improved to 31.92 ns versus 43.84 ns current (27.2%), while direct was 41.67 ns; all methods allocated 0 B. Promote operator composition with selective inlining. |
| `20260712-234038745-p15620-full-half-lerp3-composed-production-full` | Final Vec3h composed production Full | Retain production at 33.42 ns; composed ceiling was 31.89 ns and direct was 41.61 ns, all 0 B. This is 23.77% faster than immediately prior current (43.84 ns) and 24.66% faster than the original Full current (44.36 ns); exact bit test passed. |
| `20260712-234416110-p18048-screen-vector-alternative-closure-screen` | Vector alternative closure Screen: 172 methods / 86 current-direct pairs in 11m32s | All methods allocated 0 B. Sixty-four pairs were within ±5%, six had current faster by more than 5%, and sixteen had current slower by more than 5%. Thirty-five transcendental pairs closed immediately as retained scalar/already optimal; the provisional 8% Float vector-exponent Pow lead advanced to focused Quick and was later closed as neutral. Close 21/22 isolated conversion pairs within ±5%; Vec3i64-to-Vec3i128, 6% slower, needs a focused rerun. Aggregate leads remain candidate/inlining backlog, not retained gains: vector-count shifts 4.62x/5.79x, narrow promoted pair 4.25x, unsigned/signed directions 3.09x, float fraction/modulo/smooth 2.51x, distance 1.48x, classification 13%–18%, Half geometry 14%, cross-vector promoted comparisons 13%, floating promotions 12%, and composition/conversion 6%–9%. Aggregate callers combine multiple public leaves and do not identify the cause of a gap. |
| `20260712-235843285-p42992-screen-vector-normalized-cached-screen` | Cached normalization candidates, 72 methods across Vec2/3/4 float, double, and Half | Float and double candidates were neutral/slower and are retained as already optimal. Only Vec2h/3h/4h cached-length candidates advanced. |
| `20260713-000514112-p41020-quick-half-normalized-cached-quick` | Pre-production cached Half normalization Quick | Confirm cached `LengthSquared` plus selective `AggressiveInlining` for all Half dimensions. Exact generator and two runtime bit/fallback tests passed. |
| `20260713-001051752-p25840-quick-half-normalized-production-quick` | Final cached Half normalization production Quick | All methods allocated 0 B. Vec2h fallback improved 55.455→36.075 ns (35.0%) and zero 56.117→35.103 ns (37.4%); Vec3h fallback 78.91→57.79 ns (26.8%) and zero 84.52→56.83 ns (32.8%); Vec4h fallback 109.88→67.04 ns (39.0%) and zero 103.86→65.41 ns (37.0%). Vec4h failure paths also improved about 39%; other failure paths were neutral/small. Retain production for Vec2h/3h/4h. |
| `20260713-001645610-p46988-screen-vector-count-shifts-inline-screen` | Broad per-lane shift `AggressiveInlining` experiment | Reject and revert: aggregate improvements were only 0.6% and 1.0%, far below the retention threshold. |
| `20260713-002448904-p23496-screen-vec4-variable-shifts-avx2-screen` | Exact Vec4i/Vec4u AVX2 variable-count shift candidates | All six `<<`, `>>`, and `>>>` candidates advanced at 31% to 40% faster with 0 B. Negative and large counts were exact-validated with explicit `& 31` masking. |
| `20260713-002607390-p49660-quick-vec4-variable-shifts-avx2-quick` | Focused Vec4i/Vec4u AVX2 variable-count shift Quick | Confirm all six candidates and retain portable AVX2 with component fallback. Two generator tests and two runtime exact tests cover the emitted paths and count semantics. |
| `20260713-003027628-p51100-quick-vec4-variable-shifts-production-quick` | Final Vec4i/Vec4u variable-count shift production Quick | All methods allocated 0 B. Vec4i left improved 1.2156→0.7794 ns (35.9%), right 1.2232→0.7673 ns (37.3%), and logical right 1.2275→0.7594 ns (38.1%); Vec4u left improved 1.2251→0.7213 ns (41.1%), right 1.2669→0.7363 ns (41.9%), and logical right 1.1673→0.7331 ns (37.2%). Retain all six production paths; Vec2/3 Int32 and all Int64 per-lane shifts remain pending focused intrinsic candidates. |
| `20260713-003611392-p11212-screen-partial-int32-variable-shifts-avx2-screen` | Padded Vec2i/u and Vec3i/u AVX2 variable-count shifts: 24 methods / 12 current-candidate pairs | All methods allocated 0 B and setup exact-validated negative and large counts. Reject every `<<`, `>>`, and `>>>` candidate: Vec2 candidates were 84% to 112% slower and Vec3 candidates 57% to 75% slower. Pack, pad, and extract overhead dominates two or three shifted lanes, so all partial Int32 shapes are retained scalar/already optimal; Vec4 remains optimized and Int64 variable-count shifts remain pending. |
| `20260713-004113554-p44860-quick-vec4-float-pow-vector-quick` | Focused Vec4 Float vector-exponent Pow Quick | Current was 16.63 ns versus 16.67 ns for the direct scalar candidate (ratio 1.00), 0 B. The provisional 8% Screen lead did not reproduce; retain the exact scalar runtime implementation as already optimal with no pending candidate. |
| `20260713-004308146-p54504-screen-core-float-double-leaf-screen` | Focused float/double fractional, modulo, smooth-step, and classification leaves: 18 methods / 9 current-direct pairs | All methods allocated 0 B and all pairs were neutral at about 1%. Vec4 float current/direct: FractionalPart 1.931/1.940 ns, Modulo 2.011/2.017 ns, SmoothStep 2.486/2.475 ns, IsNaN 1.192/1.188 ns, IsInfinity 1.464/1.474 ns, IsFinite 1.469/1.470 ns. Vec4d classification current/direct: IsNaN 1.206/1.190 ns, IsInfinity 1.534/1.529 ns, IsFinite 1.496/1.477 ns. Retain every current leaf as already optimal; the earlier 2.51x fractional/modulo/smooth and 13%–18% classification aggregate gaps were caller-composition artifacts. Half exact-rounding and raw-bit candidates remain separate. |
| `20260713-004603877-p50308-screen-double-vector-interpolation-step-screen` | Focused double vector-amount Lerp, Barycentric, and vector/scalar-edge Step candidates | Reject vector Lerp composition: Vec2d was 4.82x slower, Vec3d neutral at ratio 0.98, and Vec4d neutral at 0.97. Reject Barycentric composition: Vec2d was 8.75x slower, Vec3d 15% slower, and Vec4d 9% slower. Reject packed scalar-edge Step: Vec2d was 5.78x slower and Vec4d 2.91x slower. Advance only packed Vec2d/Vec4d vector-edge Step. |
| `20260713-004805448-p44888-quick-double-vector-step-packed-quick` | Prototype packed Vec2d/Vec4d vector-edge Step Quick | Confirm the packed vector-edge candidates and reject the scalar-edge shape; only vector-edge Step advances to production. |
| `20260713-005138348-p26660-quick-double-vector-step-production-quick` | Final packed Vec2d/Vec4d vector-edge Step production Quick | All methods allocated 0 B. Old production baselines 0.8744/1.6543 ns improve to final current 0.5477/0.7458 ns, about 37.4%/54.9% faster, matching candidate parity at 0.5452/0.7436 ns. Retain packed vector-edge Step for Vec2d and Vec4d. |
| `20260713-005340240-p47552-screen-double-scalar-lerp-core-screen` | Focused Vec2d/Vec3d/Vec4d scalar-amount Lerp composition Screen | All methods allocated 0 B. Vec2d current was 0.7972 ns versus 3.8658 ns composed (4.85x slower); Vec3d 1.0822/1.0767 ns (ratio 0.99); Vec4d 1.4384/1.3855 ns (ratio 0.96, below the 5% retention threshold). Reject composition and retain current as already optimal for every double dimension. |
| `20260713-005729039-p53796` | Initial Native Float-to-Int32 experiment | Invalid and non-evidence: the Native conversion used different exceptional behavior from the public scalar cast contract. No performance result from this run supports an optimization decision. |
| `20260713-005859582-p35120-screen-float-to-int32-validation-probe` | Defined `ConvertToInt32` exceptional-semantics validation probe | Establish the exact conversion behavior required for NaN, infinity, overflow, negative, and large inputs before performance screening. Only candidates matching this probe advance. |
| `20260713-005931733-p18152-screen-core-float-to-int32-intrinsic-valid-screen` | Valid exact Float-to-Int32 intrinsic Screen | Retain Vec3/Vec4 defined `Vector128` candidates for truncate, floor, ceiling, and round. Reject Vec2: defined `Vector64` truncate was 3.98x slower and rounded conversions 10.81x to 10.86x slower. All retained paths preserve exceptional semantics and allocate 0 B. |
| `20260713-010158251-p7576-quick-core-float-to-int32-vec3-vec4-quick` | Focused Vec3/Vec4 defined Float-to-Int32 candidate Quick | Confirm all eight Vec3/Vec4 conversion candidates and retain defined `Vector128` lowering; Vec2 remains scalar/already optimal. |
| `20260713-010626361-p34556-quick-core-float-to-int32-production-quick` | Final Vec3/Vec4 defined Float-to-Int32 production Quick | All methods allocated 0 B. Vec3 ceiling improved 1.1812→0.8072 ns (31.7%), floor 1.1697→0.8011 ns (31.5%), round 1.1770→0.8015 ns (31.9%), truncate 1.0948→0.7735 ns (29.3%). Vec4 ceiling improved 1.5284→0.3847 ns (74.8%), floor 1.5386→0.3717 ns (75.8%), round 1.5368→0.3720 ns (75.8%), truncate 1.3835→0.3788 ns (72.6%). Exact exceptional validation passed; retain production. |
| `20260713-010911247-p36020-screen-core-vector-distance-direct-screen` | Focused float, double, and Int32 Distance/DistanceSquared Screen: 36 methods | All methods allocated 0 B. Float direct leaves advanced. Double pairs were within ±5% or current faster. Int32 Vec2/3 were neutral; Vec4 current won, with direct Distance 22% and DistanceSquared 3% slower. The earlier 1.48x mixed aggregate gap was caller composition, not a universal distance-leaf gain. |
| `20260713-011224137-p49936-quick-core-float-distance-direct-quick` | Focused Vec2/3/4 float distance candidate Quick | Confirm direct float Distance and DistanceSquared leaves across all three dimensions and advance them to production; double and Int32 remain current/already optimal. |
| `20260713-011622350-p36796-quick-core-float-distance-production-quick` | Final Vec2/3/4 float distance production Quick | All methods allocated 0 B and production matches the direct ceiling. Distance improves Vec2 0.9143→0.8684 ns (5.0%), Vec3 1.1621→0.9304 ns (19.9%), Vec4 1.2228→1.0897 ns (10.9%). DistanceSquared improves Vec2 0.7364→0.6964 ns (5.4%), Vec3 1.1619→0.8487 ns (27.0%), Vec4 1.2266→1.0845 ns (11.6%). Retain all six float production paths. |
| `20260713-011831144-p18280-screen-vec4-int32-bit-functions-screen` | Initial Vec4i/Vec4u packed bit-function Screen | Advance packed BitCount, leading/trailing-zero count, find-LSB/find-MSB, and power-of-two candidates to focused Quick under hardware gates with exact component fallbacks. |
| `20260713-012103033-p3820-quick-vec4-int32-bit-functions-quick` | Focused Vec4i/Vec4u bit-function candidate Quick | Confirm the retained packed candidates. Reject unsigned Vec4 IsPowerOfTwo at only 3.7%, below the retention threshold; keep that leaf scalar. |
| `20260713-012917985-p41868-quick-vec4-int32-bit-functions-production-quick` | First Vec4i/Vec4u bit-function production Quick | Validate the initial production lowering and preserve hardware-gated packed paths plus exact scalar fallbacks for subsequent selective-inlining review. |
| `20260713-013513999-p37860-screen-vec4-int32-bit-functions-inline-screen` | Selective Vec4 Int32 bit-function inlining Screen | Characterize leaf-specific inlining after the first production Quick; the later current-only production run is the authoritative retained-performance result. |
| `20260713-013818172-p35256-screen-vec2-vec3-int32-bit-functions-screen` | Padded Vec2i/u and Vec3i/u bit-function Screen | Reject all partial-register candidates as slower or neutral: Vec2 was 8% to 144% slower and Vec3 11% to 69% slower, except FindMSB only 3% to 4% faster and therefore below threshold. Retain all six Vec2/3 surfaces scalar/already optimal. |
| `20260713-014233856-p47332-quick-vec4-int32-bit-functions-final-production-quick` | Final current-only Vec4i/Vec4u bit-function production Quick | All methods allocated 0 B. Signed gains versus original candidate-Quick current: BitCount 15.1%, FindLSB 42.8%, FindMSB 62.4%, IsPowerOfTwo 17.2%, leading-zero count 44.4%, trailing-zero count 17.0%. Unsigned gains: BitCount 16.8%, FindLSB 41.5%, FindMSB 62.4%, leading-zero count 43.7%, trailing-zero count 18.9%; unsigned IsPowerOfTwo remains scalar. Retain hardware gates/fallbacks; exact tests pass. |
| `20260713-015223902-p26240-screen-double-vector-saturate-screen` | Vec2d/Vec3d/Vec4d packed Saturate candidate Screen | Advance exact packed candidates for all three double dimensions after validating NaN payload, signed zero, and infinity behavior. All methods allocated 0 B. |
| `20260713-015325989-p45168-quick-double-vector-saturate-quick` | Focused all-shape double Saturate candidate Quick | Confirm Vec2d/Vec3d/Vec4d packed candidates and advance all three exact paths to production. Runtime exact validation passed. |
| `20260713-015642359-p48224-quick-double-vector-saturate-production-quick` | Final all-shape double Saturate production Quick | All methods allocated 0 B and production current matches or beats the packed helper. Versus pre-change Quick current, Vec2d improves 1.3061→0.5113 ns (60.9%), Vec3d 1.9108→1.0486 ns (45.1%), and Vec4d 2.4728→0.5637 ns (77.2%). Retain all three production paths with exact NaN payload, signed-zero, and infinity semantics. |
| `20260713-015830255-p6296` | Raw x86 Double-to-Int32 conversion experiment | Invalid and non-evidence: raw x86 conversion did not match the public exceptional/saturating conversion semantics. No timing from this run supports a production decision. |
| `20260713-020041497-p44684` | Saturating Double-to-Int32 validation probe | Passed the authoritative exceptional-input probe and established the semantics required for every subsequent candidate. |
| `20260713-020126525-p55480` | Valid Vec2d/Vec3d/Vec4d defined Double-to-Int32 Screen | Advance Vec2d and Vec4d ceiling, floor, round, truncate, and cast paths under the saturating contract; reject Vec3d and retain its current scalar implementation. All retained paths allocate 0 B. |
| `20260713-020353012-p16324` | Focused Vec2d/Vec4d defined Double-to-Int32 candidate Quick | Confirm the exact Vec2d/Vec4d candidates before production. Vec3d remains rejected/current scalar. |
| `20260713-021027775-p27052` | Final Vec2d/Vec4d defined Double-to-Int32 production Quick | All methods allocated 0 B and final current beats the standalone helper. Vec2d ceiling/floor/round/truncate improve from 1.2221/1.2199/1.2309/1.1772 ns to 0.8499/0.8589/0.8331/0.9193 ns. Vec4d improves from 2.0760/2.0896/2.0530/1.9331 ns to 0.7466/0.7056/0.6869/0.6788 ns. Retain all defined/saturating production paths. |
| `20260713-exact-register-storage-overlay-screen` | Private generic-intrinsic storage for Vec2d/4d, Vec2i/u, Vec4i/u, Vec2/4i64/u64 | Reject every generic `Vector64/128/256<T>` field overlay and retain equal-size `Unsafe.BitCast` lowering. All methods allocated 0 B, but Int2 bitwise compound regressed 0.5466→0.8273 ns (51.3%), Int4 add 0.6065→0.7260 ns (19.7%), Int64 compounds 8.5%–9.9%, and Int32 bounds 8.3%–12.2%; double compounds/divides were neutral or slightly slower. A hybrid packed-read/bit-cast-output variant also lost. Generic intrinsic fields do not receive the special JIT representation treatment of `System.Numerics.Vector2/3/4`. |
| `20260713-quaternion-system-storage-overlay-screen` | Private `System.Numerics.Quaternion` storage for `Quat` | Retain. Arithmetic `(left + right) * 0.5f` improved 0.9216→0.6942 ns (24.7%) and normalize compound improved 2.3114→2.0025 ns (13.4%), all 0 B. The final System references were 0.8066 ns and 1.7143 ns respectively. Exact field bits, layout offsets, normal hardware-intrinsic execution, and the disabled-intrinsic fallback pass. System conjugation changed a NaN sign bit, so conjugate deliberately retains the exact sign-bit-XOR implementation. |
| `20260713-float-value-semantics-split-short` | Float Vec2/3/4 typed `Equals` and `GetHashCode` measured separately against System.Numerics | All 12 methods allocated 0 B. Equals: Alvor/System was 1.1369/0.8932 ns for Vec2, 1.2657/0.9179 ns for Vec3, and 0.9870/0.6792 ns for Vec4, identifying equality as the consistent combined-workload gap. Hash: 1.8624/2.1243 ns, 3.4595/2.9058 ns, and 3.5916/3.3898 ns respectively—mixed rather than universally slower. Vec4 disassembly confirms scalar branched `float.Equals` versus System packed `Vector128.Equals`; both hashes use `HashCode.Combine`, with only wrapper/codegen differences. |
| `20260713-float-packed-equality-production-short` | Final Vec2/3/4 private-System-view typed equality | Retain all three, 0 B. Alvor improved 1.1369→0.6576 ns (42.2%), 1.2657→0.8744 ns (30.9%), and 0.9870→0.6149 ns (37.7%). Current System references are 0.6602/0.8359/0.6148 ns, putting Vec2 and Vec4 at exact parity and Vec3 within 4.6%. Unchanged hash controls moved 2%–26% between launches, so within-run parity is the authoritative signal and raw before/after gains include environmental drift. Exact generator/runtime tests pass with hardware intrinsics enabled and disabled. |

Permanent suites now exist for the original 66 box/plane/ray branch categories,
42 Boolean/mask categories, broad float/double matrix and quaternion surfaces,
ScalarMath, value infrastructure, intervals, spheres, segments, capsules,
triangles, OBBs, frustums, quads, and viewports. The complete authored vector
closure is now 2,180 closed cases; the non-vector audit-gap wave adds 108
categories; all 90 public value types have 20 cold-text categories (1,800
cases); and all 28 compound layouts have 16 infrastructure categories (448
cases). Authored-but-unexecuted suites are inventory evidence only: they do not
change an optimization status until measured and, where required, promoted
through Screen/Quick/Full.

## Completion Rule

No `In progress`, `Unmeasured`, `Missing`, or undocumented public member family
may remain when the goal is marked complete. The final audit must compare this
manifest against the generated catalog, generated public source, `ScalarMath`,
and the published generic interfaces. A surface may remain scalar, but only with
a concrete semantic, layout, throughput, latency, code-size, allocation, or
cold-path reason.
