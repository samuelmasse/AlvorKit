Console.WriteLine("AlvorKit.Maths demo");

Section("Vectors, Masks, And Spans");

Vec3 velocity = new(3f, 4f, 0f);
var direction = velocity.Normalized;
var displacement = direction * 12f * (1f / 60f);
var requestedPosition = new Vec3(12f, -3f, 5f);
var clampedPosition = Vec3.Clamp(requestedPosition, Vec3.Zero, new Vec3(10f, 10f, 10f));
var outsideWorld = requestedPosition < Vec3.Zero | requestedPosition > new Vec3(10f, 10f, 10f);
Span<float> packedDirection = stackalloc float[Vec3.ComponentCount];
direction.CopyTo(packedDirection);
var unpackedDirection = Vec3.Create(packedDirection);

Print("velocity", Format3(velocity));
Print("length", velocity.Length.ToString("0.###", CultureInfo.InvariantCulture));
Print("normalized", Format3(direction));
Print("60 Hz displacement", Format3(displacement));
Print("outside mask", outsideWorld.ToString());
Print("clamped position", Format3(clampedPosition));
Print("span round-trip", Format3(unpackedDirection));

ExpectClose(velocity.Length, 5f, "Vec3 length should use all three components.");
ExpectClose3(direction, new Vec3(0.6f, 0.8f, 0f), "Vec3.Normalized should keep direction and unit length.");
ExpectClose3(displacement, new Vec3(0.12f, 0.16f, 0f), "Scalar and vector products should compose naturally.");
Expect(outsideWorld == new Vec3b(true, true, false), "Relational operators should return component masks.");
ExpectClose3(clampedPosition, new Vec3(10f, 0f, 5f), "Clamp should operate component-wise.");
ExpectClose3(unpackedDirection, direction, "Vec3 should round-trip through component spans.");

Section("Boxes, Intervals, And Spheres");

var playfield = Box2.CreateFromCenterSize(Vec2.Zero, new Vec2(20f, 12f));
var requestedSpawn = new Vec2(12f, -8f);
var safeSpawn = playfield.ClosestPoint(requestedSpawn);
var paddedPlayfield = playfield.Inflated(new Vec2(2f, 2f));
var storageBox = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(6f, 6f, 6f));
var storedSphere = new Sphere3(new Vec3(1f, 0f, 0f), 1f);
var edgeSphere = new Sphere3(new Vec3(3.5f, 0f, 0f), 1f);
var patrolRange = new Intervalf(2f, 8f);
var rayHitRange = Intervalf.CreateFromEndpoints(6f, 3f);
var visibleRange = Intervalf.Intersection(patrolRange, rayHitRange);
Vec3[] cloudPoints =
[
    new(-2f, 0f, 0f),
    new(2f, 0f, 0f),
    new(0f, 1f, 0f),
    new(0f, 0f, -1f),
];
var cloudSphere = Sphere3.CreateFromPoints(cloudPoints);
var emptyCloud = Sphere3.TryCreateFromPoints([], out var emptySphere);

Print("playfield", playfield.ToString("0.###", CultureInfo.InvariantCulture));
Print("safe spawn", Format2(safeSpawn));
Print("padded size", Format2(paddedPlayfield.Size));
Print("visible range", FormatInterval(visibleRange));
Print("box contains sphere", storageBox.Contains(storedSphere).ToString());
Print("box intersects edge sphere", storageBox.Intersects(edgeSphere).ToString());
Print("cloud sphere", FormatSphere(cloudSphere));
Print("empty cloud sphere", emptyCloud ? FormatSphere(emptySphere) : "<none>");

