namespace AlvorKit;

/// <summary>Checks dependency-scoped invalidation with actual MSBuild reads and native notifications.</summary>
[TestClass]
[DoNotParallelize]
public class SolutionWatchDependencyTest
{
    /// <summary>Shared dependency changes update their consumers and leave unrelated repositories idle.</summary>
    [TestMethod]
    public async Task EvaluatesOnlyAffectedSolutions()
    {
        await using var fixture = new SolutionWatchFixture();
        var reference = "<ItemGroup><ProjectReference Include=\"../../../../Shared/Shared.csproj\" /></ItemGroup>";
        var first = fixture.Repository("First", reference);
        var second = fixture.Repository("Second", reference);
        var unrelated = fixture.Repository("Unrelated", "");
        fixture.Write("Shared/Shared.csproj", SolutionGeneratorTest.Project(""));
        fixture.Write("Shared/Extra/Extra.csproj", SolutionGeneratorTest.Project(""));
        fixture.Write("Repos/First/.gitignore", "src/Ignored/\n");
        fixture.Write("Repos/First/src/First/Code.cs", "class Code { }");
        fixture.Start();
        await fixture.Until(() => fixture.Count(first) > 0 && fixture.Count(second) > 0 && fixture.Count(unrelated) > 0);
        await fixture.Quiet(first, second, unrelated);
        var initial = new[] { fixture.Count(first), fixture.Count(second), fixture.Count(unrelated) };

        fixture.Write("Repos/First/src/First/Code.cs", "class Code { public int Value; }");
        fixture.Write("Repos/First/src/Ignored/Bad.csproj", "not a project");
        await fixture.Quiet(first, second, unrelated);
        CollectionAssert.AreEqual(initial, new[] { fixture.Count(first), fixture.Count(second), fixture.Count(unrelated) });

        fixture.Write("Shared/Shared.csproj", SolutionGeneratorTest.Project(
            "<ItemGroup><ProjectReference Include=\"Extra/Extra.csproj\" /></ItemGroup>"));
        await fixture.Membership(first, "Extra.csproj", true);
        await fixture.Membership(second, "Extra.csproj", true);
        await fixture.Quiet(first, second, unrelated);
        Assert.AreEqual(initial[2], fixture.Count(unrelated));

        var secondCount = fixture.Count(second);
        fixture.Write("Repos/First/src/First/First.csproj", SolutionGeneratorTest.Project(""));
        await fixture.Membership(first, "Shared.csproj", false);
        await fixture.Quiet(first, second, unrelated);
        Assert.AreEqual(secondCount, fixture.Count(second));
        Assert.AreEqual(initial[2], fixture.Count(unrelated));
    }

    /// <summary>Absent external imports are observed before Exists checks, regardless of their extension.</summary>
    [TestMethod]
    public async Task WatchesMissingExternalImportsAndRecoversFromInvalidXml()
    {
        await using var fixture = new SolutionWatchFixture();
        var body = """
            <Import Project="../../../../External/Nested/References.custom"
                    Condition="Exists('../../../../External/Nested/References.custom')" />
            """;
        var root = fixture.Repository("Game", body);
        fixture.Write("External/Extra.csproj", SolutionGeneratorTest.Project(""));
        fixture.Start();
        await fixture.Membership(root, "Game.csproj", true);
        fixture.Write("External/Nested/References.custom", """
            <Project><ItemGroup><ProjectReference Include="../../../../External/Extra.csproj" /></ItemGroup></Project>
            """);
        await fixture.Membership(root, "Extra.csproj", true);
        fixture.Write("Repos/Game/src/Game/Game.csproj", "<Project>");
        await fixture.Until(() => !File.Exists(RepositoryProjects.SolutionPath(root)));
        fixture.Write("Repos/Game/src/Game/Game.csproj", SolutionGeneratorTest.Project(body));
        await fixture.Membership(root, "Extra.csproj", true);
        fixture.Write("External/Nested/References.custom", "<Project />");
        await fixture.Membership(root, "Extra.csproj", false);
        await fixture.Quiet(root);
    }

