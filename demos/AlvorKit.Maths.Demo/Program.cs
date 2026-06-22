Console.WriteLine("AlvorKit.Maths demo");

Section("Floating vectors");

Vec3 velocity = (3f, 4f, 0f);
var direction = velocity.Normalized;
var displacement = direction * 12f * (1f / 60f);

Print("velocity", Format3(velocity));
Print("length", velocity.Length.ToString("0.###", CultureInfo.InvariantCulture));
Print("normalized", Format3(direction));
Print("one 60 Hz step at speed 12", Format3(displacement));

ExpectClose(velocity.Length, 5f, "Vec3 length should use all three components.");
ExpectClose3(direction, (0.6f, 0.8f, 0f), "Vec3.Normalized should keep direction and unit length.");
ExpectClose3(displacement, (0.12f, 0.16f, 0f), "Scalar and vector products should compose naturally.");

Section("Swizzles and composition");

Vec4 color = (0.10f, 0.20f, 0.40f, 1f);
var redGreen = color.RG;
Vec3 sample = (1f, 2f, 3f);
sample.YX = (8f, 9f);
var swappedBeforeColorWrite = sample.XY;
sample.BGR = (4f, 5f, 6f);

Print("rgba color", Format4(color));
Print("color.RG", Format2(redGreen));
Print("after YX write", Format2(swappedBeforeColorWrite));
Print("after BGR write", Format3(sample.RGB));
Print("repeated swizzle", Format4(sample.RRGB));

ExpectClose2(redGreen, (0.10f, 0.20f), "Vec4 color aliases should read the same component storage.");
ExpectClose2(swappedBeforeColorWrite, (9f, 8f), "Writable swizzles should assign each distinct component once.");
ExpectClose3(sample.RGB, (6f, 5f, 4f), "RGB aliases should match XYZ storage.");
ExpectClose4(sample.RRGB, (6f, 6f, 5f, 4f), "Read swizzles may repeat components.");

Section("Masks and selection");

Vec3 requestedPosition = (12f, -3f, 5f);
var worldMin = Vec3.Zero;
Vec3 worldMax = (10f, 10f, 10f);
var clampedPosition = Vec3.Clamp(requestedPosition, worldMin, worldMax);
var outsideWorld = requestedPosition < worldMin | requestedPosition > worldMax;
var acceptedPosition = outsideWorld.Select(clampedPosition, requestedPosition);

Print("requested", Format3(requestedPosition));
Print("outside mask", Format3b(outsideWorld));
Print("clamped", Format3(acceptedPosition));
Print("mask.Any", outsideWorld.Any.ToString());

Expect(outsideWorld == (true, true, false), "Relational operators should return component masks.");
ExpectClose3(acceptedPosition, (10f, 0f, 5f), "Mask selection should pick only the corrected components.");

Section("Integer grids");

Vec3i tile = (27, 18, 7);
var chunk = tile >> 3;
var localTile = tile & 0b111;
var powerOfTwo = Vec3i.IsPowerOfTwo((16, 18, 32));

Print("tile", Format3i(tile));
Print("tile >> 3", Format3i(chunk));
Print("tile & 7", Format3i(localTile));
Print("power-of-two mask", Format3b(powerOfTwo));

Expect(chunk == (3, 2, 0), "Integer vectors should shift each component.");
Expect(localTile == (3, 2, 7), "Bitwise operators should apply component-wise.");
Expect(powerOfTwo == (true, false, true), "Integer helper masks should preserve per-component answers.");

Section("Boxes and bounds");

var playfield = Box2.CreateFromCenterSize((0f, 0f), (20f, 12f));
Vec2 requestedSpawn = (12f, -8f);
var safeSpawn = playfield.ClosestPoint(requestedSpawn);
var paddedPlayfield = playfield.Inflated(new Vec2(2f));
var tileBounds = new Box2i(new Vec2i(0, 0), new Vec2i(16, 9));
var chunkBounds = new Box3i(new Vec3i(0, 0, 0), new Vec3i(16, 8, 4));

Print("playfield", playfield.ToString("0.###", CultureInfo.InvariantCulture));
Print("requested spawn", Format2(requestedSpawn));
Print("safe spawn", Format2(safeSpawn));
Print("padded size", Format2(paddedPlayfield.Size));
Print("tile bounds size", Format2i(tileBounds.Size));
Print("chunk volume", chunkBounds.Volume.ToString(CultureInfo.InvariantCulture));

