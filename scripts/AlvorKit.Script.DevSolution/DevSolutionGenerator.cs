namespace AlvorKit.Script.DevSolution;

/// <summary>Generates a deterministic .slnx that combines a consumer solution with AlvorKit projects.</summary>
internal sealed class DevSolutionGenerator
{
    /// <summary>Generates the solution file described by the supplied options.</summary>
    /// <param name="options">Validated generator options.</param>
    public DevSolutionResult Generate(DevSolutionOptions options)
    {
        EnsureWritableOutput(options);

        var outputDirectory = Path.GetDirectoryName(options.OutputPath) ?? Environment.CurrentDirectory;
        var consumerSolution = SlnxDocument.Read(options.ConsumerSolutionPath);
        var engineSolution = SlnxDocument.Read(options.EngineSolutionPath);
        var output = BuildDocument(consumerSolution, engineSolution, options, outputDirectory);
        var content = SlnxXml.Format(output);
        var changed = !File.Exists(options.OutputPath) || File.ReadAllText(options.OutputPath) != content;

        if (changed)
        {
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(options.OutputPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        return new(options.OutputPath, changed, consumerSolution.Projects.Count, engineSolution.Projects.Count);
    }

    /// <summary>Creates the combined solution document in output order.</summary>
    private static XDocument BuildDocument(
        SlnxDocument consumerSolution,
        SlnxDocument engineSolution,
        DevSolutionOptions options,
        string outputDirectory)
    {
        var root = new XElement("Solution");
        var folders = new Dictionary<string, XElement>(StringComparer.Ordinal);
        var projectPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddProjects(root, folders, projectPaths, consumerSolution.Projects, [], outputDirectory, preserveDefaultStartup: true);
        AddProjects(root, folders, projectPaths, engineSolution.Projects, options.EngineFolderSegments, outputDirectory, preserveDefaultStartup: false);
        return new(root);
    }

    /// <summary>Adds projects to the generated root, grouping non-root projects by solution folder.</summary>
    private static void AddProjects(
        XElement root,
        IDictionary<string, XElement> folders,
        ISet<string> projectPaths,
        IEnumerable<SlnxProject> projects,
        IReadOnlyList<string> folderPrefix,
        string outputDirectory,
        bool preserveDefaultStartup)
    {
        foreach (var project in projects)
        {
            if (!projectPaths.Add(Path.GetFullPath(project.AbsolutePath)))
                continue;

            var folderSegments = SolutionFolderPath.Combine(folderPrefix, project.FolderSegments);
            var element = project.ToElement(outputDirectory);
            if (!preserveDefaultStartup)
                element.Attribute("DefaultStartup")?.Remove();

            if (folderSegments.Count == 0)
            {
                root.Add(element);
                continue;
            }

            var folderName = SolutionFolderPath.FormatName(folderSegments);
            EnsureParentFolders(root, folders, folderSegments);
            var folder = folders[folderName];

            folder.Add(element);
        }
    }

    /// <summary>Ensures every folder prefix exists before adding a project to the deepest folder.</summary>
    private static void EnsureParentFolders(XElement root, IDictionary<string, XElement> folders, IReadOnlyList<string> folderSegments)
    {
        for (var length = 1; length <= folderSegments.Count; length++)
        {
            var name = SolutionFolderPath.FormatName(folderSegments.Take(length).ToArray());
            if (folders.ContainsKey(name))
                continue;

            var folder = new XElement("Folder", new XAttribute("Name", name));
            folders.Add(name, folder);
            root.Add(folder);
        }
    }

    /// <summary>Rejects output paths that would overwrite an input solution.</summary>
    private static void EnsureWritableOutput(DevSolutionOptions options)
    {
        if (PathText.SamePath(options.OutputPath, options.ConsumerSolutionPath))
            throw new InvalidOperationException("Output path must not overwrite the consumer solution.");
        if (PathText.SamePath(options.OutputPath, options.EngineSolutionPath))
            throw new InvalidOperationException("Output path must not overwrite the engine solution.");
    }
}
