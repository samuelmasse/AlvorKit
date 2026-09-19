namespace AlvorKit;

/// <summary>Verifies filesystem-driven updates with actual isolated notifications.</summary>
[TestClass]
[DoNotParallelize]
public class SolutionWatcherTest
{
    /// <summary>Creating, moving, and deleting projects updates one solution without manual membership edits.</summary>
    [TestMethod]
    public async Task ReconcilesProjectDirectoryChanges()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.CreateDirectory("Game");
        GitRepositoryFixture.Initialize(root);
        workspace.Write("Game/src/Game/Game.csproj", SolutionGeneratorTest.Project(""));
        using var cancellation = new CancellationTokenSource();
        using var watcher = new SolutionWatcher(new([], workspace.Root, true, false, false));
        var task = watcher.RunAsync(cancellation.Token);

        try
        {
            await WaitFor(root, "Game.csproj", true);
            workspace.Write("Game/src/Game.Extra/Game.Extra.csproj", SolutionGeneratorTest.Project(""));
            await WaitFor(root, "src/Game.Extra/Game.Extra.csproj", true);
            workspace.Write("Game/.gitignore", "src/Game.Extra/\n");
            await WaitFor(root, "src/Game.Extra/Game.Extra.csproj", false);
            workspace.Write("Game/.gitignore", "");
            await WaitFor(root, "src/Game.Extra/Game.Extra.csproj", true);
            Directory.Move(Path.Combine(root, "src/Game.Extra"), Path.Combine(root, "src/Game.Moved"));
            await WaitFor(root, "src/Game.Moved/Game.Extra.csproj", true);
            Directory.Delete(Path.Combine(root, "src/Game.Moved"), true);
            await WaitFor(root, "Game.Extra.csproj", false);
            Assert.AreEqual(1, Directory.GetFiles(root, "*.slnx").Length);

            workspace.Write("Game/src/Game/Game.csproj", SolutionGeneratorTest.Project("""
                <ItemGroup><ProjectReference Include="../Missing/Missing.csproj" /></ItemGroup>
                """));
            await WaitForExistence(root, false);
            workspace.Write("Game/src/Missing/Missing.csproj", SolutionGeneratorTest.Project(""));
            await WaitFor(root, "Missing.csproj", true);

            File.Delete(Path.Combine(root, "src/Game/Game.csproj"));
            File.Delete(Path.Combine(root, "src/Missing/Missing.csproj"));
            await WaitForExistence(root, false);
            workspace.Write("Game/src/Game/Game.csproj", SolutionGeneratorTest.Project(""));
            await WaitFor(root, "Game.csproj", true);
            var renamed = Path.Combine(workspace.Root, "Renamed");
            Directory.Move(root, renamed);
            root = renamed;
            await WaitFor(root, "Game.csproj", true);
            Assert.IsFalse(File.Exists(Path.Combine(root, "Game.slnx")));
            Directory.Delete(Path.Combine(root, ".git"), recursive: true);
            await WaitForExistence(root, false);
        }
        finally
        {
            cancellation.Cancel();

            try { await task; }
            catch (OperationCanceledException) { }
        }
    }

    /// <summary>Waits for graph invalidation to remove stale generated output.</summary>
    private static async Task WaitForExistence(string root, bool exists)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);

        while (DateTime.UtcNow < deadline)
        {
            if (File.Exists(RepositoryProjects.SolutionPath(root)) == exists)
                return;

            await Task.Delay(50);
        }

        Assert.Fail($"Solution existence did not become {exists}.");
    }

    /// <summary>Existence reads ignore content edits, while missing imports and directory moves remain observable.</summary>
    [TestMethod]
    public void MatchesFilesystemReadSemantics()
    {
        var root = Path.GetFullPath("watch-inputs");
        var path = Path.Combine(root, "Shared/References.props");
        var existence = new SolutionWatchInput(path, null, false, false);
        var content = new SolutionWatchInput(path, null, false, true);
        Assert.IsFalse(existence.Matches(path, false, WatcherChangeTypes.Changed));
        Assert.IsTrue(existence.Matches(path, false, WatcherChangeTypes.Created));
        Assert.IsTrue(content.Matches(path, false, WatcherChangeTypes.Changed));
        Assert.IsTrue(content.Matches(Path.Combine(root, "Shared"), true, WatcherChangeTypes.Renamed));
        Assert.IsFalse(content.Matches(Path.Combine(root, "Shared/Other.props"), false, WatcherChangeTypes.Changed));
        var glob = new SolutionWatchInput(root, "*.csproj", true, false);
        Assert.IsTrue(glob.Matches(Path.Combine(root, "src/New/New.csproj"), false, WatcherChangeTypes.Created));
        Assert.IsTrue(glob.Matches(Path.Combine(root, "src/New"), true, WatcherChangeTypes.Renamed));
        Assert.IsFalse(glob.Matches(Path.Combine(root, "src/New/Code.cs"), false, WatcherChangeTypes.Created));
        Assert.IsFalse(glob.Matches(Path.Combine(root, "src/New/New.csproj"), false, WatcherChangeTypes.Changed));
    }

    /// <summary>Waits for observable serialized membership with a bounded timeout.</summary>
    private static async Task WaitFor(string root, string text, bool present)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        var solution = RepositoryProjects.SolutionPath(root);

        while (DateTime.UtcNow < deadline)
        {
            if (File.Exists(solution) && File.ReadAllText(solution).Contains(text, StringComparison.Ordinal) == present)
                return;

            await Task.Delay(50);
        }

        Assert.Fail($"Solution membership did not become {present} for {text}.");
    }
}