Expect(!playfield.Contains(requestedSpawn), "Box containment should identify points outside a world bound.");
Expect(playfield.Contains(safeSpawn), "ClosestPoint should clamp a point into the box.");
ExpectClose2(safeSpawn, (10f, -6f), "ClosestPoint should clamp each axis independently.");
ExpectClose2(paddedPlayfield.Size, (24f, 16f), "Inflated should expand both min and max corners.");
Expect(tileBounds.Area == 144, "Integer boxes should be useful for tile and viewport extents.");
Expect(chunkBounds.Volume == 512, "3D integer boxes should report chunk volume.");

Section("Spheres and intervals");

var detectionSphere = new Sphere3(Vec3.Zero, 5f);
var lootBounds = Box3.CreateFromCenterSize((3f, 0f, 0f), (2f, 2f, 2f));
var lootSphere = Sphere3.CreateFromBox(lootBounds);
Vec3 signalPoint = (8f, 0f, 0f);
var closestSignalPoint = detectionSphere.ClosestPoint(signalPoint);
var patrolRange = new Intervalf(2f, 8f);
var rayHitRange = Intervalf.CreateFromEndpoints(6f, 3f);
var visibleRange = Intervalf.Intersection(patrolRange, rayHitRange);
var storageBox = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(6f));
var storedSphere = new Sphere3(new Vec3(1f, 0f, 0f), 1f);
var edgeSphere = new Sphere3(new Vec3(3.5f, 0f, 0f), 1f);

Print("detection sphere", FormatSphere(detectionSphere));
Print("loot sphere from box", FormatSphere(lootSphere));
Print("signal closest point", Format3(closestSignalPoint));
Print("signal distance", detectionSphere.DistanceTo(signalPoint).ToString("0.###", CultureInfo.InvariantCulture));
Print("patrol range", FormatInterval(patrolRange));
Print("ray hit range", FormatInterval(rayHitRange));
Print("visible range", FormatInterval(visibleRange));
Print("box contains sphere", storageBox.Contains(storedSphere).ToString());
Print("box intersects edge sphere", storageBox.Intersects(edgeSphere).ToString());

Expect(detectionSphere.Contains(lootSphere), "Sphere containment should account for the contained sphere radius.");
ExpectClose(lootSphere.Radius, MathF.Sqrt(3f), "CreateFromBox should produce a centered sphere containing all box corners.");
ExpectClose3(closestSignalPoint, (5f, 0f, 0f), "Sphere ClosestPoint should clamp to the sphere surface.");
ExpectClose(detectionSphere.DistanceTo(signalPoint), 3f, "Sphere DistanceTo should report zero inside and positive distance outside.");
Expect(rayHitRange == new Intervalf(3f, 6f), "CreateFromEndpoints should sort interval endpoints.");
Expect(visibleRange == new Intervalf(3f, 6f), "Interval intersection should keep only the overlapping distances.");
Expect(patrolRange.Contains(visibleRange), "Interval containment should work for nested ranges.");
Expect(storageBox.Contains(storedSphere), "Box containment should require the whole sphere to fit inside the box.");
Expect(!storageBox.Contains(edgeSphere), "A sphere crossing a box face should not be fully contained.");
Expect(storageBox.Intersects(edgeSphere), "Box/sphere intersection should count touching or crossing boundaries.");

Section("Span interop");

Span<float> packed = stackalloc float[Vec3.ComponentCount];
direction.CopyTo(packed);
var roundTrip = Vec3.Create(packed);

Print("packed span", $"[{packed[0]:0.###}, {packed[1]:0.###}, {packed[2]:0.###}]");
Print("span round-trip", Format3(roundTrip));

ExpectClose3(roundTrip, direction, "Vec3 should round-trip through a component span.");
ExpectClose3((packed[0], packed[1], packed[2]), direction, "CopyTo should preserve component order.");

Section("Matrix algebra and layout");

