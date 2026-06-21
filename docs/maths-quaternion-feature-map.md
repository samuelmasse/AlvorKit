# Maths Quaternion Feature Map

## Reference Surfaces

This file maps the source libraries reviewed for quaternion work to the proposed AlvorKit API. It is a planning reference, not a
requirement to copy any library verbatim.

- GLM local source:
  - `C:/Users/Samuel/Documents/Repos/glm/glm/detail/type_quat.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/gtc/quaternion.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/gtx/quaternion.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/ext/quaternion_common.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/ext/quaternion_geometric.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/ext/quaternion_trigonometric.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/ext/quaternion_transform.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/ext/quaternion_exponential.hpp`
  - `C:/Users/Samuel/Documents/Repos/glm/glm/ext/quaternion_relational.hpp`
- OpenTK docs:
  - `https://opentk.net/api/OpenTK.Mathematics.Quaternion.html`
  - `https://opentk.net/api/OpenTK.Mathematics.Quaterniond.html`
- System docs:
  - `https://learn.microsoft.com/en-us/dotnet/api/system.numerics.quaternion?view=net-10.0`

## Surface Comparison

| Area | GLM | OpenTK | System.Numerics | AlvorKit plan |
| --- | --- | --- | --- | --- |
| Float quaternion | `quat` | `Quaternion` | `Quaternion` | `Quat` |
| Double quaternion | `dquat` | `Quaterniond` | None | `Quatd` |
| Storage naming | Usually `x, y, z, w` | `Xyz`, `X`, `Y`, `Z`, `W` | `X`, `Y`, `Z`, `W` | `X`, `Y`, `Z`, `W`, `Vector` |
| Identity | `quat_identity` | `Identity` | `Identity` | `Identity` |
| Formatting | C++ streams only | `IFormattable` | `ToString()` only | Vector/matrix-level span formatting and parsing |
| System interop | Not applicable | Explicit System conversion | Native type | Explicit `Quat` conversion |
| Matrix conversion | `mat3_cast`, `mat4_cast`, `quat_cast` | `FromMatrix` | `CreateFromRotationMatrix` | `ToMat3`, `ToMat4`, `CreateFromRotationMatrix` |
| Axis-angle | `angleAxis`, `angle`, `axis` | `FromAxisAngle`, `ToAxisAngle` | `CreateFromAxisAngle` | `CreateFromAxisAngle`, `ToAxisAngle`, `Angle`, `Axis` |
| Euler | `eulerAngles`, `pitch`, `yaw`, `roll` | `FromEulerAngles`, `ToEulerAngles` | `CreateFromYawPitchRoll` | Both Euler and yaw-pitch-roll factories |
| Look rotation | `quatLookAt`, `quatLookAtRH`, `quatLookAtLH` | Not prominent | None | `LookRotation` overloads with `ProjectionHandedness` |
| Rotation between vectors | `rotation` in GTX | Not prominent | None | `CreateRotationBetween` |
| Interpolation | `lerp`, `slerp`, `squad`, `shortMix`, `fastMix` | `Slerp` | `Lerp`, `Slerp` | `Lerp`, `Nlerp`, `Slerp`, `Squad` |
| Exponential helpers | `exp`, `log`, `pow`, `sqrt` | Not prominent | None | `Exp`, `Log`, `Pow`, `Sqrt` |
| Relational helpers | Component masks | Equality | Equality | Component mask helpers matching vectors/matrices |

## Name Mapping

