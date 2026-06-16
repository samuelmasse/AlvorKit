namespace AlvorKit.Testing;

/// <summary>Creates an isolated temporary directory for filesystem-oriented tests.</summary>
public sealed class TempWorkspace : IDisposable
{
    private const string DefaultPrefix = "AlvorKit.Testing";

    private TempWorkspace(string root)
    {
        Root = root;
        Directory.CreateDirectory(root);
    }

    /// <summary>Absolute path to the temporary workspace root.</summary>
    public string Root { get; }

    /// <summary>Creates a fresh workspace under the system temporary directory.</summary>
    public static TempWorkspace Create(string prefix = DefaultPrefix) =>
        new(ResolvePhysicalPath(Path.Combine(Path.GetTempPath(), prefix, Guid.NewGuid().ToString("N"))));

    /// <summary>Resolves symlinked path prefixes so test expectations match paths returned by the runtime.</summary>
    private static string ResolvePhysicalPath(string path)
    {
        var pendingParts = new Stack<string>();
        var existingPath = Path.GetFullPath(path);
        while (!Directory.Exists(existingPath))
        {
            pendingParts.Push(Path.GetFileName(existingPath));
            existingPath = Path.GetDirectoryName(existingPath) ?? throw new DirectoryNotFoundException(path);
        }

        var physicalPath = ResolveExistingPhysicalPath(existingPath);
        while (pendingParts.TryPop(out var part))
            physicalPath = Path.Combine(physicalPath, part);

        return physicalPath;
    }

    /// <summary>Resolves symlinks in each component of an existing directory path.</summary>
    private static string ResolveExistingPhysicalPath(string path)
    {
        var root = Path.GetPathRoot(path) ?? "";
        var physicalPath = root;
        var relativePath = Path.GetRelativePath(root, Path.GetFullPath(path));
        if (relativePath == ".")
            return physicalPath;

        foreach (var part in relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            if (part.Length == 0)
                continue;

            var nextPath = Path.Combine(physicalPath, part);
            var linkTarget = new DirectoryInfo(nextPath).ResolveLinkTarget(returnFinalTarget: true);
            physicalPath = linkTarget?.FullName ?? nextPath;
        }

        return physicalPath;
    }

    /// <summary>Builds an absolute path under the workspace and creates its parent directory.</summary>
    public string PathFor(params string[] parts)
    {
        var path = parts.Aggregate(Root, Path.Combine);
        var directory = Path.GetDirectoryName(path);

        if (directory is not null)
            Directory.CreateDirectory(directory);

        return path;
    }

    /// <summary>Creates a directory under the workspace and returns its absolute path.</summary>
    public string CreateDirectory(params string[] parts)
    {
        var path = parts.Aggregate(Root, Path.Combine);
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>Writes a text file under the workspace and returns its absolute path.</summary>
    public string Write(string relativePath, string content)
    {
        var path = PathFor(relativePath.Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>Writes a text file under the workspace and returns its absolute path.</summary>
    public string WriteFile(string relativePath, string content) =>
        Write(relativePath, content);

    /// <summary>Writes an SDK-style project file with optional project references.</summary>
    public string WriteProject(
        string root,
        string name,
        IReadOnlyList<string> references,
        bool isTestProject = true)
    {
        var projectPath = PathFor(root, name, name + ".csproj");
        var properties = new XElement("PropertyGroup");
        if (!isTestProject)
            properties.Add(new XElement("IsTestProject", "false"));

        var document = new XDocument(
            new XElement("Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                properties,
                new XElement("ItemGroup",
                    references.Select(reference =>
                        new XElement(
                            "ProjectReference",
                            new XAttribute("Include", Path.GetRelativePath(Path.GetDirectoryName(projectPath)!, reference)))))));

        document.Save(projectPath);
        return projectPath;
    }

    /// <summary>Deletes the workspace directory when the test ends.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