Mat3 scaleBasis = Mat3.CreateDiagonal((2f, 3f, 4f));
var orthonormalBasis = scaleBasis.Orthonormalized;
var outerProduct = Mat3.CreateOuterProduct((1f, 2f, 3f), (4f, 5f, 6f));
var blendedBasis = Mat3.Lerp(Mat3.Identity, scaleBasis, 0.5f);
Span<float> basisColumnMajor = stackalloc float[9];
scaleBasis.CopyToColumnMajor(basisColumnMajor);

Print("scale basis", FormatMat3(scaleBasis));
Print("basis diagonal", Format3(scaleBasis.Diagonal));
Print("basis trace", scaleBasis.Trace.ToString("0.###", CultureInfo.InvariantCulture));
Print("basis determinant", scaleBasis.Determinant.ToString("0.###", CultureInfo.InvariantCulture));
Print("outer product row 1", Format3(outerProduct.Row1));
Print("lerped diagonal", Format3(blendedBasis.Diagonal));
Print("packed first column", $"[{basisColumnMajor[0]:0.###}, {basisColumnMajor[1]:0.###}, {basisColumnMajor[2]:0.###}]");

ExpectClose3(scaleBasis.Diagonal, (2f, 3f, 4f), "Matrix diagonal helpers should expose the primary scale terms.");
ExpectClose(scaleBasis.Trace, 9f, "Matrix trace should add the square diagonal.");
ExpectClose(scaleBasis.Determinant, 24f, "Diagonal matrix determinant should multiply the diagonal terms.");
ExpectCloseMat3(orthonormalBasis, Mat3.Identity, "Orthonormalized should turn scaled basis vectors back into unit axes.");
ExpectClose3(outerProduct.Row1, (8f, 10f, 12f), "Outer products should combine a column vector with a row vector.");
ExpectClose3(blendedBasis.Diagonal, (1.5f, 2f, 2.5f), "Matrix interpolation should lerp each component.");

Section("2D affine matrices");

Mat3x2 spriteTransform =
    Mat3x2.CreateTranslation((8f, 3f)) *
    Mat3x2.CreateRotation(MathF.PI / 2f) *
    Mat3x2.CreateScale((2f, 1f));
Vec2 localCorner = (1f, 2f);
var worldCorner = Mat3x2.TransformPoint(spriteTransform, localCorner);
var worldXAxis = Mat3x2.TransformVector(spriteTransform, Vec2.UnitX);
var affineInverseOk = Mat3x2.TryInvert(spriteTransform, out var inverseSpriteTransform);
var recoveredCorner = Mat3x2.TransformPoint(inverseSpriteTransform, worldCorner);
var affineIdentityCheck = spriteTransform * inverseSpriteTransform;

Print("sprite transform", FormatMat3x2(spriteTransform));
Print("local corner", Format2(localCorner));
Print("world corner", Format2(worldCorner));
Print("world x axis", Format2(worldXAxis));
Print("affine translation", Format2(spriteTransform.Translation));
Print("affine inverse", FormatMat3x2(affineIdentityCheck));

Expect(affineInverseOk, "Affine transforms that scale by non-zero values should be invertible.");
ExpectClose2(worldCorner, (6f, 5f), "Mat3x2 should compose scale, rotation, and translation for 2D points.");
ExpectClose2(worldXAxis, (0f, 2f), "TransformVector should ignore translation and preserve linear transform terms.");
ExpectClose2(recoveredCorner, localCorner, "Affine inverse should recover the original local point.");
ExpectCloseMat3x2(affineIdentityCheck, Mat3x2.AffineIdentity, "Affine inverse should compose back to identity.");

Section("3D camera matrices");

Mat4 model = Mat4.CreateWorld((1f, 2f, 0f), Vec3.UnitZ, Vec3.UnitY);
Mat4 view = Mat4.LookAt((0f, 0f, 5f), Vec3.Zero, Vec3.UnitY);
Mat4 projection = Mat4.CreatePerspectiveFieldOfView(MathF.PI / 3f, 16f / 9f, 0.1f, 100f);
Mat4 explicitProjection = Mat4.CreatePerspectiveFieldOfView(
    MathF.PI / 3f,
    16f / 9f,
    0.1f,
    100f,
    ProjectionHandedness.Right,
    ProjectionDepthRange.NegativeOneToOne);
