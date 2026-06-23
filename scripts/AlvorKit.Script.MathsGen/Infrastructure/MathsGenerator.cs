namespace AlvorKit.Script.MathsGen;

/// <summary>Coordinates vector source generation and filesystem output.</summary>
[ExcludeFromCodeCoverage(Justification = "Coordinates full generated-project filesystem output; focused emitter tests cover generation logic.")]
internal static class MathsGenerator
{
    /// <summary>The generated primitives project and package name.</summary>
    public const string PrimitivesProjectName = "AlvorKit.Maths.Primitives";

    /// <summary>The project subdirectory that contains generated vector source files.</summary>
    public const string VecDirectoryName = "Vec";

    /// <summary>The project subdirectory that contains generated matrix source files.</summary>
    public const string MatDirectoryName = "Mat";

    /// <summary>The project subdirectory that contains generated quaternion source files.</summary>
    public const string QuatDirectoryName = "Quat";

    /// <summary>The project subdirectory that contains generated plane source files.</summary>
    public const string PlaneDirectoryName = "Plane";

    /// <summary>The project subdirectory that contains generated frustum source files.</summary>
    public const string FrustumDirectoryName = "Frustum";

    /// <summary>The project subdirectory that contains generated sphere source files.</summary>
    public const string SphereDirectoryName = "Sphere";

    /// <summary>The project subdirectory that contains generated interval source files.</summary>
    public const string IntervalDirectoryName = "Interval";

    /// <summary>The project subdirectory that contains generated ray source files.</summary>
    public const string RayDirectoryName = "Ray";

    /// <summary>The project subdirectory that contains generated segment source files.</summary>
    public const string SegmentDirectoryName = "Segment";

    /// <summary>The project subdirectory that contains generated capsule source files.</summary>
    public const string CapsuleDirectoryName = "Capsule";

    /// <summary>The project subdirectory that contains generated triangle source files.</summary>
    public const string TriangleDirectoryName = "Triangle";

    /// <summary>The project subdirectory that contains generated oriented bounding box source files.</summary>
    public const string ObbDirectoryName = "Obb";

    /// <summary>The project subdirectory that contains generated box source files.</summary>
    public const string BoxDirectoryName = "Box";

    /// <summary>The project subdirectory that contains generated quad source files.</summary>
    public const string QuadDirectoryName = "Quad";

    /// <summary>The project subdirectory that contains generated viewport source files.</summary>
    public const string ViewportDirectoryName = "Viewport";

