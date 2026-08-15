namespace AlvorKit;

/// <summary>Audits repository project edges that must remain profiler-free.</summary>
[TestClass]
public sealed class ProfilerFreeProjectBoundaryAuditTest
{
    private const string CoreClrProject =
        "src/AlvorKit.Interception.CoreClr/AlvorKit.Interception.CoreClr.csproj";

    private const string LauncherProject =
        "scripts/AlvorKit.Script.TestInterception/AlvorKit.Script.TestInterception.csproj";

    private static readonly string[] ForbiddenReferences =
    [
        "AlvorKit.Interception.CoreClr",
        "AlvorKit.Interception.Profiler.Backend",
        "AlvorKit.Interception.Profiler.Native",
        "native/interception-profiler"
    ];

    /// <summary>The neutral Interception project closure contains no CoreCLR or profiler asset edge.</summary>
    [TestMethod]
    public void NeutralInterceptionClosure_HasNoProfilerHostOrNativeAsset()
    {
        var repositoryRoot = FindRepositoryRoot();

        AssertProfilerFreeClosure(
            repositoryRoot,
            "src/AlvorKit.Interception/AlvorKit.Interception.csproj");
    }

    /// <summary>Ordinary Mocking and game-runtime closures do not acquire the optional profiler host.</summary>
    [TestMethod]
    public void OrdinaryMockingAndGameClosures_HaveNoProfilerHostOrNativeAsset()
    {
        var repositoryRoot = FindRepositoryRoot();
        string[] ordinaryConsumers =
        [
            "src/AlvorKit.Mocking/AlvorKit.Mocking.csproj",
            "src/AlvorKit.Mocking.Dynamic/AlvorKit.Mocking.Dynamic.csproj",
            "demos/AlvorKit.Mocking.Demo/AlvorKit.Mocking.Demo.csproj",
            "src/AlvorKit.Engine/AlvorKit.Engine.csproj",
            "src/AlvorKit.Engine.Loop/AlvorKit.Engine.Loop.csproj"
        ];

        foreach (var consumer in ordinaryConsumers)
            AssertProfilerFreeClosure(repositoryRoot, consumer);
    }

    /// <summary>Only the CoreCLR backend and isolated launcher carry direct profiler package references.</summary>
    [TestMethod]
    public void ProfilerHostProjects_AreOnlyDirectProfilerPackageCarriers()
    {
        var repositoryRoot = FindRepositoryRoot();
        var carriers = ProjectFiles(repositoryRoot)
            .Where(
                project => References(project)
                    .Any(
                        reference =>
                            IsProfilerBackendOrAsset(reference.Include)))
            .Select(project => Relative(repositoryRoot, project))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { LauncherProject, CoreClrProject },
            carriers);

        var coreClr = Path.Combine(
            repositoryRoot,
            CoreClrProject.Replace('/', Path.DirectorySeparatorChar));
        var references = References(coreClr).ToArray();
        Assert.IsTrue(
            references.Any(
                reference =>
                    reference.Kind == "ProjectReference" &&
                    reference.Include.Contains(
                        "AlvorKit.Interception",
                        StringComparison.Ordinal) &&
                    !reference.Include.Contains(
                        "Profiler.Backend",
                        StringComparison.Ordinal)));
        Assert.IsTrue(
            references.Any(
                reference =>
                    reference.Kind == "PackageReference" &&
                    reference.Include ==
                    "AlvorKit.Interception.Profiler.Backend"));

        var launcher = Path.Combine(
            repositoryRoot,
            LauncherProject.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.Contains(
            References(launcher)
                .Where(static reference =>
                    reference.Kind == "PackageReference")
                .Select(static reference => reference.Include)
                .ToArray(),
            "AlvorKit.Interception.Profiler.Native");
    }

    private static void AssertProfilerFreeClosure(
        string repositoryRoot,
        string relativeProject)
    {
        var project = Path.Combine(
            repositoryRoot,
            relativeProject.Replace('/', Path.DirectorySeparatorChar));
        var closure = ProjectClosure(project);
        var violations = closure
            .SelectMany(
                path => References(path)
                    .Where(
                        reference =>
                            IsForbidden(reference.Include))
                    .Select(
                        reference =>
                            $"{Relative(repositoryRoot, path)}: " +
                            $"{reference.Kind}={reference.Include}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.HasCount(
            0,
            violations,
            $"{relativeProject} profiler references: " +
            $"{string.Join(", ", violations)}.");
    }

    private static IReadOnlyList<string> ProjectClosure(string rootProject)
    {
        var pending = new Stack<string>();
        var visited = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);
        pending.Push(Path.GetFullPath(rootProject));

        while (pending.TryPop(out var project))
        {
            if (!visited.Add(project))
                continue;
            if (!File.Exists(project))
            {
                Assert.Fail($"Project graph entry does not exist: {project}.");
                continue;
            }

            foreach (var reference in References(project))
            {
                if (reference.Kind != "ProjectReference" ||
                    reference.Include.Contains("$(", StringComparison.Ordinal))
                {
                    continue;
                }

                var normalizedReference = reference.Include
                    .Replace('\\', Path.DirectorySeparatorChar)
                    .Replace('/', Path.DirectorySeparatorChar);
                var referencedProject = Path.GetFullPath(
                    normalizedReference,
                    Path.GetDirectoryName(project)!);
                pending.Push(referencedProject);
            }
        }

        return [.. visited.Order(StringComparer.Ordinal)];
    }

    private static IEnumerable<ProjectReferenceEntry> References(
        string project)
    {
        var document = XDocument.Load(project);
        return document
            .Descendants()
            .Where(
                static element =>
                    element.Name.LocalName is
                        "ProjectReference" or
                        "PackageReference" or
                        "NativeReference" or
                        "Content" or
                        "None")
            .Select(
                static element =>
                    new ProjectReferenceEntry(
                        element.Name.LocalName,
                        element.Attribute("Include")?.Value ?? ""))
            .Where(static reference => reference.Include.Length > 0);
    }

    private static IReadOnlyList<string> ProjectFiles(string repositoryRoot)
    {
        string[] roots = ["src", "scripts", "demos", "tests"];
        return
        [
            .. roots
                .Select(root => Path.Combine(repositoryRoot, root))
                .Where(Directory.Exists)
                .SelectMany(
                    root =>
                        Directory.GetFiles(
                            root,
                            "*.csproj",
                            SearchOption.AllDirectories))
                .Order(StringComparer.Ordinal)
        ];
    }

    private static bool IsForbidden(string reference) =>
        ForbiddenReferences.Any(
            forbidden =>
                reference.Replace('\\', '/')
                    .Contains(forbidden, StringComparison.Ordinal));

    private static bool IsProfilerBackendOrAsset(string reference)
    {
        var normalized = reference.Replace('\\', '/');
        return normalized.Contains(
                   "AlvorKit.Interception.Profiler.Backend",
                   StringComparison.Ordinal) ||
               normalized.Contains(
                   "AlvorKit.Interception.Profiler.Native",
                   StringComparison.Ordinal) ||
               normalized.Contains(
                   "native/interception-profiler",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlvorKit.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find the AlvorKit repository root.");
    }

    private static string Relative(string repositoryRoot, string path) =>
        Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');

    private sealed record ProjectReferenceEntry(
        string Kind,
        string Include);
}
