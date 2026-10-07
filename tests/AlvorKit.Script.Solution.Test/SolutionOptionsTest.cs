namespace AlvorKit;

/// <summary>Verifies repository discovery and command mode validation.</summary>
[TestClass]
public class SolutionOptionsTest
{
    /// <summary>Aggregate output requires an existing directory, the supported extension, and unambiguous repository folders.</summary>
    [TestMethod]
    public void RejectsInvalidAggregatePathsAndDuplicateNames()
    {
        using var workspace = TempWorkspace.Create();
        var invalid = Path.Combine(workspace.Root, "Workspace.sln");
        Assert.ThrowsExactly<ArgumentException>(() => SolutionOptions.Create([], workspace.Root, false, false, false, invalid));
        var missing = Path.Combine(workspace.Root, "Missing/Workspace.slnx");
        Assert.ThrowsExactly<DirectoryNotFoundException>(
            () => SolutionOptions.Create([], workspace.Root, false, false, false, missing));
        var first = workspace.CreateDirectory("First/Game");
        var second = workspace.CreateDirectory("Second/Game");
        var output = Path.Combine(workspace.Root, "Workspace.slnx");
        var options = SolutionOptions.Create([first, second], null, false, false, false, output);
        Assert.ThrowsExactly<ArgumentException>(() => options.DiscoverRepositories());
    }

    /// <summary>Discovers managed checkouts without requiring a preexisting solution.</summary>
    [TestMethod]
    public void DiscoversOnlyManagedCheckouts()
    {
        using var workspace = TempWorkspace.Create();
        GitRepositoryFixture.Initialize(workspace.CreateDirectory("Game"));
        workspace.Write("Game/src/Game.csproj", "<Project />");
        workspace.Write("Archive/src/Archive.csproj", "<Project />");
        GitRepositoryFixture.Initialize(workspace.CreateDirectory("Empty"));
        var options = SolutionOptions.Create([], workspace.Root, false, false, true, null);
        CollectionAssert.AreEqual(new[] { Path.Combine(workspace.Root, "Game") }, options.DiscoverRepositories().ToArray());
    }

    /// <summary>Contradictory root selectors and write/check modes are explicit errors.</summary>
    [TestMethod]
    public void RejectsConflictingOptions()
    {
        Assert.ThrowsExactly<ArgumentException>(() => SolutionOptions.Create(["."], ".", false, false, false, null));
        Assert.ThrowsExactly<ArgumentException>(() => SolutionOptions.Create(["."], null, true, true, false, null));
        Assert.ThrowsExactly<ArgumentException>(() => SolutionOptions.Create(["."], null, true, false, true, null));
    }
}