Mat4 modelView = view * model;
Mat4 modelViewProjection = projection * modelView;
Vec4 viewport = (0f, 0f, 1920f, 1080f);
Vec3 objectOrigin = Vec3.Zero;
var worldOrigin = Mat4.TransformPoint(model, objectOrigin);
var clipOrigin = modelViewProjection * new Vec4(objectOrigin, 1f);
var normalizedOrigin = clipOrigin / clipOrigin.W;
var windowOrigin = Mat4.Project(objectOrigin, modelView, projection, viewport);
var viewportOrigin = Mat4.CreateViewport(viewport) * new Vec4(normalizedOrigin.X, normalizedOrigin.Y, normalizedOrigin.Z, 1f);
var unprojectedOrigin = Mat4.UnProject(windowOrigin, modelView, projection, viewport);
Span<float> modelViewProjectionColumnMajor = stackalloc float[Mat4.ComponentCount];
Span<char> projectionText = stackalloc char[256];
modelViewProjection.CopyToColumnMajor(modelViewProjectionColumnMajor);
var projectionTextOk = projection.TryFormat(
    projectionText,
    out var projectionCharsWritten,
    "0.###",
    CultureInfo.InvariantCulture);
var parsedProjectionOk = Mat4.TryParse(
    projection.ToString("G9", CultureInfo.InvariantCulture),
    CultureInfo.InvariantCulture,
    out var parsedProjection);

Print("model translation", Format3(model.Translation));
Print("world origin", Format3(worldOrigin));
Print("window origin", Format3(windowOrigin));
Print("viewport origin", Format3((viewportOrigin.X, viewportOrigin.Y, viewportOrigin.Z)));
Print("mvp first column", Format4((
    modelViewProjectionColumnMajor[0],
    modelViewProjectionColumnMajor[1],
    modelViewProjectionColumnMajor[2],
    modelViewProjectionColumnMajor[3])));
Print("projection span text", projectionTextOk ? new string(projectionText[..projectionCharsWritten]) : "<too small>");

ExpectCloseMat4(projection, explicitProjection, "Default projection overloads should use OpenGL-style right-handed depth.");
ExpectClose3(model.Translation, (1f, 2f, 0f), "Mat4.Translation should expose the transform translation column.");
ExpectClose3(worldOrigin, (1f, 2f, 0f), "CreateWorld should place the local origin at the requested position.");
ExpectClose3((viewportOrigin.X, viewportOrigin.Y, viewportOrigin.Z), windowOrigin, "CreateViewport should match Project after perspective divide.");
ExpectClose3(unprojectedOrigin, objectOrigin, "Project and UnProject should round-trip object-space points.");
Expect(projectionTextOk, "Matrix TryFormat should write into caller-owned spans.");
Expect(parsedProjectionOk, "Matrix TryParse should accept the invariant ToString representation.");
ExpectCloseMat4(parsedProjection, projection, "Matrix ToString and TryParse should round-trip precise formatted values.");

Section("Frustums and rays");

Span<Plane3> boxFrustumPlanes =
[
    Plane3.Create(Vec3.UnitX, 1f),
    Plane3.Create(-Vec3.UnitX, 1f),
    Plane3.Create(Vec3.UnitY, 1f),
    Plane3.Create(-Vec3.UnitY, 1f),
    Plane3.Create(Vec3.UnitZ, 0f),
    Plane3.Create(-Vec3.UnitZ, 10f),
];
var boxFrustum = Frustum3.CreateFromPlanes(boxFrustumPlanes);
var pickRay = new Ray3(new Vec3(0f, 0f, -2f), new Vec3(0f, 0f, 2f));
var missedRay = new Ray3(new Vec3(2f, 0f, -2f), Vec3.UnitZ);
var targetBox = new Box3(new Vec3(-0.5f, -0.5f, 2f), new Vec3(0.5f, 0.5f, 4f));
var targetSphere = new Sphere3(new Vec3(0f, 0f, 6f), 1f);
var clippedSphere = new Sphere3(new Vec3(1.25f, 0f, 6f), 0.5f);
var rejectedSphere = new Sphere3(new Vec3(2f, 0f, 6f), 0.25f);
var normalizedPickRayOk = pickRay.TryNormalize(out var normalizedPickRay);
var frustumHit = pickRay.TryIntersect(boxFrustum, out var frustumDistances);
Intervalf boxDistances;
var boxHit = pickRay.TryIntersect(targetBox, out boxDistances);
var sphereHit = pickRay.TryIntersect(targetSphere, out var sphereDistance);
var nearPlaneHit = pickRay.TryIntersect(boxFrustum.Near, out var nearPlaneDistance);