    /// <summary>Unresolvable imports stop explicitly instead of leaving an incomplete watcher running.</summary>
    [TestMethod]
    public async Task RejectsUnobservableImportFailures()
    {
        using var workspace = TempWorkspace.Create();
        GitRepositoryFixture.Initialize(workspace.Root);
        workspace.Write("src/Game/Game.csproj", SolutionGeneratorTest.Project("<Import Project=\"missing.custom\" />"));
        using var watcher = new SolutionWatcher(new([workspace.Root], null, true, false, false));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var exception = await Assert.ThrowsExactlyAsync<SolutionImportException>(() => watcher.RunAsync(cancellation.Token));
        StringAssert.Contains(exception.Message, "Fix the import and restart");
        Assert.IsFalse(File.Exists(RepositoryProjects.SolutionPath(workspace.Root)));
    }

    /// <summary>A newly created Directory.Build.props can change root selection without editing a project.</summary>
    [TestMethod]
    public async Task WatchesPreviouslyAbsentDirectoryBuildProps()
    {
        await using var fixture = new SolutionWatchFixture();
        var root = fixture.Repository("Game", "");
        fixture.Write("Repos/Game/src/Tool/Tool.csproj", SolutionGeneratorTest.Project(""));
        fixture.Start();
        await fixture.Membership(root, "Tool.csproj", true);
        fixture.Write("Repos/Game/src/Tool/Directory.Build.props", """
            <Project><PropertyGroup><WorkspaceProject>false</WorkspaceProject></PropertyGroup></Project>
            """);
        await fixture.Membership(root, "Tool.csproj", false);
        File.Delete(fixture.PathFor("Repos/Game/src/Tool/Directory.Build.props"));
        await fixture.Membership(root, "Tool.csproj", true);
    }

    /// <summary>Wildcard import membership is observed even before its first matching file exists.</summary>
    [TestMethod]
    public async Task WatchesExternalWildcardImports()
    {
        await using var fixture = new SolutionWatchFixture();
        var root = fixture.Repository("Game", "<Import Project=\"../../../../External/Rules/*.custom\" />");
        fixture.Write("External/Extra.csproj", SolutionGeneratorTest.Project(""));
        fixture.Start();
        await fixture.Membership(root, "Game.csproj", true);
        fixture.Write("External/Rules/References.custom", """
            <Project><ItemGroup><ProjectReference Include="../../../../External/Extra.csproj" /></ItemGroup></Project>
            """);
        await fixture.Membership(root, "Extra.csproj", true);
        File.Delete(fixture.PathFor("External/Rules/References.custom"));
        await fixture.Membership(root, "Extra.csproj", false);
    }

    /// <summary>Index changes and external ignore configuration alter discovered membership immediately.</summary>
    [TestMethod]
    public async Task WatchesGitIndexAndExternalIgnoreRules()
    {
        await using var fixture = new SolutionWatchFixture();
        var root = fixture.Repository("Game", "");
        fixture.Write("Repos/Game/src/Extra/Extra.csproj", SolutionGeneratorTest.Project(""));
        fixture.Write("External/ignore", "src/Extra/\n");
        GitRepositoryFixture.Run(root, "config", "core.excludesFile", fixture.PathFor("External/ignore"));
        fixture.Start();
        await fixture.Membership(root, "Game.csproj", true);
        await fixture.Membership(root, "Extra.csproj", false);
        GitRepositoryFixture.Run(root, "add", "--force", "src/Extra/Extra.csproj");
        await fixture.Membership(root, "Extra.csproj", true);
        GitRepositoryFixture.Run(root, "rm", "--cached", "src/Extra/Extra.csproj");
        await fixture.Membership(root, "Extra.csproj", false);
        fixture.Write("External/ignore", "");
        await fixture.Membership(root, "Extra.csproj", true);
    }
}
