namespace AlvorKit;

/// <summary>Builds and snapshots one exact Debug project for a Source Update session.</summary>
[ExcludeFromCodeCoverage(Justification = "Runs the selected SDK and copies a complete external build output.")]
internal static class AlvorSenseEditableProject
{
    internal static AlvorSenseSessionManifest Prepare(
        string sessionDir,
        AlvorSenseSessionManifest manifest)
    {
        var workingDirectory = Path.GetFullPath(
            manifest.WorkingDirectory);
        var project = Path.GetFullPath(
            manifest.EditableProject
                ?? throw new InvalidOperationException("Editable project path is missing."),
            workingDirectory);
        if (!File.Exists(project))
            throw new FileNotFoundException($"Editable project was not found: {project}", project);

        Build(project, workingDirectory);
        var targetPath = QueryTargetPath(project, workingDirectory);
        var targetDirectory = Path.GetDirectoryName(targetPath)
            ?? throw new InvalidOperationException("Editable target path has no output directory.");
        var artifactDirectory = Path.GetFullPath(AlvorSensePaths.EditableArtifacts(sessionDir));
        CopyDirectory(targetDirectory, artifactDirectory);

        var assemblyPath = Path.Combine(artifactDirectory, Path.GetFileName(targetPath));
        var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
        if (!File.Exists(assemblyPath) || !File.Exists(pdbPath))
            throw new InvalidOperationException("Editable build did not produce the expected assembly and portable PDB.");

        var assemblyHash = HashFile(assemblyPath);
        var pdbHash = HashFile(pdbPath);
        var sdkVersion = Run("dotnet", ["--version"], workingDirectory).Trim();
        var mvid = ReadMvid(assemblyPath);
        var codeViewPath = ReadCodeViewPath(assemblyPath);
        var projectIdentity = ProjectIdentity(project, sdkVersion, assemblyHash, pdbHash);
        var launch = new AlvorSenseEditableLaunchManifest(
            1,
            project,
            assemblyPath,
            pdbPath,
            assemblyHash,
            pdbHash,
            mvid,
            projectIdentity,
            sdkVersion,
            codeViewPath);
        var launchPath = Path.GetFullPath(AlvorSensePaths.EditableLaunchManifest(sessionDir));
        AlvorSenseJson.Save(launchPath, launch);
        return manifest with
        {
            Project = null,
            Assembly = assemblyPath,
            EditableProject = project,
            WorkingDirectory = workingDirectory,
            EditableLaunchManifestPath = launchPath
        };
    }

    private static void Build(string project, string workingDirectory)
    {
        _ = Run(
            "dotnet",
            [
                "build",
                project,
                "--configuration",
                "Debug",
                "--property:Optimize=false",
                "--property:DebugSymbols=true",
                "--property:DebugType=portable",
                "--property:PublishTrimmed=false",
                "--property:PublishReadyToRun=false",
                "--property:PublishSingleFile=false",
                "--property:UseSharedCompilation=false"
            ],
            workingDirectory);
    }

    private static string QueryTargetPath(string project, string workingDirectory) =>
        Run(
            "dotnet",
            [
                "msbuild",
                project,
                "-getProperty:TargetPath",
                "-property:Configuration=Debug",
                "-property:Optimize=false",
                "-nologo"
            ],
            workingDirectory).Trim();

    private static string Run(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory)
    {
        var start = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start)
            ?? throw new InvalidOperationException($"Failed to start {fileName}.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var stdout = output.GetAwaiter().GetResult();
        var stderr = error.GetAwaiter().GetResult();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{fileName} exited with code {process.ExitCode}: {stderr}{stdout}");
        }
        return stdout;
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(
                destination,
                Path.GetRelativePath(source, directory)));
        }
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(
                file,
                Path.Combine(destination, Path.GetRelativePath(source, file)));
        }
    }

    private static string ReadMvid(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        return metadata.GetGuid(metadata.GetModuleDefinition().Mvid).ToString("N");
    }

    private static string ReadCodeViewPath(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var pe = new PEReader(stream);
        var entry = pe.ReadDebugDirectory()
            .Single(static entry => entry.Type == DebugDirectoryEntryType.CodeView);
        return pe.ReadCodeViewDebugDirectoryData(entry).Path;
    }

    private static string ProjectIdentity(
        string project,
        string sdkVersion,
        string assemblyHash,
        string pdbHash)
    {
        var projectDirectory = Path.GetDirectoryName(project)!;
        var sources = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
                !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => $"{Path.GetRelativePath(projectDirectory, path)}:{HashFile(path)}");
        var identity = string.Join(
            "\n",
            new[]
            {
                project,
                sdkVersion,
                HashFile(project),
                assemblyHash,
                pdbHash
            }.Concat(sources));
        return Hash(Encoding.UTF8.GetBytes(identity));
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Hash(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
