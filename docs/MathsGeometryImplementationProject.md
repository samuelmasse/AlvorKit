# Maths Geometry Implementation Project

This document records the geometry implementation project for the AlvorKit maths
library after vectors, matrices, quaternions, boxes, planes, and frustums.

The goal is to build a small, coherent game geometry layer on top of the
existing algebra types. The new APIs should feel like natural AlvorKit C#, not a
bundle of copied names from GLM, OpenTK, MonoGame, DirectXCollision, Unity, or
Bevy.

## Current Foundation

The maths package already has these first-class building blocks:

- Vectors: `Vec2`, `Vec3`, `Vec4`, scalar-family variants, formatting,
  parsing, span APIs, and numeric-style operators.
- Matrices: square and non-square float/double matrices, projection helpers,
  depth-range and handedness options, formatting, parsing, span APIs, and
  `System.Numerics` interop where a matching system type exists.
- Quaternions: `Quat` and `Quatd`, rotation construction, interpolation,
  vector transforms, formatting, parsing, span APIs, and conversions.
- Boxes: `Box2`, `Box3`, integer variants, double variants, half-open and
  inclusive containment, union, intersection, closest point, inflation, and
  vector-shaped APIs.
- Planes: `Plane3` and `Plane3d`, construction, normalization, evaluation,
  projection, transformation, formatting, parsing, and scalar-family
  conversions.
- Frustums: `Frustum3` and `Frustum3d`, six named planes, clip-transform
  extraction, finite corner extraction, point and box tests, transformation,
  formatting, parsing, and scalar-family conversions.

This pass added the missing game-query shapes and their relationships:

- `Sphere3` / `Sphere3d`
- `Ray3` / `Ray3d`
- `Intervalf` / `Intervald` or an equivalent interval shape
- `PlaneIntersectionKind`
- `Obb3` / `Obb3d`
- `Segment3` / `Segment3d`
- `Capsule3` / `Capsule3d`
- `Triangle3` / `Triangle3d`
- Viewport project, unproject, and pick-ray helpers

## Implementation Status

Implemented in the active maths package:

- `Sphere3` / `Sphere3d`, including point-cloud bounding sphere helpers.
- `Intervalf` / `Intervald`.
- `Ray3` / `Ray3d`.
- `PlaneIntersectionKind`.
- Plane, sphere, box, ray, and frustum relationships.
- Frustum finite-corner helpers, bounding-box extraction, frustum/frustum
  classification, precise box checks, and OBB/capsule relationships.
- `Obb3` / `Obb3d`.
- `Segment3` / `Segment3d`.
- `Capsule3` / `Capsule3d`.
- `Triangle3` / `Triangle3d`.
- `Viewport` / `Viewportd` with project, unproject, pick-ray, pick-matrix,
  span, formatting, and float/double conversion support.

Closeout status:

- `MathsGen` source parity is complete for the generated geometry families
  listed above. Regeneration is the source of truth for those primitive shapes.
- `MathsGen` source files are grouped into shallow folders for catalogs, specs,
  emitters, infrastructure, and CLI entry points.
- Optional features listed in the Deferred Features section remain intentionally
  out of scope until a concrete caller appears.

## Reference Summary

The references point in two different directions.

GLM, OpenTK, and Silk.NET.Maths are algebra-first libraries. GLM has extensive
projection and frustum matrix builders. OpenTK exposes vectors, matrices,
quaternions, boxes, colors, and Bezier curves. Silk.NET.Maths is a generic
number-oriented algebra package. These references are useful for style and
baseline algebra coverage, but they do not define the full game collision layer.

MonoGame/XNA, DirectXCollision, SharpDX, Unity, Godot, and Bevy are stronger
references for game geometry. Their common pattern is a family of simple
bounding and query primitives:

- Sphere
- Axis-aligned box
- Oriented box
- Frustum
- Plane
- Ray
- Triangle

Unity and Godot also show that frustum plane arrays are a common interchange
shape. Unity documents that plane-array AABB tests are conservative and can
false-positive around large boxes near frustum edges. That matches how fast
frustum culling is usually used: reject obvious misses cheaply, then do more
precise work only when required.

