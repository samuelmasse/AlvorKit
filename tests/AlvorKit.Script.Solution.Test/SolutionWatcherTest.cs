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
        workspace.CreateDirectory("Game", ".git");
        workspace.Write("Game/src/Game/Game.csproj", SolutionGeneratorTest.Project(""));
        using var cancellation = new CancellationTokenSource();
        using var watcher = new SolutionWatcher(new([], workspace.Root, true, false, false));
        var task = watcher.RunAsync(cancellation.Token);

        try
        {
            await WaitFor(root, "Game.csproj", true);
            workspace.Write("Game/src/Game.Extra/Game.Extra.csproj", SolutionGeneratorTest.Project(""));
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
            Directory.Delete(Path.Combine(root, ".git"));
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

    /// <summary>Output churn is ignored, while dotted directory removals and generated project inputs invalidate.</summary>
    [TestMethod]
    public void FiltersInputsWithoutDependingOnPathExistence()
    {
        var root = Path.GetFullPath("watch-inputs");
        Assert.IsTrue(SolutionWatchInputs.Relevant(root, Path.Combine(root, "Game/src/Game.Moved"), true));
        Assert.IsTrue(SolutionWatchInputs.Relevant(root, Path.Combine(root, "AlvorKit/out/bindgen/Binding.csproj"), false));
        Assert.IsTrue(SolutionWatchInputs.Relevant(root, Path.Combine(root, "Game/.git"), true));
        Assert.IsFalse(SolutionWatchInputs.Relevant(root, Path.Combine(root, "Game/obj/Build.props"), false));
        Assert.IsFalse(SolutionWatchInputs.Relevant(root, Path.Combine(root, "Game/Game.slnx"), false));
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