    /// <summary>Regenerates the generated primitives project under <paramref name="outputRoot"/>.</summary>
    public static void GenerateTo(string outputRoot, string packageVersion)
    {
        if (string.IsNullOrWhiteSpace(outputRoot))
            throw new ArgumentException("Output root must not be blank.", nameof(outputRoot));
        if (string.IsNullOrWhiteSpace(packageVersion))
            throw new ArgumentException("Package version must not be blank.", nameof(packageVersion));

        var projectDirectory = Path.Combine(outputRoot, PrimitivesProjectName);
        var vecDirectory = Path.Combine(projectDirectory, VecDirectoryName);
        var matDirectory = Path.Combine(projectDirectory, MatDirectoryName);
        var quatDirectory = Path.Combine(projectDirectory, QuatDirectoryName);
        var planeDirectory = Path.Combine(projectDirectory, PlaneDirectoryName);
        var frustumDirectory = Path.Combine(projectDirectory, FrustumDirectoryName);
        var sphereDirectory = Path.Combine(projectDirectory, SphereDirectoryName);
        var intervalDirectory = Path.Combine(projectDirectory, IntervalDirectoryName);
        var rayDirectory = Path.Combine(projectDirectory, RayDirectoryName);
        var segmentDirectory = Path.Combine(projectDirectory, SegmentDirectoryName);
        var capsuleDirectory = Path.Combine(projectDirectory, CapsuleDirectoryName);
        var triangleDirectory = Path.Combine(projectDirectory, TriangleDirectoryName);
        var obbDirectory = Path.Combine(projectDirectory, ObbDirectoryName);
        var boxDirectory = Path.Combine(projectDirectory, BoxDirectoryName);
        var quadDirectory = Path.Combine(projectDirectory, QuadDirectoryName);
        var viewportDirectory = Path.Combine(projectDirectory, ViewportDirectoryName);
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        RecreateDirectory(projectDirectory);
        File.WriteAllText(ProjectFile(projectDirectory), ProjectSource(packageVersion), encoding);
        string[] directories =
        [
            vecDirectory, matDirectory, quatDirectory, planeDirectory, frustumDirectory,
            sphereDirectory, intervalDirectory, rayDirectory, segmentDirectory, capsuleDirectory, triangleDirectory,
            obbDirectory, boxDirectory, quadDirectory, viewportDirectory,
        ];
        foreach (var directory in directories)
            Directory.CreateDirectory(directory);

        foreach (var (fileName, source) in VectorInterfaceFileEmitter.EmitAll())
            File.WriteAllText(Path.Combine(vecDirectory, fileName), source, encoding);

        foreach (var vector in VectorCatalog.Vectors)
        {
            var source = VectorFileEmitter.Emit(vector);
            var path = Path.Combine(vecDirectory, $"{vector.TypeName}.g.cs");
            File.WriteAllText(path, source, encoding);
            var swizzles = SwizzleFileEmitter.Emit(vector);
            File.WriteAllText(Path.Combine(vecDirectory, $"{vector.TypeName}.Swizzles.g.cs"), swizzles, encoding);
        }

        foreach (var matrix in MatrixCatalog.Matrices)
            File.WriteAllText(Path.Combine(matDirectory, $"{matrix.TypeName}.g.cs"), MatrixFileEmitter.Emit(matrix), encoding);

        foreach (var quaternion in QuaternionCatalog.Quaternions)
            File.WriteAllText(Path.Combine(quatDirectory, $"{quaternion.TypeName}.g.cs"), QuaternionFileEmitter.Emit(quaternion), encoding);

        foreach (var plane in PlaneCatalog.Planes)
            File.WriteAllText(Path.Combine(planeDirectory, $"{plane.TypeName}.g.cs"), PlaneFileEmitter.Emit(plane), encoding);

        foreach (var frustum in FrustumCatalog.Frustums)
            File.WriteAllText(Path.Combine(frustumDirectory, $"{frustum.TypeName}.g.cs"), FrustumFileEmitter.Emit(frustum), encoding);

        foreach (var sphere in SphereCatalog.Spheres)
            File.WriteAllText(Path.Combine(sphereDirectory, $"{sphere.TypeName}.g.cs"), SphereFileEmitter.Emit(sphere), encoding);

        foreach (var interval in IntervalCatalog.Intervals)
            File.WriteAllText(Path.Combine(intervalDirectory, $"{interval.TypeName}.g.cs"), IntervalFileEmitter.Emit(interval), encoding);

        foreach (var ray in RayCatalog.Rays)
            File.WriteAllText(Path.Combine(rayDirectory, $"{ray.TypeName}.g.cs"), RayFileEmitter.Emit(ray), encoding);

        foreach (var segment in SegmentCatalog.Segments)
            File.WriteAllText(Path.Combine(segmentDirectory, $"{segment.TypeName}.g.cs"), SegmentFileEmitter.Emit(segment), encoding);

        foreach (var capsule in CapsuleCatalog.Capsules)
            File.WriteAllText(Path.Combine(capsuleDirectory, $"{capsule.TypeName}.g.cs"), CapsuleFileEmitter.Emit(capsule), encoding);

        foreach (var triangle in TriangleCatalog.Triangles)
            File.WriteAllText(Path.Combine(triangleDirectory, $"{triangle.TypeName}.g.cs"), TriangleFileEmitter.Emit(triangle), encoding);

        foreach (var obb in ObbCatalog.Obbs)
            File.WriteAllText(Path.Combine(obbDirectory, $"{obb.TypeName}.g.cs"), ObbFileEmitter.Emit(obb), encoding);

        foreach (var box in BoxCatalog.Boxes)
            File.WriteAllText(Path.Combine(boxDirectory, $"{box.TypeName}.g.cs"), BoxFileEmitter.Emit(box), encoding);

        foreach (var quad in QuadCatalog.Quads)
            File.WriteAllText(Path.Combine(quadDirectory, $"{quad.TypeName}.g.cs"), QuadFileEmitter.Emit(quad), encoding);

        foreach (var viewport in ViewportCatalog.Viewports)
            File.WriteAllText(Path.Combine(viewportDirectory, $"{viewport.TypeName}.g.cs"), ViewportFileEmitter.Emit(viewport), encoding);
    }

    /// <summary>Returns the generated project file path for <paramref name="projectDirectory"/>.</summary>
    private static string ProjectFile(string projectDirectory) =>
        Path.Combine(projectDirectory, $"{PrimitivesProjectName}.csproj");

    /// <summary>Returns generated project source for the pinned primitives package version.</summary>
    private static string ProjectSource(string packageVersion) =>
        MathsTemplate.Render("maths-primitives-project.csproj.tmpl", ("Version", packageVersion));

    /// <summary>Deletes stale generated files before creating the output directory again.</summary>
    private static void RecreateDirectory(string projectDirectory)
    {
        if (Directory.Exists(projectDirectory))
            Directory.Delete(projectDirectory, recursive: true);

        Directory.CreateDirectory(projectDirectory);
    }
}