Useful references:

- MonoGame `BoundingFrustum` documentation:
  https://docs.monogame.net/api/Microsoft.Xna.Framework.BoundingFrustum.html
- DirectXCollision `BoundingFrustum` documentation:
  https://learn.microsoft.com/en-us/windows/win32/api/directxcollision/ns-directxcollision-boundingfrustum
- Unity `GeometryUtility.CalculateFrustumPlanes`:
  https://docs.unity3d.com/ScriptReference/GeometryUtility.CalculateFrustumPlanes.html
- Unity `GeometryUtility.TestPlanesAABB`:
  https://docs.unity3d.com/6000.1/Documentation/ScriptReference/GeometryUtility.TestPlanesAABB.html
- Godot `Camera3D.get_frustum`:
  https://docs.godotengine.org/en/4.4/classes/class_camera3d.html
- Bevy `Frustum` docs:
  https://docs.rs/bevy/latest/bevy/camera/primitives/struct.Frustum.html
- LearnOpenGL frustum culling:
  https://learnopengl.com/Guest-Articles/2021/Scene/Frustum-Culling
- Bruno Opsenica frustum culling notes:
  https://bruop.github.io/frustum_culling/
- OpenTK math namespace:
  https://opentk.net/api/OpenTK.Mathematics.html
- Silk.NET.Maths NuGet package:
  https://www.nuget.org/packages/Silk.NET.Maths

## Design Principles

Treat every real geometry value as a first-class API shape.

- Accept `Vec3`, `Box3`, `Sphere3`, `Ray3`, `Obb3`, and similar types when that
  is what the caller is passing.
- Do not add scalar-coordinate overloads such as `Contains(float x, float y,
  float z)`.
- Do not encode real shapes as loose tuples or unrelated parameter groups when a
  named type would carry the concept better.
- Keep generated float and double variants for geometry that represents
  continuous space.
- Do not add integer variants for rays, planes, frustums, spheres, capsules,
  OBBs, or triangles unless a concrete use case appears.
- Keep storage layouts simple, sequential, immutable when practical, and
  compatible with span formatting and parsing patterns used by vectors and
  matrices.
- Avoid hidden allocations in runtime and query methods.
- Prefer `Try...` methods for operations that can fail because the shape is
  degenerate or infinite.
- Do not silently normalize, clamp, or fix caller-provided data in constructors
  or property setters.
- Add precise methods with explicit names instead of making fast methods slower
  or surprising.

All new public maths types should match the implementation standard of the
existing primitives:

- XML docs written for API consumers.
- Interface hierarchy in `AlvorKit.Maths.Core`.
- Generated `float` and `double` primitive implementations where appropriate.
- `IEquatable<T>`, `IComparable<T>` where meaningful, formatting, UTF-8
  formatting, parsing, UTF-8 parsing, and span copy APIs where the shape has a
  stable scalar component representation.
- Tuple conversions only when they are naturally readable and do not collide
  with another shape.
- Tests for construction, formatting, parsing, equality, edge cases, and all
  geometry relationships introduced by each task.

## Naming

Use short AlvorKit names:

- `Sphere3`, `Sphere3d`
- `Ray3`, `Ray3d`
- `Obb3`, `Obb3d`
- `Segment3`, `Segment3d`
- `Capsule3`, `Capsule3d`
- `Triangle3`, `Triangle3d`

`Obb` is acceptable because the term is common in game math and avoids long type
names. XML documentation should spell out "oriented bounding box" in summaries.

Use `Intervalf` and `Intervald` if an interval type is added as a concrete
primitive. Avoid `Interval3` because the interval is one-dimensional even when
it is used by a 3D ray.

Prefer `Classify` for methods returning `ContainmentKind` or
`PlaneIntersectionKind`.

Prefer `Intersects` for boolean broad tests.

Prefer `TryIntersect` when an intersection returns a distance, interval, point,
or other output that only exists when the intersection succeeds.

## Shared Enums And Helpers

### `ContainmentKind`

Already present:

```csharp
public enum ContainmentKind
{
    Disjoint,
    Intersects,
    Contains,
}
```