Print("box frustum near", FormatPlane(boxFrustum.Near));
Print("pick ray", FormatRay(pickRay));
Print("normalized ray", FormatRay(normalizedPickRay));
Print("ray point at 2", Format3(pickRay.PointAt(2f)));
Print("frustum hit range", frustumHit ? FormatInterval(frustumDistances) : "<miss>");
Print("box hit range", boxHit ? FormatInterval(boxDistances) : "<miss>");
Print("sphere hit distance", sphereHit ? sphereDistance.ToString("0.###", CultureInfo.InvariantCulture) : "<miss>");
Print("frustum sphere class", boxFrustum.Classify(targetSphere).ToString());
Print("clipped sphere class", boxFrustum.Classify(clippedSphere).ToString());
Print("rejected sphere class", boxFrustum.Classify(rejectedSphere).ToString());
Print("missed frustum", missedRay.Intersects(boxFrustum).ToString());

Expect(normalizedPickRayOk, "Ray TryNormalize should succeed for nonzero directions.");
ExpectClose3(normalizedPickRay.Direction, Vec3.UnitZ, "Ray normalization should keep the origin and unit-length direction.");
ExpectClose3(pickRay.PointAt(2f), (0f, 0f, 2f), "Ray PointAt should use the stored, non-normalized direction.");
Expect(frustumHit, "A ray through the center should intersect the six-plane frustum.");
Expect(frustumDistances == new Intervalf(1f, 6f), "Ray/frustum intersections should return entry and exit distances.");
Expect(boxHit, "A ray through the center should intersect the target box.");
Expect(boxDistances == new Intervalf(2f, 3f), "Ray/box slab tests should return the clipped distance interval.");
Expect(sphereHit, "A ray through the center should intersect the target sphere.");
ExpectClose(sphereDistance, 3.5f, "Ray/sphere intersections should return the nearest nonnegative distance.");
Expect(nearPlaneHit, "The same ray should hit the frustum near plane.");
ExpectClose(nearPlaneDistance, 1f, "Plane intersection should respect non-normalized ray direction distances.");
Expect(boxFrustum.Contains(targetSphere), "Frustum sphere containment should require every plane to fully contain the sphere.");
Expect(boxFrustum.Classify(targetSphere) == ContainmentKind.Contains, "A centered sphere should be fully inside the frustum.");
Expect(boxFrustum.Classify(clippedSphere) == ContainmentKind.Intersects, "A sphere crossing a side plane should intersect the frustum.");
Expect(!boxFrustum.Intersects(rejectedSphere), "A sphere fully outside one frustum plane should be rejected.");
Expect(!missedRay.Intersects(boxFrustum), "A ray outside the side planes should miss the frustum.");

Section("Planes and shadows");

var floorPlane = Plane3.CreateFromPointNormal((0f, 1f, 0f), Vec3.UnitY);
Vec3 floatingPoint = (2f, 4f, -3f);
var pointOnFloor = floorPlane.ProjectPoint(floatingPoint);
var mirroredPoint = floorPlane.ReflectPoint(floatingPoint);
var reflectedByMatrix = Mat4.TransformPoint(Mat4.CreateReflection(floorPlane), floatingPoint);
var shadowPoint = Mat4.TransformPoint(Mat4.CreateShadow(new Vec3(0f, -1f, 0f), floorPlane), floatingPoint);
var belowFloor = new Vec3(2f, 0f, -3f);
var liftedSphere = new Sphere3(new Vec3(0f, 2.5f, 0f), 0.5f);
var floorCrossingBox = new Box3(new Vec3(-1f, 0.5f, -1f), new Vec3(1f, 1.5f, 1f));
Span<char> planeText = stackalloc char[64];
var planeTextOk = floorPlane.TryFormat(planeText, out var planeCharsWritten, "0.###", CultureInfo.InvariantCulture);