Expect(!playfield.Contains(requestedSpawn), "Box containment should identify points outside a world bound.");
Expect(playfield.Contains(safeSpawn), "ClosestPoint should clamp a point into the box.");
ExpectClose2(safeSpawn, new Vec2(10f, -6f), "ClosestPoint should clamp each axis independently.");
ExpectClose2(paddedPlayfield.Size, new Vec2(24f, 16f), "Inflated should expand both min and max corners.");
Expect(visibleRange == new Intervalf(3f, 6f), "Interval intersection should keep only overlapping distances.");
Expect(storageBox.Contains(storedSphere), "Box containment should require the whole sphere to fit inside the box.");
Expect(!storageBox.Contains(edgeSphere), "A sphere crossing a box face should not be fully contained.");
Expect(storageBox.Intersects(edgeSphere), "Box/sphere intersection should count touching or crossing boundaries.");
Expect(cloudSphere.Contains(new Vec3(2f, 0f, 0f)), "CreateFromPoints should contain every source point.");
Expect(!emptyCloud && emptySphere == Sphere3.Empty, "TryCreateFromPoints should return false for empty spans.");

Section("Matrices, Projections, And Quaternions");

Mat3 scaleBasis = Mat3.CreateDiagonal(new Vec3(2f, 3f, 4f));
var outerProduct = Mat3.CreateOuterProduct(new Vec3(1f, 2f, 3f), new Vec3(4f, 5f, 6f));
Mat4 model = Mat4.CreateWorld(new Vec3(1f, 2f, 0f), Vec3.UnitZ, Vec3.UnitY);
Mat4 view = Mat4.LookAt(new Vec3(0f, 0f, 5f), Vec3.Zero, Vec3.UnitY);
Mat4 projection = Mat4.CreatePerspectiveFieldOfView(MathF.PI / 3f, 16f / 9f, 0.1f, 100f);
Mat4 explicitProjection = Mat4.CreatePerspectiveFieldOfView(
    MathF.PI / 3f,
    16f / 9f,
    0.1f,
    100f,
    ProjectionHandedness.Right,
    ProjectionDepthRange.NegativeOneToOne);
Mat4 clipFromWorld = projection * view;
Mat4 clipFromObject = clipFromWorld * model;
Quat zQuarterTurn = Quat.CreateFromAxisAngle(Vec3.UnitZ, MathF.PI / 2f);
var quaternionRotated = zQuarterTurn * Vec3.UnitX;
var quaternionMatrix = Mat4.CreateRotation(zQuarterTurn);
var matrixRoundTrip = Quat.CreateFromRotationMatrix(quaternionMatrix);
Span<float> clipColumnMajor = stackalloc float[Mat4.ComponentCount];
clipFromObject.CopyToColumnMajor(clipColumnMajor);

Print("scale diagonal", Format3(scaleBasis.Diagonal));
Print("scale determinant", scaleBasis.Determinant.ToString("0.###", CultureInfo.InvariantCulture));
Print("outer row 1", Format3(outerProduct.Row1));
Print("model translation", Format3(model.Translation));
Print("world origin", Format3(Mat4.TransformPoint(model, Vec3.Zero)));
Print("projection default", projection == explicitProjection ? "OpenGL RH -1..1" : "custom");
Print("clip first column", Format4(new Vec4(clipColumnMajor[0], clipColumnMajor[1], clipColumnMajor[2], clipColumnMajor[3])));
Print("quarter-turn +X", Format3(quaternionRotated));
Print("quat round-trip", FormatQuat(matrixRoundTrip));

ExpectClose3(scaleBasis.Diagonal, new Vec3(2f, 3f, 4f), "Matrix diagonal helpers should expose scale terms.");
ExpectClose(scaleBasis.Determinant, 24f, "Diagonal matrix determinant should multiply the diagonal terms.");
ExpectClose3(outerProduct.Row1, new Vec3(8f, 10f, 12f), "Outer products should combine a column vector with a row vector.");
ExpectCloseMat4(projection, explicitProjection, "Default projection overloads should use OpenGL-style right-handed depth.");
ExpectClose3(model.Translation, new Vec3(1f, 2f, 0f), "Mat4.Translation should expose the transform translation column.");
ExpectClose3(Mat4.TransformPoint(model, Vec3.Zero), new Vec3(1f, 2f, 0f), "CreateWorld should place the local origin at the requested position.");
ExpectClose3(quaternionRotated, Vec3.UnitY, "Quaternion axis-angle rotation should match the matrix rotation convention.");
ExpectClose3(matrixRoundTrip * Vec3.UnitX, Vec3.UnitY, "Quat should round-trip through Mat4 rotation conversion.");

