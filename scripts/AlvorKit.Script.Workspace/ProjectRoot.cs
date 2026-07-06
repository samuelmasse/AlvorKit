namespace AlvorKit.Script.Workspace;

/// <summary>Finds the repository root for script tools that can run from build, test, or command-line directories.</summary>
public static class ProjectRoot
{
    /// <summary>The solution file that marks the repository root.</summary>
    public const string SolutionFileName = "AlvorKit.slnx";

    /// <summary>Finds the nearest repository root above a path.</summary>
    /// <param name="startPath">A file or directory path inside the repository.</param>
    public static string FindFrom(string startPath) =>
        FindFrom(startPath, requireResDirectory: false);

    /// <summary>Finds the nearest repository root above a path and optionally requires the root <c>res</c> directory.</summary>
    /// <param name="startPath">A file or directory path inside the repository.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindFrom(string startPath, bool requireResDirectory) =>
        SolutionRoot.FindFrom(startPath, SolutionFileName, requireResDirectory);

    /// <summary>Finds the repository root from likely process directories and an optional assembly anchor.</summary>
    /// <param name="anchor">A type from the calling assembly, used to add its output directory as a search candidate.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    public static string FindFromCurrentProcess(Type? anchor = null, bool requireResDirectory = false) =>
        SolutionRoot.FindFromCurrentProcess(SolutionFileName, anchor, requireResDirectory);

    /// <summary>Finds the repository root from ordered candidate directories.</summary>
    /// <param name="startPaths">Candidate paths to search, in priority order.</param>
    /// <param name="requireResDirectory">Whether the discovered root must contain a <c>res</c> directory.</param>
    internal static string FindFromCandidates(IEnumerable<string?> startPaths, bool requireResDirectory = false)
        => SolutionRoot.FindFromCandidates(startPaths, SolutionFileName, requireResDirectory);

    /// <summary>Returns the root <c>res</c> directory for the current process.</summary>
    /// <param name="anchor">A type from the calling assembly, used to add its output directory as a search candidate.</param>
    public static string ResDirectory(Type? anchor = null) =>
        Path.Combine(FindFromCurrentProcess(anchor, requireResDirectory: true), "res");
}