Use this for shape containment from the receiver's perspective.

### `PlaneIntersectionKind`

Add this when the second phase needs plane classifiers:

```csharp
public enum PlaneIntersectionKind
{
    Negative,
    Intersecting,
    Positive,
}
```

The names should describe the side of the plane using the sign of
`Plane3.Evaluate(point)`.

### `Intervalf` / `Intervald`

Intervals represent a one-dimensional inclusive range. They are useful for ray
entry and exit distances, slab tests, and clipped parameter ranges.

Candidate API:

```csharp
public readonly struct Intervalf
{
    public static Intervalf Empty { get; }

    public float Min { get; }
    public float Max { get; }

    public bool IsEmpty { get; }
    public float Length { get; }
    public float Center { get; }

    public static Intervalf Create(float min, float max);
    public static Intervalf CreateFromEndpoints(float first, float second);

    public bool Contains(float value);
    public bool Contains(Intervalf other);
    public bool Intersects(Intervalf other);

    public static Intervalf Union(Intervalf left, Intervalf right);
    public static Intervalf Intersection(Intervalf left, Intervalf right);
}
```

Open decision: whether intervals should be generated primitives or a small
hand-authored pair. Generated is preferable if formatting, parsing, scalar-family
conversion, and tests can fit the existing generator model cleanly.

## `Sphere3` And `Sphere3d`

A sphere is a ball in 3D space.

Storage:

```csharp
public Vec3 Center { get; }
public float Radius { get; }
```

Core API:

```csharp
public float Diameter { get; }
public float RadiusSquared { get; }
public bool IsEmpty { get; }

public static Sphere3 Create(Vec3 center, float radius);
public static Sphere3 CreateFromBox(Box3 box);
public static bool TryCreateFromPoints(ReadOnlySpan<Vec3> points, out Sphere3 result);

public bool Contains(Vec3 point);
public bool Contains(Sphere3 sphere);
public bool Intersects(Sphere3 sphere);
public bool Intersects(Box3 box);

public Vec3 ClosestPoint(Vec3 point);
public float DistanceTo(Vec3 point);
public float DistanceSquaredTo(Vec3 point);

public Sphere3 Translated(Vec3 offset);
public Sphere3 Scaled(float scale);

public static Sphere3 Union(Sphere3 left, Sphere3 right);
```

Integration:

```csharp
public bool Box3.Contains(Sphere3 sphere);
public bool Box3.Intersects(Sphere3 sphere);

public bool Frustum3.Contains(Sphere3 sphere);
public bool Frustum3.Intersects(Sphere3 sphere);
public ContainmentKind Frustum3.Classify(Sphere3 sphere);

public PlaneIntersectionKind Plane3.Classify(Sphere3 sphere);
```

Implementation notes:

- Reject negative radii or model them as empty. Do not silently take absolute
  value.
- Frustum-sphere tests must not assume normalized frustum planes.
- `CreateFromPoints` can start as a simple approximate bounding sphere. If an
  exact minimal sphere is not implemented, document the approximation plainly.

## `Ray3` And `Ray3d`

A ray starts at an origin and extends infinitely in one direction.

Storage:

```csharp
public Vec3 Origin { get; }
public Vec3 Direction { get; }
```

Core API:

```csharp
public static Ray3 Create(Vec3 origin, Vec3 direction);

public Vec3 PointAt(float distance);
public Ray3 Translated(Vec3 offset);
public Ray3 Normalized();
public bool TryNormalize(out Ray3 result);

public bool Intersects(Plane3 plane);
public bool Intersects(Box3 box);
public bool Intersects(Sphere3 sphere);
public bool Intersects(Frustum3 frustum);

public bool TryIntersect(Plane3 plane, out float distance);
public bool TryIntersect(Box3 box, out float distance);
public bool TryIntersect(Sphere3 sphere, out float distance);
public bool TryIntersect(Frustum3 frustum, out Intervalf distances);
```

Implementation notes:

- Do not normalize direction automatically.
- Document that returned distances are in units of the ray direction length
  unless the ray has been normalized.
