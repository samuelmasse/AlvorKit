namespace AlvorKit.Script.NewGame;

/// <summary>Copies and renames the starter AlvorKit game project into a new repository.</summary>
internal sealed class NewGameGenerator
{
    /// <summary>UTF-8 encoding used for generated repository text files.</summary>
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Concrete starter project used as the generation source.</summary>
    private readonly NewGameStarterProject starterProject = new();

    /// <summary>Creates the output repository from the concrete starter files.</summary>
    public NewGameResult Generate(NewGameOptions options)
    {
        EnsureAlvorKitRoot(options.AlvorKitRoot);
        EnsureOutputAvailable(options.OutputPath);
        EnsureNotInsideAlvorKit(options.OutputPath, options.AlvorKitRoot);

        var count = 0;
        foreach (var file in starterProject.ReadFiles())
        {
            WriteFile(options.OutputPath, file, options);
            count++;
        }

        WriteText(
            options.OutputPath,
            NewGameStarterProject.SolutionPath(options),
            starterProject.RenderSolution(options));
        count++;

        return new(options.OutputPath, count);
    }

    private static void EnsureAlvorKitRoot(string root)
    {
        if (!File.Exists(Path.Combine(root, ProjectRoot.SolutionFileName)))
            throw new DirectoryNotFoundException($"AlvorKit repository root not found at '{root}'.");
    }

    /// <summary>Rejects output paths that would overwrite an existing repository.</summary>
    private static void EnsureOutputAvailable(string outputPath)
    {
        if (Directory.Exists(outputPath) && Directory.EnumerateFileSystemEntries(outputPath).Any())
            throw new InvalidOperationException($"Output directory '{outputPath}' already exists and is not empty.");
    }

    /// <summary>Rejects scaffold output inside AlvorKit so games stay sibling repositories.</summary>
    private static void EnsureNotInsideAlvorKit(string outputPath, string alvorKitRoot)
    {
        var relative = Path.GetRelativePath(alvorKitRoot, outputPath);
        if (!relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative))
            throw new InvalidOperationException("Output directory must not be inside the AlvorKit repository.");
    }

    /// <summary>Writes one rendered starter file to the target repository.</summary>
    private void WriteFile(string outputRoot, NewGameSourceFile file, NewGameOptions options)
    {
        var relativePath = starterProject.RenderPath(file.RelativePath, options);
        var path = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(path) ?? outputRoot;
        Directory.CreateDirectory(directory);
        if (file.IsText)
            File.WriteAllText(path, starterProject.RenderText(file, options), Utf8NoBom);
        else
            File.WriteAllBytes(path, file.Bytes);
    }

    /// <summary>Writes generated text content under the target repository.</summary>
    private static void WriteText(string outputRoot, string relativePath, string content)
    {
        var path = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(path) ?? outputRoot;
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, content, Utf8NoBom);
    }
}
