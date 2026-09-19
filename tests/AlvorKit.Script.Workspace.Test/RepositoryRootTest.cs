namespace AlvorKit;

/// <summary>Verifies that repository identity and project discovery do not require a solution.</summary>
[TestClass]
public class RepositoryRootTest
{
    /// <summary>Both normal checkout directories and Git indirection files mark a root.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void FindRootWithoutSolution(bool gitFile)
    {
        using var workspace = TempWorkspace.Create();

        if (gitFile)
            workspace.Write(".git", "gitdir: elsewhere");
        else workspace.CreateDirectory(".git");

        var source = workspace.Write("src/Game/Main.cs", "");
        Assert.AreEqual(workspace.Root, RepositoryRoot.FindFrom(source));
    }

    /// <summary>Only managed source areas contribute roots; generated dependencies enter through graph references.</summary>
    [TestMethod]
    public void DiscoveryExcludesOutputAndTemplates()
    {
        using var workspace = TempWorkspace.Create();
        GitRepositoryFixture.Initialize(workspace.Root);
        workspace.Write(".gitignore", "obj/\nout/\n");
        var project = workspace.Write("src/Game/Game.csproj", "<Project />");
        workspace.Write("src/Game/obj/Hidden.csproj", "<Project />");
        workspace.Write("out/bindgen/Generated.csproj", "<Project />");
        workspace.Write("res/templates/Starter.csproj", "<Project />");
        workspace.Write("native/Native.csproj", "<Project />");
        CollectionAssert.AreEqual(new[] { project }, RepositoryProjects.Discover(workspace.Root).ToArray());
        Assert.AreEqual("Game", RepositoryProjects.Namespace(workspace.Root));
    }

    /// <summary>Commands fail clearly without generated output or with ambiguous output.</summary>
    [TestMethod]
    public void RequireExactlyOneSolution()
    {
        using var workspace = TempWorkspace.Create();
        var missing = Assert.ThrowsExactly<InvalidOperationException>(() => RepositoryProjects.RequireSolutionFileName(workspace.Root));
        StringAssert.Contains(missing.Message, "AlvorKit.Script.Solution");
        workspace.Write("Game.slnx", "<Solution />");
        Assert.AreEqual("Game.slnx", RepositoryProjects.RequireSolutionFileName(workspace.Root));
        workspace.Write("Other.slnx", "<Solution />");
        Assert.ThrowsExactly<InvalidOperationException>(() => RepositoryProjects.RequireSolutionFileName(workspace.Root));
    }
}
