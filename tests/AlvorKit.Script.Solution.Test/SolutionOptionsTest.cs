namespace AlvorKit;

/// <summary>Verifies repository discovery and command mode validation.</summary>
[TestClass]
public class SolutionOptionsTest
{
    /// <summary>Discovers managed checkouts without requiring a preexisting solution.</summary>
    [TestMethod]
    public void DiscoversOnlyManagedCheckouts()
    {
        using var workspace = TempWorkspace.Create();
        workspace.CreateDirectory("Game", ".git");
        workspace.Write("Game/src/Game.csproj", "<Project />");
        workspace.Write("Archive/src/Archive.csproj", "<Project />");
        workspace.CreateDirectory("Empty", ".git");
        var options = SolutionOptions.Create([], workspace.Root, false, false, true);
        CollectionAssert.AreEqual(new[] { Path.Combine(workspace.Root, "Game") }, options.DiscoverRepositories().ToArray());
    }

    /// <summary>Contradictory root selectors and write/check modes are explicit errors.</summary>
    [TestMethod]
    public void RejectsConflictingOptions()
    {
        Assert.ThrowsExactly<ArgumentException>(() => SolutionOptions.Create(["."], ".", false, false, false));
        Assert.ThrowsExactly<ArgumentException>(() => SolutionOptions.Create(["."], null, true, true, false));
        Assert.ThrowsExactly<ArgumentException>(() => SolutionOptions.Create(["."], null, true, false, true));
    }
}