- A zero direction ray is degenerate. Boolean methods should return `false`
  unless the query has a well-defined point-only meaning.
- Prefer `TryIntersect` over returning nullable floats.

## Plane Classification

Add plane classification after `Sphere3` and `Ray3` are present.

Candidate API:

```csharp
public PlaneIntersectionKind Plane3.Classify(Vec3 point);
public PlaneIntersectionKind Plane3.Classify(Box3 box);
public PlaneIntersectionKind Plane3.Classify(Sphere3 sphere);
public PlaneIntersectionKind Plane3.Classify(Frustum3 frustum);
```

For points, `Intersecting` should mean the evaluation is exactly zero. Do not add
tolerance parameters until there is a real caller. If tolerance becomes needed,
prefer a distinct overload that accepts a scalar tolerance, not a global epsilon.

## Frustum Improvements

The current `Frustum3` is a six-plane clipping volume with named planes. Keep it
as the general shape. Do not rename it to `ViewFrustum3` unless a later project
adds another meaning for frustum.

Add plane-array construction:

```csharp
public static Frustum3 CreateFromPlanes(ReadOnlySpan<Plane3> planes);
public static bool TryCreateFromPlanes(ReadOnlySpan<Plane3> planes, out Frustum3 result);
```

Plane order:

```text
Left, Right, Bottom, Top, Near, Far
```

Clarify corner order in XML documentation. Proposed order:

```text
Near bottom-left
Near bottom-right
Near top-left
Near top-right
Far bottom-left
Far bottom-right
Far top-left
Far top-right
```

Add sphere relationships after `Sphere3`:

```csharp
public bool Contains(Sphere3 sphere);
public bool Intersects(Sphere3 sphere);
public ContainmentKind Classify(Sphere3 sphere);
```

Add frustum relationships after finite/infinite behavior is decided:

```csharp
public bool Contains(Frustum3 other);
public bool Intersects(Frustum3 other);
public ContainmentKind Classify(Frustum3 other);
public bool TryClassify(Frustum3 other, out ContainmentKind result);
```

Open decision:

- Finite frustum relationships can use corner extraction.
- Infinite frustums can fail finite-corner extraction.
- If precise infinite-frustum relationships are not implemented in the first
  pass, prefer `TryClassify` over pretending the answer is always available.

Add optional finite helpers:

```csharp
public bool HasFiniteCorners { get; }
public bool TryCreateBoundingBox(out Box3 box);
public bool TryCopyNormalizedPlanesTo(Span<Plane3> destination);
```

Do not replace fast `Intersects(Box3)` semantics. If precise box/frustum testing
is needed, add it with an explicit name:

```csharp
public bool IntersectsPrecise(Box3 box);
public ContainmentKind ClassifyPrecise(Box3 box);
```

Document that the fast box test is suitable for culling and may be conservative.

## `Obb3` And `Obb3d`

An OBB is an oriented bounding box: a box with its own rotation.

Storage:

```csharp
public Vec3 Center { get; }
public Vec3 HalfSize { get; }
public Quat Orientation { get; }
```

Core API:

```csharp
public static Obb3 Create(Vec3 center, Vec3 halfSize, Quat orientation);
public static Obb3 CreateFromBox(Box3 box);
public static Obb3 Transform(Box3 box, Mat4 transform);

public Vec3 Size { get; }
public bool IsEmpty { get; }

public bool Contains(Vec3 point);
public bool Contains(Sphere3 sphere);
public bool Intersects(Sphere3 sphere);
public bool Intersects(Box3 box);
public bool Intersects(Obb3 other);
public bool Intersects(Plane3 plane);
public bool Intersects(Frustum3 frustum);

public Vec3 ClosestPoint(Vec3 point);
public void CopyCornersTo(Span<Vec3> destination);
public bool TryCopyCornersTo(Span<Vec3> destination);
```

Integration:

```csharp
public bool Frustum3.Intersects(Obb3 box);
public ContainmentKind Frustum3.Classify(Obb3 box);
public PlaneIntersectionKind Plane3.Classify(Obb3 box);
```

Implementation notes:

