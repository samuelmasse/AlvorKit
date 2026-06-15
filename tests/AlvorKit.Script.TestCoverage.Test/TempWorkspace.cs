using System.Xml.Linq;

namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Temporary directory helper for filesystem-oriented coverage tests.</summary>
internal sealed class TempWorkspace : IDisposable
{
    /// <summary>Creates the temporary workspace directory.</summary>
    private TempWorkspace(string root)
    {
        Root = root;
        Directory.CreateDirectory(root);
    }

    /// <summary>Absolute path to the temporary workspace root.</summary>
    public string Root { get; }

    /// <summary>Creates a fresh temporary workspace.</summary>
    public static TempWorkspace Create() =>
        new(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "AlvorKit.TestCoverage." + Guid.NewGuid().ToString("N")));

    /// <summary>Builds an absolute path under the workspace and creates its parent directory.</summary>
    public string PathFor(params string[] parts)
    {
        var path = parts.Aggregate(Root, System.IO.Path.Combine);
        var directory = System.IO.Path.GetDirectoryName(path);

        if (directory is not null)
            Directory.CreateDirectory(directory);

        return path;
    }

    /// <summary>Writes a file under the workspace and returns its absolute path.</summary>
    public string Write(string name, string content)
    {
        var path = PathFor(name);
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>Writes an SDK-style project file with optional project references.</summary>
    public string WriteProject(string root, string name, IReadOnlyList<string> references)
    {
        var projectPath = PathFor(root, name, name + ".csproj");
        var document = new XDocument(
            new XElement("Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("ItemGroup",
                    references.Select(reference =>
                        new XElement(
                            "ProjectReference",
                            new XAttribute("Include", System.IO.Path.GetRelativePath(System.IO.Path.GetDirectoryName(projectPath)!, reference)))))));

        document.Save(projectPath);
        return projectPath;
    }

    /// <summary>Deletes the temporary workspace directory.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
