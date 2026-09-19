namespace AlvorKit;

/// <summary>Verifies the public command routes explicit roots and modes without requiring a solution.</summary>
[TestClass]
public class SolutionCommandTest
{
    /// <summary>Repeated roots are normalized once and the requested operating mode reaches execution.</summary>
    [TestMethod]
    [DataRow("--check")]
    [DataRow("--watch")]
    [DataRow("--list-only")]
    public async Task RoutesExplicitRootsAndMode(string mode)
    {
        using var workspace = TempWorkspace.Create();
        SolutionOptions? selected = null;
        var command = SolutionOptions.Command(options =>
        {
            selected = options;
            return Task.FromResult(23);
        });
        var exitCode = await command.Parse(["--repo-root", workspace.Root, workspace.Root, mode]).InvokeAsync();

        Assert.AreEqual(23, exitCode);
        Assert.IsNotNull(selected);
        CollectionAssert.AreEqual(new[] { workspace.Root }, selected.DiscoverRepositories().ToArray());
        Assert.AreEqual(mode == "--check", selected.Check);
        Assert.AreEqual(mode == "--watch", selected.Watch);
        Assert.AreEqual(mode == "--list-only", selected.ListOnly);
        Assert.IsNull(selected.ParentDirectory);
    }

    /// <summary>Default invocation finds the checkout from Git metadata rather than a solution file.</summary>
    [TestMethod]
    public void DefaultRootUsesCurrentCheckout()
    {
        var options = SolutionOptions.Create([], null, false, false, false);
        CollectionAssert.AreEqual(new[] { RepositoryRoot.FindFrom(Environment.CurrentDirectory) },
            options.DiscoverRepositories().ToArray());
    }

    /// <summary>Explicit roots must exist before project discovery validates their Git metadata.</summary>
    [TestMethod]
    public void ValidatesExplicitDirectoryExistence()
    {
        using var workspace = TempWorkspace.Create();
        var options = SolutionOptions.Create([workspace.Root], null, false, false, false);
        CollectionAssert.AreEqual(new[] { workspace.Root }, options.DiscoverRepositories().ToArray());

        var missing = SolutionOptions.Create([Path.Combine(workspace.Root, "Missing")], null, false, false, false);
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => missing.DiscoverRepositories());
    }
}
