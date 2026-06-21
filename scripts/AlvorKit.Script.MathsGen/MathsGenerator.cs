namespace AlvorKit.Script.MathsGen;

/// <summary>Coordinates vector source generation and filesystem output.</summary>
internal static class MathsGenerator
{
    /// <summary>The generated primitives project and package name.</summary>
    public const string PrimitivesProjectName = "AlvorKit.Maths.Primitives";

    /// <summary>The project subdirectory that contains generated vector source files.</summary>
    public const string VecDirectoryName = "Vec";

    /// <summary>The project subdirectory that contains generated matrix source files.</summary>
    public const string MatDirectoryName = "Mat";

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
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        RecreateDirectory(projectDirectory);
        File.WriteAllText(ProjectFile(projectDirectory), ProjectSource(packageVersion), encoding);
        File.WriteAllText(Path.Combine(projectDirectory, "ScalarMath.g.cs"), ScalarMathSource(), encoding);
        Directory.CreateDirectory(vecDirectory);
        Directory.CreateDirectory(matDirectory);
        foreach (var (fileName, source) in VectorInterfaceFileEmitter.EmitAll())
            File.WriteAllText(Path.Combine(vecDirectory, fileName), source, encoding);
        foreach (var (fileName, source) in MatrixInterfaceFileEmitter.EmitAll())
            File.WriteAllText(Path.Combine(matDirectory, fileName), source, encoding);

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
    }

    /// <summary>Returns the generated project file path for <paramref name="projectDirectory"/>.</summary>
    private static string ProjectFile(string projectDirectory) =>
        Path.Combine(projectDirectory, $"{PrimitivesProjectName}.csproj");

    /// <summary>Returns generated project source for the pinned primitives package version.</summary>
    private static string ProjectSource(string packageVersion) =>
        MathsTemplate.Render("maths-primitives-project.csproj.tmpl", ("Version", packageVersion));

    /// <summary>Returns generated scalar math helper source.</summary>
    private static string ScalarMathSource() =>
        MathsTemplate.Render("scalar-math.cs.tmpl");

    /// <summary>Deletes stale generated files before creating the output directory again.</summary>
    private static void RecreateDirectory(string projectDirectory)
    {
        if (Directory.Exists(projectDirectory))
            Directory.Delete(projectDirectory, recursive: true);

        Directory.CreateDirectory(projectDirectory);
    }
}
