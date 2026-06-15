namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests repository root discovery for lint command execution.</summary>
[TestClass]
public sealed class RepositoryPathsTest
{
    /// <summary>Finds the repository root from a nested child directory.</summary>
    [TestMethod]
    public void FindRootWalksParentsUntilSolutionFile()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("AlvorKit.slnx", "<Solution />");
        var child = Path.Combine(workspace.Root, "a", "b");
        Directory.CreateDirectory(child);

        var root = RepositoryPaths.FindRoot(child);

        Assert.AreEqual(workspace.Root, root);
    }

    /// <summary>Fails clearly when no repository marker exists.</summary>
    [TestMethod]
    public void FindRootFailsWithoutSolutionFile()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsException<InvalidOperationException>(() => RepositoryPaths.FindRoot(workspace.Root));
    }
}
