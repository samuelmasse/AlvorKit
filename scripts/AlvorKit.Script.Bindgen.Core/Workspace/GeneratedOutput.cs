namespace AlvorKit.Script.Bindgen;

/// <summary>Absolute directories used for one generated API/backend project pair.</summary>
/// <param name="Root">Directory that receives shared generated project files such as Directory.Build.props.</param>
/// <param name="ApiDirectory">Generated public API project directory.</param>
/// <param name="BackendDirectory">Generated backend project directory.</param>
public sealed record GeneratedProjectLayout(string Root, string ApiDirectory, string BackendDirectory);

/// <summary>Common filesystem and project-file helpers for generated binding output.</summary>
public static class GeneratedOutput
{
    /// <summary>Deletes and recreates a generated directory so stale files cannot survive a run.</summary>
    public static void RecreateDirectory(string directory)
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);
    }

    /// <summary>Resolves generated project directories, optionally replacing the configured binding root.</summary>
    public static GeneratedProjectLayout ResolveProjectLayout(
        string repositoryRoot,
        string? outputRoot,
        string apiProject,
        string backendProject)
    {
        if (outputRoot is null)
        {
            var apiDirectory = Path.GetFullPath(Path.Combine(repositoryRoot, apiProject));
            var backendDirectory = Path.GetFullPath(Path.Combine(repositoryRoot, backendProject));
            return new(Path.GetDirectoryName(apiDirectory)!, apiDirectory, backendDirectory);
        }

        var root = Path.GetFullPath(outputRoot);
        return new(
            root,
            Path.Combine(root, ProjectDirectoryName(apiProject)),
            Path.Combine(root, ProjectDirectoryName(backendProject)));
    }

    /// <summary>Emits the shared MSBuild settings used by generated API and backend projects.</summary>
    public static string EmitSharedProps() =>
        TemplateResource.Read(typeof(GeneratedOutput), "res/templates/bindgen/core/directory-build.props.tmpl");

    /// <summary>Returns the configured generated project directory name independent of platform separators.</summary>
    private static string ProjectDirectoryName(string projectPath)
    {
        var name = Path.GetFileName(projectPath.Replace('\\', '/'));
        return name.Length > 0
            ? name
            : throw new ArgumentException("Generated project path must include a directory name.", nameof(projectPath));
    }
}
