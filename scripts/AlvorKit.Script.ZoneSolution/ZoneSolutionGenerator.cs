namespace AlvorKit.Script.ZoneSolution;

/// <summary>Generates a deterministic .slnx that groups sibling repositories by solution folder.</summary>
internal sealed class ZoneSolutionGenerator
{
    /// <summary>Generates the solution file described by the supplied options.</summary>
    /// <param name="options">Validated generator options.</param>
    public ZoneSolutionResult Generate(ZoneSolutionOptions options)
    {
        var repositories = ZoneRepositoryDiscovery.Discover(options);
        EnsureWritableOutput(options.OutputPath, repositories);

        var outputDirectory = Path.GetDirectoryName(options.OutputPath) ?? Environment.CurrentDirectory;
        var sourceDocuments = repositories
            .Select(repository => (Repository: repository, Document: SlnxDocument.Read(repository.SolutionPath)))
            .ToArray();
        var output = BuildDocument(sourceDocuments, outputDirectory);
        var content = SlnxXml.Format(output);
        var changed = !File.Exists(options.OutputPath) || File.ReadAllText(options.OutputPath) != content;

        if (changed)
        {
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(options.OutputPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        return new(
            options.OutputPath,
            changed,
            sourceDocuments
                .Select(source => new ZoneRepositoryResult(source.Repository.Name, source.Repository.SolutionPath, source.Document.Projects.Count))
                .ToArray());
    }

    /// <summary>Creates the combined solution document in output order.</summary>
    private static XDocument BuildDocument(
        IReadOnlyList<(ZoneRepository Repository, SlnxDocument Document)> sourceDocuments,
        string outputDirectory)
    {
        var root = new XElement("Solution");
        var folders = new Dictionary<string, XElement>(StringComparer.Ordinal);
        var projectPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in sourceDocuments)
            AddProjects(root, folders, projectPaths, source.Document.Projects, [source.Repository.Name], outputDirectory);

        return new(root);
    }

    /// <summary>Adds projects to the generated root, grouping all projects under repository-prefixed folders.</summary>
    private static void AddProjects(
        XElement root,
        IDictionary<string, XElement> folders,
        ISet<string> projectPaths,
        IEnumerable<SlnxProject> projects,
        IReadOnlyList<string> folderPrefix,
        string outputDirectory)
    {
        foreach (var project in projects)
        {
            if (!projectPaths.Add(Path.GetFullPath(project.AbsolutePath)))
                continue;

            var folderSegments = SolutionFolderPath.Combine(folderPrefix, project.FolderSegments);
            var element = project.ToElement(outputDirectory);
            EnsureParentFolders(root, folders, folderSegments);
            var folderName = SolutionFolderPath.FormatName(folderSegments);
            folders[folderName].Add(element);
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
    private static void EnsureWritableOutput(string outputPath, IEnumerable<ZoneRepository> repositories)
    {
        foreach (var repository in repositories)
        {
            if (PathText.SamePath(outputPath, repository.SolutionPath))
                throw new InvalidOperationException($"Output path must not overwrite the {repository.Name} solution.");
        }
    }
}