- Do not use scalar-axis overloads.
- Do not silently normalize `Orientation`.
- Intersection with another OBB should use SAT when implemented.
- It is acceptable to ship OBB/frustum and OBB/plane before full OBB/OBB if the
  API is staged carefully.

## `Segment3` And `Segment3d`

A segment is a finite line between two points.

Storage:

```csharp
public Vec3 Start { get; }
public Vec3 End { get; }
```

Core API:

```csharp
public Vec3 Direction { get; }
public float Length { get; }
public float LengthSquared { get; }
public Vec3 Center { get; }

public static Segment3 Create(Vec3 start, Vec3 end);

public Vec3 PointAt(float amount);
public Vec3 ClosestPoint(Vec3 point);
public float DistanceTo(Vec3 point);
public float DistanceSquaredTo(Vec3 point);

public bool Intersects(Sphere3 sphere);
public bool Intersects(Box3 box);
public bool TryIntersect(Plane3 plane, out float amount);
```

Implementation notes:

- `PointAt(0)` returns `Start`; `PointAt(1)` returns `End`.
- Do not clamp `amount` in `PointAt`. If needed, add `PointAtClamped`.
- Segment-plane intersection should return the segment parameter, not a world
  distance.

## `Capsule3` And `Capsule3d`

A capsule is a line segment swept by a radius. It is common for character
controllers, hitboxes, and swept tests.

Storage:

```csharp
public Vec3 Start { get; }
public Vec3 End { get; }
public float Radius { get; }
```

Core API:

```csharp
public Segment3 Segment { get; }
public Vec3 Center { get; }
public float Length { get; }
public float Diameter { get; }

public static Capsule3 Create(Vec3 start, Vec3 end, float radius);

public bool Contains(Vec3 point);
public bool Intersects(Sphere3 sphere);
public bool Intersects(Capsule3 capsule);
public bool Intersects(Box3 box);
public bool Intersects(Plane3 plane);

public Vec3 ClosestPoint(Vec3 point);
public float DistanceTo(Vec3 point);
public float DistanceSquaredTo(Vec3 point);
```

Integration:

```csharp
public bool Frustum3.Intersects(Capsule3 capsule);
public ContainmentKind Frustum3.Classify(Capsule3 capsule);
```

Implementation notes:

- Reject negative radii or model them as empty. Do not silently take absolute
  value.
- Capsule-box and capsule-frustum can be staged after simpler point, sphere,
  capsule, and plane operations.

## `Triangle3` And `Triangle3d`

A triangle is one finite mesh face in 3D space.

Storage:

```csharp
public Vec3 A { get; }
public Vec3 B { get; }
public Vec3 C { get; }
```

Core API:

```csharp
public Vec3 EdgeAB { get; }
public Vec3 EdgeAC { get; }
public Vec3 Normal { get; }
public Plane3 Plane { get; }
public float Area { get; }
public bool IsDegenerate { get; }

public static Triangle3 Create(Vec3 a, Vec3 b, Vec3 c);

public Vec3 Barycentric(Vec3 point);
public bool Contains(Vec3 point);
public Vec3 ClosestPoint(Vec3 point);
public float DistanceTo(Vec3 point);
public float DistanceSquaredTo(Vec3 point);

public bool Intersects(Ray3 ray);
public bool TryIntersect(Ray3 ray, out float distance);
public bool Intersects(Box3 box);
public bool Intersects(Sphere3 sphere);
```

Implementation notes:

- `Normal` should either return the unit normal or be named `NormalUnsafe` /
  `UnnormalizedNormal`. Prefer a clear pair:
  `Normal` for unit normal and `UnnormalizedNormal` for raw cross product.
- `Plane` creation should fail or produce a documented result for degenerate
  triangles. A `TryCreatePlane(out Plane3 plane)` method may be clearer than a
  property.
- Ray-triangle should use a stable algorithm and return distance along the ray.

## Viewport, Project, Unproject, And Picking

Projection helpers are the main bridge between math types and editor/game input.
They are implemented as `Viewport` and `Viewportd` because the viewport is a
real value: 2D window bounds plus a normalized depth interval.

Core API:

