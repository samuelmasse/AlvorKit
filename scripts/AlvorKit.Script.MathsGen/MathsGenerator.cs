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

    /// <summary>The project subdirectory that contains generated box source files.</summary>
    public const string BoxDirectoryName = "Box";

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
        var boxDirectory = Path.Combine(projectDirectory, BoxDirectoryName);
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        RecreateDirectory(projectDirectory);
        File.WriteAllText(ProjectFile(projectDirectory), ProjectSource(packageVersion), encoding);
        Directory.CreateDirectory(vecDirectory);
        Directory.CreateDirectory(matDirectory);
        Directory.CreateDirectory(quatDirectory);
        Directory.CreateDirectory(planeDirectory);
        Directory.CreateDirectory(frustumDirectory);
        Directory.CreateDirectory(sphereDirectory);
        Directory.CreateDirectory(intervalDirectory);
        Directory.CreateDirectory(rayDirectory);
        Directory.CreateDirectory(boxDirectory);
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
        {
            var source = MatrixFileEmitter.Emit(matrix);
            File.WriteAllText(Path.Combine(matDirectory, $"{matrix.TypeName}.g.cs"), source, encoding);
        }

        foreach (var quaternion in QuaternionCatalog.Quaternions)
        {
            var source = QuaternionFileEmitter.Emit(quaternion);
            File.WriteAllText(Path.Combine(quatDirectory, $"{quaternion.TypeName}.g.cs"), source, encoding);
        }

        foreach (var plane in PlaneCatalog.Planes)
        {
            var source = PlaneFileEmitter.Emit(plane);
            File.WriteAllText(Path.Combine(planeDirectory, $"{plane.TypeName}.g.cs"), source, encoding);
        }

        foreach (var frustum in FrustumCatalog.Frustums)
        {
            var source = FrustumFileEmitter.Emit(frustum);
            File.WriteAllText(Path.Combine(frustumDirectory, $"{frustum.TypeName}.g.cs"), source, encoding);
        }

        foreach (var sphere in SphereCatalog.Spheres)
        {
            var source = SphereFileEmitter.Emit(sphere);
            File.WriteAllText(Path.Combine(sphereDirectory, $"{sphere.TypeName}.g.cs"), source, encoding);
        }

        foreach (var interval in IntervalCatalog.Intervals)
        {
            var source = IntervalFileEmitter.Emit(interval);
            File.WriteAllText(Path.Combine(intervalDirectory, $"{interval.TypeName}.g.cs"), source, encoding);
        }

        foreach (var ray in RayCatalog.Rays)
        {
            var source = RayFileEmitter.Emit(ray);
            File.WriteAllText(Path.Combine(rayDirectory, $"{ray.TypeName}.g.cs"), source, encoding);
        }

        foreach (var box in BoxCatalog.Boxes)
        {
            var source = BoxFileEmitter.Emit(box);
            File.WriteAllText(Path.Combine(boxDirectory, $"{box.TypeName}.g.cs"), source, encoding);
        }
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