Print("floor plane", FormatPlane(floorPlane));
Print("floating point", Format3(floatingPoint));
Print("closest floor point", Format3(pointOnFloor));
Print("mirrored point", Format3(mirroredPoint));
Print("shadow point", Format3(shadowPoint));
Print("point side", floorPlane.Classify(floatingPoint).ToString());
Print("below side", floorPlane.Classify(belowFloor).ToString());
Print("box side", floorPlane.Classify(floorCrossingBox).ToString());
Print("sphere side", floorPlane.Classify(liftedSphere).ToString());
Print("plane span text", planeTextOk ? new string(planeText[..planeCharsWritten]) : "<too small>");

ExpectClose(floorPlane.Evaluate(pointOnFloor), 0f, "Plane projection should land back on the plane.");
ExpectClose3(pointOnFloor, (2f, 1f, -3f), "Plane ProjectPoint should move along the normal.");
ExpectClose3(mirroredPoint, reflectedByMatrix, "CreateReflection should match Plane3.ReflectPoint.");
ExpectClose3(shadowPoint, pointOnFloor, "Directional shadow projection should land on the plane.");
Expect(floorPlane.Classify(floatingPoint) == PlaneIntersectionKind.Positive, "Plane classification should expose the positive half-space.");
Expect(floorPlane.Classify(pointOnFloor) == PlaneIntersectionKind.Intersecting, "Points on the plane should classify as intersecting.");
Expect(floorPlane.Classify(belowFloor) == PlaneIntersectionKind.Negative, "Plane classification should expose the negative half-space.");
Expect(floorPlane.Classify(floorCrossingBox) == PlaneIntersectionKind.Intersecting, "Boxes crossing the plane should classify as intersecting.");
Expect(floorPlane.Classify(liftedSphere) == PlaneIntersectionKind.Positive, "Spheres fully above the plane should classify positive.");
Expect(planeTextOk, "Plane TryFormat should write into caller-owned spans.");

Section("Quaternion rotations");

Quat zQuarterTurn = Quat.CreateFromAxisAngle(Vec3.UnitZ, MathF.PI / 2f);
var quaternionRotated = zQuarterTurn * Vec3.UnitX;
var quaternionMatrix = Mat4.CreateRotation(zQuarterTurn);
var halfTurn = Quat.Slerp(Quat.Identity, zQuarterTurn, 0.5f);
var halfTurnVector = halfTurn * Vec3.UnitX;
var matrixRoundTrip = Quat.CreateFromRotationMatrix(quaternionMatrix);
Span<char> quaternionText = stackalloc char[128];
var quaternionTextOk = zQuarterTurn.TryFormat(
    quaternionText,
    out var quaternionCharsWritten,
    "0.###",
    CultureInfo.InvariantCulture);

Print("quarter turn quat", FormatQuat(zQuarterTurn));
Print("rotated +X", Format3(quaternionRotated));
Print("half slerp +X", Format3(halfTurnVector));
Print("matrix column 0", Format4(quaternionMatrix.Column0));
Print("matrix round-trip quat", FormatQuat(matrixRoundTrip));
Print("quaternion span text", quaternionTextOk ? new string(quaternionText[..quaternionCharsWritten]) : "<too small>");

ExpectClose3(quaternionRotated, Vec3.UnitY, "Quaternion axis-angle rotation should match the matrix rotation convention.");
ExpectClose3(halfTurnVector, (MathF.Sqrt(0.5f), MathF.Sqrt(0.5f), 0f), "Slerp should travel halfway along the rotation arc.");
ExpectCloseMat4(quaternionMatrix, Mat4.CreateRotation(MathF.PI / 2f, Vec3.UnitZ), "Quat-to-matrix should match Mat4 axis rotation.");
ExpectClose3(matrixRoundTrip * Vec3.UnitX, Vec3.UnitY, "Quat should round-trip through Mat4 rotation conversion.");
Expect(quaternionTextOk, "Quaternion TryFormat should write into caller-owned spans.");

Section("Scalar helpers");

var wrapped = ScalarMath.Modulo(-1.25f, 1f);
var eased = ScalarMath.SmoothStep(0f, 1f, 0.35f);
var bits = ScalarMath.BitCount(0b1011_0001);

Print("Modulo(-1.25, 1)", wrapped.ToString("0.###", CultureInfo.InvariantCulture));
Print("SmoothStep(0, 1, .35)", eased.ToString("0.###", CultureInfo.InvariantCulture));
Print("BitCount(0b10110001)", bits.ToString(CultureInfo.InvariantCulture));