```csharp
public struct Viewport
{
    public Box2 Bounds;
    public Intervalf Depth;

    public Vec2 Size { get; }
    public Vec2 Center { get; }

    public static Viewport Create(Box2 bounds);
    public static Viewport Create(Box2 bounds, Intervalf depth);
    public static Viewport Create(ReadOnlySpan<float> values);

    public Vec4 ToViewportVector();
    public Mat4 CreateTransform(ProjectionDepthRange depthRange);
    public Vec3 Project(Vec3 source, Mat4 clipFromSource, ProjectionDepthRange depthRange);
    public Vec3 UnProject(Vec3 screen, Mat4 sourceFromClip, ProjectionDepthRange depthRange);
    public Ray3 CreatePickRay(Vec2 screen, Mat4 worldFromClip, ProjectionDepthRange depthRange);
    public Mat4 CreatePickMatrix(Box2 selection);
}
```

Implementation notes:

- `Bounds` uses `Box2`; there are no scalar-coordinate overloads.
- `Depth` uses `Intervalf` / `Intervald`.
- `Project` takes `clipFromSource`.
- `UnProject` and `CreatePickRay` take `sourceFromClip` / `worldFromClip`, so
  callers can explicitly pass the already-inverted clip matrix.

## Generated Architecture

Default to generation for shape families that need float and double variants,
common formatting, parsing, span APIs, scalar-family conversion, and interface
hierarchy support.

Likely generated:

- `Sphere3` / `Sphere3d`
- `Ray3` / `Ray3d`
- `Obb3` / `Obb3d`
- `Segment3` / `Segment3d`
- `Capsule3` / `Capsule3d`
- `Triangle3` / `Triangle3d`
- `Intervalf` / `Intervald`, if it fits the generator cleanly

Likely hand-authored:

- `PlaneIntersectionKind`
- Small shared algorithms if they are clearer as reusable internal
  collaborators than generated text

For every generated family, add matching core interfaces under
`src/AlvorKit.Maths.Core` first. The interfaces should express stable concepts,
not generator internals.

## Testing Plan

Each shape family should include tests for:

- Construction and property values.
- Empty or degenerate cases.
- Formatting and parsing.
- Span copy and creation.
- Equality, comparison, and hash behavior.
- Float/double conversion where present.
- Point containment.
- Pairwise relationships introduced in that phase.
- Boundary behavior, such as touching sphere/box/frustum surfaces.
- Negative-radius rejection or empty handling.
- Zero-length segment and zero-direction ray behavior.
- Non-normalized plane and ray behavior where applicable.

Frustum tests should explicitly cover:

- OpenGL default depth range.
- Zero-to-one depth range.
- Infinite projection behavior.
- Conservative box culling behavior.
- Sphere tests with unnormalized planes.
- Finite corner extraction failures where expected.

Run focused coverage for touched source projects after implementation. The
touched maths modules should aim for complete line and method coverage, with
branch coverage high enough to satisfy the repo gate without contorting tests
around compiler-shaped branches.

## Task Breakdown

### Phase 1: Sphere

- [x] Add `ISphere3` and related transform or shape interfaces if needed.
- [x] Generate `Sphere3` and `Sphere3d`.
- [x] Add sphere tests.
- [x] Add box-sphere relationships.
- [x] Add frustum-sphere relationships.
- [x] Add plane-sphere classification if `PlaneIntersectionKind` is introduced in
  this phase.

### Phase 2: Ray And Interval

- [x] Decide concrete interval shape.
- [x] Add `Intervalf` and `Intervald`, or a small equivalent.
- [x] Add `IRay3`.
- [x] Generate `Ray3` and `Ray3d`.
- [x] Add ray-plane, ray-box, ray-sphere, and ray-frustum queries.
- [x] Add tests for normalized and non-normalized ray behavior.

### Phase 3: Plane Classification

- [x] Add `PlaneIntersectionKind`.
- [x] Add plane classifiers for point, box, sphere, and frustum.
- [x] Add tests around positive, negative, touching, and crossing cases.

### Phase 4: Frustum Polish

