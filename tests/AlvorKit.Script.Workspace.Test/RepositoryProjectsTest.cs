namespace AlvorKit;

/// <summary>Verifies project discovery follows Git's tracked and untracked file rules.</summary>
[TestClass]
public class RepositoryProjectsTest
{
    /// <summary>Git ignore rules exclude untracked projects while tracked projects remain roots until removed from disk.</summary>
    [TestMethod]
    public void IncludesTrackedAndUntrackedProjectsWithoutIgnoredOrDeletedFiles()
    {
        using var workspace = TempWorkspace.Create();
        GitRepositoryFixture.Initialize(workspace.Root);
        workspace.Write(".gitignore", "src/Ignored/\n");
        workspace.Write("src/.gitignore", "NestedIgnored/\n");
        workspace.Write(".git/info/exclude", "src/LocalIgnored/\n");
        var tracked = workspace.Write("src/Ignored/Tracked.csproj", "<Project />");
        var deleted = workspace.Write("src/Deleted.csproj", "<Project />");
        GitRepositoryFixture.Run(workspace.Root, "add", "--force", "--", "src/Ignored/Tracked.csproj", "src/Deleted.csproj");
        File.Delete(deleted);
        var untracked = workspace.Write("src/New project/Grün.csproj", "<Project />");
        var root = workspace.Write("Game.csproj", "<Project />");
        workspace.Write("src/Ignored/Hidden.csproj", "<Project />");
        workspace.Write("src/NestedIgnored/Hidden.csproj", "<Project />");
        workspace.Write("src/LocalIgnored/Hidden.csproj", "<Project />");
        workspace.Write("res/templates/Hidden.csproj", "<Project />");

        CollectionAssert.AreEqual(new[] { root, tracked, untracked }.Order(StringComparer.Ordinal).ToArray(),
            RepositoryProjects.Discover(workspace.Root).ToArray());
    }

    /// <summary>Git listing errors and uninitialized directories fail explicitly rather than changing discovery rules.</summary>
    [TestMethod]
    public void RejectsMissingOrInvalidGitMetadata()
    {
        using var workspace = TempWorkspace.Create();
        var missing = Assert.ThrowsExactly<InvalidOperationException>(() => RepositoryProjects.Discover(workspace.Root));
        StringAssert.Contains(missing.Message, "git init");
        workspace.CreateDirectory(".git");
        var invalid = Assert.ThrowsExactly<InvalidOperationException>(() => RepositoryProjects.Discover(workspace.Root));
        StringAssert.Contains(invalid.Message, "Git query failed");
    }
}