| Source name | AlvorKit name |
| --- | --- |
| `glm::angleAxis(angle, axis)` | `Quat.CreateFromAxisAngle(axis, radians)` |
| `glm::angle(q)` | `q.Angle` |
| `glm::axis(q)` | `q.Axis` |
| `glm::eulerAngles(q)` | `q.EulerAngles` |
| `glm::pitch(q)` | `q.Pitch` |
| `glm::yaw(q)` | `q.Yaw` |
| `glm::roll(q)` | `q.Roll` |
| `glm::mat3_cast(q)` | `q.ToMat3()` |
| `glm::mat4_cast(q)` | `q.ToMat4()` |
| `glm::quat_cast(m)` | `Quat.CreateFromRotationMatrix(m)` |
| `glm::quatLookAtRH(direction, up)` | `Quat.LookRotation(direction, up, ProjectionHandedness.Right)` |
| `glm::quatLookAtLH(direction, up)` | `Quat.LookRotation(direction, up, ProjectionHandedness.Left)` |
| `glm::rotation(from, to)` | `Quat.CreateRotationBetween(fromUnitDirection, toUnitDirection)` |
| `glm::lerp(x, y, a)` | `Quat.Lerp(from, to, amount)` |
| `glm::slerp(x, y, a)` | `Quat.Slerp(from, to, amount)` |
| `glm::fastMix(x, y, a)` | `Quat.Nlerp(from, to, amount)` |
| `glm::squad(q1, q2, s1, s2, h)` | `Quat.Squad(q1, q2, s1, s2, amount)` |
| `glm::intermediate(prev, curr, next)` | `Quat.CreateSquadControlPoint(previous, current, next)` |
| `glm::exp(q)` | `Quat.Exp(value)` |
| `glm::log(q)` | `Quat.Log(value)` |
| `glm::pow(q, y)` | `Quat.Pow(value, exponent)` |
| `glm::sqrt(q)` | `Quat.Sqrt(value)` |
| `OpenTK.Quaternion.FromEulerAngles` | `Quat.CreateFromEulerAngles` |
| `OpenTK.Quaternion.ToEulerAngles` | `q.EulerAngles` or `q.ToEulerAngles()` if a method reads better in review |
| `System.Numerics.Quaternion.CreateFromYawPitchRoll` | `Quat.CreateFromYawPitchRoll` |
| `System.Numerics.Quaternion.CreateFromRotationMatrix` | `Quat.CreateFromRotationMatrix` |
| `System.Numerics.Quaternion.Concatenate` | Do not add initially. Use `left * right` with documented order. |

## Behavioral Choices

### Default Handedness

Default look rotation should match the matrix projection default: right-handed, OpenGL-style. Explicit overloads should accept
`ProjectionHandedness` for symmetry with matrix APIs.

### Axis Normalization

`CreateFromAxisAngle` should expect a unit axis, matching System and GLM. We can add `CreateFromNormalizedAxisAngle` only if review shows
the name improves clarity. Do not silently normalize in the factory unless the rest of the math API changes direction on caller-provided
values.

### Degenerate Normalization

Vectors currently expose `Normalized`, `NormalizedOrZero`, `NormalizedOr`, and `TryNormalize`. Quaternions should mirror that pattern:

- `Normalized` divides by length.
- `NormalizedOrIdentity` gives a rotation-safe fallback.
- `NormalizedOr(fallback)` lets the caller choose.
- `TryNormalize(out result)` reports failure for zero length.

### Multiplication Order

Use Hamilton product compatible with System, GLM, and OpenTK. Tests must demonstrate transform order with a known pair of rotations, not
just component formulas.

### Euler Naming

Expose both C#-friendly and System-compatible names:

- `CreateFromEulerAngles(pitch, yaw, roll)`
- `CreateFromEulerAngles(Vec3 radians)`
- `CreateFromYawPitchRoll(yaw, pitch, roll)`

The first two read naturally with `Vec3.X/Y/Z`; the last one makes System migration obvious.

## Implementation Risks

- Quaternion multiplication order is easy to misread. Add behavioral tests using vector rotation.
- Matrix conversion formulas must match the column-major matrix convention already chosen.
- Euler extraction has singularities. Test normal cases and at least one near-singular case.
- Parse and format reuse can become tangled if the generator tries to over-share vector internals.
- Exponential helpers are advanced and easy to get subtly wrong. Keep formulas close to GLM and add focused tests.
- `Quatd` cannot rely on System interop as an oracle because System has only float quaternion support.

## First Pass Acceptance Criteria

- `Quat` and `Quatd` are generated into the primitives project.
- `Quat` and `Quatd` compile through `AlvorKit.Maths`.
- Matrix rotation overloads compile and are covered by tests.
- `Quat` round-trips through `System.Numerics.Quaternion`.
- `Quatd` mirrors the same API without System interop.
- Runtime tests cover axis-angle, matrix conversion, vector rotation, interpolation, formatting, parsing, and degenerate paths.
- The maths demo shows one practical quaternion rotation flow.