Section("Planes, Rays, And Frustums");

var floorPlane = Plane3.CreateFromPointNormal(new Vec3(0f, 1f, 0f), Vec3.UnitY);
var floatingPoint = new Vec3(2f, 4f, -3f);
var pointOnFloor = floorPlane.ProjectPoint(floatingPoint);
var reflectedPoint = floorPlane.ReflectPoint(floatingPoint);
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
var targetBox = new Box3(new Vec3(-0.5f, -0.5f, 2f), new Vec3(0.5f, 0.5f, 4f));
var targetSphere = new Sphere3(new Vec3(0f, 0f, 6f), 1f);
var clippedSphere = new Sphere3(new Vec3(1.25f, 0f, 6f), 0.5f);
var rejectedSphere = new Sphere3(new Vec3(2f, 0f, 6f), 0.25f);
var frustumHit = pickRay.TryIntersect(boxFrustum, out var frustumDistances);
Intervalf boxDistances;
var boxHit = pickRay.TryIntersect(targetBox, out boxDistances);
var sphereHit = pickRay.TryIntersect(targetSphere, out var sphereDistance);
var finiteFrustum = boxFrustum.TryCreateBoundingBox(out var frustumBounds);
var preciseBoxClass = boxFrustum.ClassifyPrecise(targetBox);

Print("floor plane", FormatPlane(floorPlane));
Print("projected point", Format3(pointOnFloor));
Print("reflected point", Format3(reflectedPoint));
Print("ray/frustum range", frustumHit ? FormatInterval(frustumDistances) : "<miss>");
Print("ray/box range", boxHit ? FormatInterval(boxDistances) : "<miss>");
Print("ray/sphere distance", sphereHit ? sphereDistance.ToString("0.###", CultureInfo.InvariantCulture) : "<miss>");
Print("frustum bounds", finiteFrustum ? frustumBounds.ToString("0.###", CultureInfo.InvariantCulture) : "<infinite>");
Print("precise box class", preciseBoxClass.ToString());
Print("sphere classes", $"{boxFrustum.Classify(targetSphere)}, {boxFrustum.Classify(clippedSphere)}, {boxFrustum.Classify(rejectedSphere)}");

ExpectClose(floorPlane.Evaluate(pointOnFloor), 0f, "Plane projection should land back on the plane.");
ExpectClose3(pointOnFloor, new Vec3(2f, 1f, -3f), "Plane ProjectPoint should move along the normal.");
ExpectClose3(reflectedPoint, new Vec3(2f, -2f, -3f), "Plane reflection should mirror across the plane.");
Expect(frustumHit && frustumDistances == new Intervalf(1f, 6f), "Ray/frustum intersections should return entry and exit distances.");
Expect(boxHit && boxDistances == new Intervalf(2f, 3f), "Ray/box slab tests should return the clipped distance interval.");
Expect(sphereHit, "A ray through the center should intersect the target sphere.");
ExpectClose(sphereDistance, 3.5f, "Ray/sphere intersections should return the nearest nonnegative distance.");
Expect(boxFrustum.Classify(targetSphere) == ContainmentKind.Contains, "A centered sphere should be fully inside the frustum.");
Expect(boxFrustum.Classify(clippedSphere) == ContainmentKind.Intersects, "A sphere crossing a side plane should intersect the frustum.");
Expect(boxFrustum.Classify(rejectedSphere) == ContainmentKind.Disjoint, "A sphere outside one frustum plane should be rejected.");
Expect(boxFrustum.HasFiniteCorners && finiteFrustum, "A closed six-plane frustum should expose finite corners and bounds.");
Expect(preciseBoxClass == ContainmentKind.Contains, "Precise frustum/box checks should preserve obvious containment.");

