namespace AlvorKit;

/// <summary>Discovers authored projects and names generated repository solutions.</summary>
public static class RepositoryProjects
{
    /// <summary>Repository areas that own ordinary managed build projects.</summary>
    public static IReadOnlyList<string> Areas => ["src", "lib", "scripts", "tests", "demos", "bench"];

    /// <summary>Returns existing tracked and non-ignored untracked projects in the repository's source areas.</summary>
    public static IReadOnlyList<string> Discover(string root)
    {
        var fullRoot = Path.GetFullPath(root);
        var marker = Path.Combine(fullRoot, ".git");

        if (!Directory.Exists(marker) && !File.Exists(marker))
        {
            throw new InvalidOperationException(
                $"Project discovery requires a Git checkout root: {fullRoot}. Run git init for a new repository.");
        }

        var output = RepositoryGit.Read(fullRoot, "ls-files", "--cached", "--others", "--exclude-standard", "-z", "--", "*.csproj");
        return output.Split('\0', StringSplitOptions.RemoveEmptyEntries)
            .Where(IsSourceProject)
            .Select(path => Path.GetFullPath(path, fullRoot))
            .Where(File.Exists)
            .Where(path => (File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal).ToArray();
    }

    /// <summary>Names the one generated solution after its checkout directory.</summary>
    public static string SolutionFileName(string root) => new DirectoryInfo(Path.GetFullPath(root)).Name + ".slnx";

    /// <summary>Returns the generated solution path without using it as a repository marker.</summary>
    public static string SolutionPath(string root) => Path.Combine(Path.GetFullPath(root), SolutionFileName(root));

    /// <summary>Requires generation before a command explicitly requests the repository solution.</summary>
    public static string RequireSolutionFileName(string root)
    {
        var solutions = Directory.GetFiles(root, "*.slnx", SearchOption.TopDirectoryOnly);
        return solutions.Length switch
        {
            1 => Path.GetFileName(solutions[0]),
            0 => throw new InvalidOperationException(
                $"Generate {SolutionFileName(root)} with AlvorKit.Script.Solution before using this command."),
            _ => throw new InvalidOperationException($"Expected one generated solution in '{root}', found {solutions.Length}."),
        };
    }

    /// <summary>Gets the shared namespace prefix from authored project names, independently of checkout naming.</summary>
    public static string? Namespace(string root)
    {
        var names = Discover(root)
            .Select(path => Path.GetFileNameWithoutExtension(path).Split('.')[0])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return names.Length switch
        {
            0 => null,
            1 => names[0],
            _ => throw new InvalidOperationException($"Projects under '{root}' do not share a repository namespace prefix."),
        };
    }

    /// <summary>Keeps authored source areas separate from tracked templates and native build inputs.</summary>
    private static bool IsSourceProject(string path)
    {
        var separator = path.IndexOf('/');
        return separator < 0 || Areas.Contains(path[..separator], StringComparer.Ordinal);
    }
}
