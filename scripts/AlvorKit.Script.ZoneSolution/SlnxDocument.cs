namespace AlvorKit.Script.ZoneSolution;

/// <summary>Project entries read from a .slnx file.</summary>
/// <param name="SolutionPath">Source solution path.</param>
/// <param name="Projects">Projects in source traversal order.</param>
internal sealed record SlnxDocument(string SolutionPath, IReadOnlyList<SlnxProject> Projects)
{
    /// <summary>Reads project entries from a .slnx file.</summary>
    /// <param name="solutionPath">Path to the source solution.</param>
    public static SlnxDocument Read(string solutionPath)
    {
        var fullPath = Path.GetFullPath(solutionPath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Solution file not found.", fullPath);

        var document = XDocument.Load(fullPath);
        if (document.Root?.Name.LocalName != "Solution")
            throw new InvalidOperationException($"{fullPath} is not a .slnx Solution document.");

        var projects = new List<SlnxProject>();
        CollectProjects(fullPath, document.Root, [], projects);
        return new(fullPath, projects);
    }

    /// <summary>Collects project entries from a solution or folder element.</summary>
    private static void CollectProjects(
        string solutionPath,
        XElement container,
        IReadOnlyList<string> currentFolder,
        ICollection<SlnxProject> projects)
    {
        foreach (var element in container.Elements())
        {
            if (element.Name.LocalName == "Project")
            {
                projects.Add(SlnxProject.FromElement(solutionPath, element, currentFolder));
                continue;
            }

            if (element.Name.LocalName == "Folder")
            {
                var nextFolder = ResolveFolderSegments(element, currentFolder);
                CollectProjects(solutionPath, element, nextFolder, projects);
                continue;
            }

            throw new InvalidOperationException($"Unsupported .slnx element <{element.Name.LocalName}> in {solutionPath}.");
        }
    }

    /// <summary>Resolves a folder element name against the current source folder path.</summary>
    private static IReadOnlyList<string> ResolveFolderSegments(XElement folder, IReadOnlyList<string> currentFolder)
    {
        var name = folder.Attribute("Name")?.Value;
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Solution folder is missing a Name attribute.");

        var segments = SolutionFolderPath.ParseSegments(name);
        return SolutionFolderPath.IsRootedName(name) ? segments : SolutionFolderPath.Combine(currentFolder, segments);
    }
}

/// <summary>A single project entry read from a source .slnx file.</summary>
/// <param name="AbsolutePath">Absolute project file path.</param>
/// <param name="FolderSegments">Source solution folder segments.</param>
/// <param name="SourceElement">Original project element whose non-path attributes should be preserved.</param>
internal sealed record SlnxProject(string AbsolutePath, IReadOnlyList<string> FolderSegments, XElement SourceElement)
{
    /// <summary>Creates a project entry from a source project element.</summary>
    /// <param name="solutionPath">Source solution path.</param>
    /// <param name="element">Source project element.</param>
    /// <param name="folderSegments">Solution folder segments containing the project.</param>
    public static SlnxProject FromElement(string solutionPath, XElement element, IReadOnlyList<string> folderSegments)
    {
        var path = element.Attribute("Path")?.Value;
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Solution project is missing a Path attribute.");

        var solutionDirectory = Path.GetDirectoryName(Path.GetFullPath(solutionPath)) ?? Environment.CurrentDirectory;
        var absolutePath = Path.GetFullPath(Path.Combine(solutionDirectory, path));
        return new(absolutePath, folderSegments.ToArray(), new XElement(element));
    }

    /// <summary>Creates the generated project element for an output solution directory.</summary>
    /// <param name="outputDirectory">Directory containing the generated solution.</param>
    public XElement ToElement(string outputDirectory) =>
        new(
            "Project",
            SourceElement.Attributes().Select(attribute =>
                new XAttribute(
                    attribute.Name,
                    attribute.Name.LocalName == "Path"
                        ? PathText.ToSlnxPath(Path.GetRelativePath(outputDirectory, AbsolutePath))
                        : attribute.Value)));
}