Section("Segments, Triangles, Capsules, And OBBs");

var verticalSegment = Segment3.Create(new Vec3(0f, -1f, 0f), new Vec3(0f, 1f, 0f));
var diagonalSegment = Segment3.Create(new Vec3(-2f, 0f, 0f), new Vec3(2f, 0f, 0f));
var segmentBox = Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(1f, 1f, 1f));
var triangle = Triangle3.Create(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
var trianglePoint = new Vec3(0.25f, 0.25f, 0f);
var triangleProbe = new Sphere3(new Vec3(0.25f, 0.25f, 0.2f), 0.25f);
var capsule = Capsule3.Create(verticalSegment, 0.25f);
var capsuleProbe = new Sphere3(new Vec3(0.4f, 0f, 0f), 0.2f);
var obb = Obb3.Transform(
    Box3.CreateFromCenterSize(Vec3.Zero, new Vec3(2f, 1f, 1f)),
    Mat4.CreateTranslation(new Vec3(0.25f, 0f, 0f)) * Mat4.CreateRotationZ(MathF.PI / 4f));
Span<Vec3> obbCorners = stackalloc Vec3[Obb3.CornerCount];
var copiedObbCorners = obb.TryCopyCornersTo(obbCorners);

var groundPlane = Plane3.CreateFromPointNormal(Vec3.Zero, Vec3.UnitY);
var segmentPlaneHit = verticalSegment.TryIntersect(groundPlane, out var segmentPlaneAmount);
Print("segment length", verticalSegment.Length.ToString("0.###", CultureInfo.InvariantCulture));
Print("segment plane amount", segmentPlaneHit ? segmentPlaneAmount.ToString("0.###", CultureInfo.InvariantCulture) : "<miss>");
Print("segment/box", diagonalSegment.Intersects(segmentBox).ToString());
Print("triangle normal", Format3(triangle.Normal));
Print("triangle barycentric", Format3(triangle.Barycentric(trianglePoint)));
Print("triangle/sphere", triangle.Intersects(triangleProbe).ToString());
Print("capsule/sphere", capsule.Intersects(capsuleProbe).ToString());
Print("capsule/box", capsule.Intersects(segmentBox).ToString());
Print("obb contains origin", obb.Contains(Vec3.Zero).ToString());
Print("obb first corner", copiedObbCorners ? Format3(obbCorners[0]) : "<empty>");

Expect(segmentPlaneHit, "A vertical segment crossing the floor should hit the plane.");
ExpectClose(segmentPlaneAmount, 0.5f, "Segment plane intersections should report normalized segment amount.");
Expect(diagonalSegment.Intersects(segmentBox), "Segment/box intersections should count crossing the box.");
ExpectClose3(triangle.Normal, Vec3.UnitZ, "Triangle normals should be derived from vertex winding.");
Expect(triangle.Contains(trianglePoint), "Triangle containment should accept points inside the finite face.");
Expect(triangle.Intersects(triangleProbe), "Triangle/sphere intersection should use distance to the finite triangle.");
Expect(capsule.Contains(new Vec3(0.1f, 0f, 0f)), "Capsule containment should measure distance to its axis segment.");
Expect(capsule.Intersects(capsuleProbe), "Capsule/sphere intersection should combine both radii.");
Expect(capsule.Intersects(segmentBox), "Capsule/box intersection should account for swept radius around the axis.");
Expect(obb.Contains(Vec3.Zero), "Transformed OBB containment should use its oriented local space.");
Expect(obb.Intersects(segmentBox), "OBB/AABB intersection should use the oriented-box SAT path.");
Expect(copiedObbCorners, "OBBs should expose finite world-space corners.");

Section("Viewports And Picking");

var viewport = new Viewport(Box2.CreateFromCenterSize(new Vec2(960f, 540f), new Vec2(1920f, 1080f)));
var worldFromClip = clipFromWorld.Inverted;
var projectedOrigin = viewport.Project(Vec3.Zero, clipFromWorld);
var unprojectedOrigin = viewport.UnProject(projectedOrigin, worldFromClip);
var centerPickRay = viewport.CreatePickRay(viewport.Center, worldFromClip);
var pickMatrix = viewport.CreatePickMatrix(Box2.CreateFromCenterSize(viewport.Center, new Vec2(320f, 180f)));
Span<float> viewportComponents = stackalloc float[Viewport.ComponentCount];
Span<char> viewportText = stackalloc char[256];
viewport.CopyTo(viewportComponents);
var viewportTextOk = viewport.TryFormat(viewportText, out var viewportCharsWritten, "0.###", CultureInfo.InvariantCulture);

Print("viewport", viewport.ToString("0.###", CultureInfo.InvariantCulture));
Print("projected origin", Format3(projectedOrigin));
Print("unprojected origin", Format3(unprojectedOrigin));
Print("center pick ray", FormatRay(centerPickRay));
Print("pick matrix trace", pickMatrix.Trace.ToString("0.###", CultureInfo.InvariantCulture));
Print("viewport components", Format4(new Vec4(viewportComponents[0], viewportComponents[1], viewportComponents[2], viewportComponents[3])));
Print("viewport span text", viewportTextOk ? new string(viewportText[..viewportCharsWritten]) : "<too small>");

ExpectClose3(unprojectedOrigin, Vec3.Zero, "Viewport project/unproject should round-trip world points.", 0.0005f);
ExpectClose3(centerPickRay.Direction, new Vec3(0f, 0f, -1f), "A centered camera pick ray should point forward.", 0.0005f);
Expect(pickMatrix != Mat4.Identity, "A smaller selection rectangle should produce a non-identity pick matrix.");
ExpectClose(viewportComponents[0], 0f, "Viewport span layout should start with bounds min X.");
ExpectClose(viewportComponents[1], 0f, "Viewport span layout should start with bounds min Y.");
Expect(viewportTextOk, "Viewport TryFormat should write into caller-owned spans.");

Console.WriteLine();
Console.WriteLine("All maths checks passed.");

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

static void Print(string label, string value) =>
    Console.WriteLine($"{label,-28} {value}");

static string Format2(Vec2 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static string Format3(Vec3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static string Format4(Vec4 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static string FormatQuat(Quat value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static string FormatInterval(Intervalf value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static string FormatSphere(Sphere3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static string FormatRay(Ray3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static string FormatPlane(Plane3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

static void Expect(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static void ExpectClose(float actual, float expected, string message, float tolerance = 0.0001f) =>
    Expect(MathF.Abs(actual - expected) <= tolerance, message);

static void ExpectClose2(Vec2 actual, Vec2 expected, string message, float tolerance = 0.0001f)
{
    ExpectClose(actual.X, expected.X, message, tolerance);
    ExpectClose(actual.Y, expected.Y, message, tolerance);
}

static void ExpectClose3(Vec3 actual, Vec3 expected, string message, float tolerance = 0.0001f)
{
    ExpectClose(actual.X, expected.X, message, tolerance);
    ExpectClose(actual.Y, expected.Y, message, tolerance);
    ExpectClose(actual.Z, expected.Z, message, tolerance);
}

static void ExpectCloseMat4(Mat4 actual, Mat4 expected, string message, float tolerance = 0.0001f)
{
    for (var column = 0; column < Mat4.ColumnCount; column++)
    {
        for (var row = 0; row < Mat4.RowCount; row++)
            ExpectClose(actual[column, row], expected[column, row], message, tolerance);
    }
}