- [x] Add `CreateFromPlanes(ReadOnlySpan<Plane3>)`.
- [x] Add `TryCreateFromPlanes`.
- [x] Clarify plane and corner order documentation.
- [x] Add finite helper APIs if needed.
- [x] Add frustum-frustum classification if finite/infinite behavior is settled.
- [x] Document conservative box/frustum semantics.

### Phase 5: Oriented Box

- [x] Add `IObb3`.
- [x] Generate `Obb3` and `Obb3d`.
- [x] Add corner extraction, point containment, closest point, and plane
  classification.
- [x] Add OBB/sphere and OBB/frustum relationships.
- [x] Add OBB/OBB SAT when ready.

### Phase 6: Segment

- [x] Add `ISegment3`.
- [x] Generate `Segment3` and `Segment3d`.
- [x] Add closest-point and distance queries.
- [x] Add segment-plane, segment-sphere, and segment-box queries.

### Phase 7: Capsule

- [x] Add `ICapsule3`.
- [x] Generate `Capsule3` and `Capsule3d`.
- [x] Add point, sphere, capsule, plane, and box relationships.
- [x] Add frustum-capsule relationship if culling code needs it.

### Phase 8: Triangle

- [x] Add `ITriangle3`.
- [x] Generate `Triangle3` and `Triangle3d`.
- [x] Add normal, area, barycentric, closest point, and ray-triangle queries.
- [x] Add box and sphere relationships if useful to mesh or picking code.

### Phase 9: Viewport And Picking

- [x] Add `Viewport` or equivalent.
- [x] Add project and unproject helpers.
- [x] Add screen-point pick-ray creation.
- [x] Add tests for OpenGL default depth range and zero-to-one depth range.

## Deferred Features

Do not add these in the first pass unless a real caller appears:

- Integer variants of continuous geometry types.
- Global epsilon policies.
- Scalar-coordinate overloads for vector-shaped values.
- Camera parameter extraction from arbitrary frustums.
- Width-at-depth and height-at-depth helpers on `Frustum3`.
- Direct `System.Numerics` interop for shapes that do not exist in
  `System.Numerics`.
- Exact minimal bounding sphere unless needed.
- Full mesh, polygon, convex hull, or broad-phase acceleration structures.
- Physics-engine style sweep manifolds and contact manifolds.

## Resolved Decisions

- Negative-radius spheres and capsules represent empty shapes. The generated
  types expose `Empty`, and relationship helpers treat empty shapes as
  non-intersecting unless a specific classifier documents otherwise.
- `Intervalf` and `Intervald` are generated primitives.
- `Ray3.Direction` and `Ray3d.Direction` may be zero. `Try...` intersection
  methods return `false` for degenerate directions where no stable query result
  exists.
- Frustum-frustum classification supports finite frustums directly. `TryClassify`
  exposes the precise path, while the non-try classifier uses the documented
  conservative fallback for cases it cannot prove exactly.
- Viewport projection, unprojection, pick-ray, and pick-matrix helpers live on
  `Viewport` / `Viewportd`.
- `Triangle3.Normal` and `Triangle3d.Normal` return normalized vectors, with
  `UnnormalizedNormal` available for the raw cross product.

## Recommended Implementation Order

1. `Sphere3` / `Sphere3d`.
2. Box-sphere, frustum-sphere, and plane-sphere integration.
3. `Intervalf` / `Intervald`.
4. `Ray3` / `Ray3d`.
5. Ray-plane, ray-box, ray-sphere, and ray-frustum integration.
6. `PlaneIntersectionKind` and plane classifiers.
7. Frustum plane-span construction and documentation polish.
8. Frustum-frustum classification.
9. `Obb3` / `Obb3d`.
10. `Segment3` / `Segment3d`.
11. `Capsule3` / `Capsule3d`.
12. `Triangle3` / `Triangle3d`.
13. Viewport project, unproject, and pick-ray helpers.

This order gives the most useful engine-facing queries early while keeping the
API coherent. Sphere and ray unlock culling and picking. Plane classification
then makes the relationship vocabulary consistent. OBB, capsule, and triangle
come after the simpler primitives so their tests and algorithms can reuse the
foundation instead of inventing parallel conventions.