ExpectClose(wrapped, 0.75f, "ScalarMath.Modulo should return GLSL-style positive remainders.");
ExpectClose(eased, 0.28175f, "ScalarMath.SmoothStep should use the cubic Hermite curve.");
Expect(bits == 4, "ScalarMath.BitCount should inspect the scalar bit pattern.");

Console.WriteLine();
Console.WriteLine("All maths checks passed.");

// Writes a section heading for the demo narrative.
static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

// Writes one aligned demo observation.
static void Print(string label, string value) =>
    Console.WriteLine($"{label,-30} {value}");

// Formats a two-component floating vector with invariant tuple-style text.
static string Format2(Vec2 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a three-component floating vector with invariant tuple-style text.
static string Format3(Vec3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a four-component floating vector with invariant tuple-style text.
static string Format4(Vec4 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a quaternion with invariant tuple-style text.
static string FormatQuat(Quat value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats an interval with invariant endpoint text.
static string FormatInterval(Intervalf value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a 3D sphere with invariant center and radius text.
static string FormatSphere(Sphere3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a 3D ray with invariant origin and direction text.
static string FormatRay(Ray3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a 3D plane with invariant coefficient text.
static string FormatPlane(Plane3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a compact 2D affine matrix with invariant tuple-style text.
static string FormatMat3x2(Mat3x2 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a 3x3 matrix with invariant tuple-style text.
static string FormatMat3(Mat3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats an integer vector with invariant tuple-style text.
static string Format3i(Vec3i value) =>
    value.ToString(CultureInfo.InvariantCulture);

// Formats an integer 2D vector with invariant tuple-style text.
static string Format2i(Vec2i value) =>
    value.ToString(CultureInfo.InvariantCulture);

// Formats a Boolean mask with tuple-style text.
static string Format3b(Vec3b value) =>
    value.ToString();

// Fails fast when a demo invariant no longer holds.
static void Expect(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

// Compares two floating scalars with a small tolerance for cross-platform math.
static void ExpectClose(float actual, float expected, string message) =>
    Expect(MathF.Abs(actual - expected) <= 0.00001f, message);

// Compares two floating vectors with a small tolerance for cross-platform math.
static void ExpectClose2(Vec2 actual, Vec2 expected, string message)
{
    ExpectClose(actual.X, expected.X, message);
    ExpectClose(actual.Y, expected.Y, message);
}

// Compares compact 2D affine matrices with a small tolerance for cross-platform math.
static void ExpectCloseMat3x2(Mat3x2 actual, Mat3x2 expected, string message)
{
    for (var column = 0; column < Mat3x2.ColumnCount; column++)
    {
        for (var row = 0; row < Mat3x2.RowCount; row++)
            ExpectClose(actual[column, row], expected[column, row], message);
    }
}

// Compares three-component floating vectors with a small tolerance for cross-platform math.
static void ExpectClose3(Vec3 actual, Vec3 expected, string message)
{
    ExpectClose(actual.X, expected.X, message);
    ExpectClose(actual.Y, expected.Y, message);
    ExpectClose(actual.Z, expected.Z, message);
}

// Compares 3x3 matrices with a small tolerance for cross-platform math.
static void ExpectCloseMat3(Mat3 actual, Mat3 expected, string message)
{
    for (var column = 0; column < Mat3.ColumnCount; column++)
    {
        for (var row = 0; row < Mat3.RowCount; row++)
            ExpectClose(actual[column, row], expected[column, row], message);
    }
}

// Compares four-component floating vectors with a small tolerance for cross-platform math.
static void ExpectClose4(Vec4 actual, Vec4 expected, string message)
{
    ExpectClose(actual.X, expected.X, message);
    ExpectClose(actual.Y, expected.Y, message);
    ExpectClose(actual.Z, expected.Z, message);
    ExpectClose(actual.W, expected.W, message);
}

// Compares 4x4 matrices with a small tolerance for cross-platform math.
static void ExpectCloseMat4(Mat4 actual, Mat4 expected, string message)
{
    for (var column = 0; column < Mat4.ColumnCount; column++)
    {
        for (var row = 0; row < Mat4.RowCount; row++)
            ExpectClose(actual[column, row], expected[column, row], message);
    }
}
