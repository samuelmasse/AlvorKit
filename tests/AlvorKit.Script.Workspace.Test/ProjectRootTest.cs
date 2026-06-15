namespace AlvorKit.Script.Workspace.Test;

/// <summary>Tests repository-root discovery shared by script tools.</summary>
[TestClass]
public sealed class ProjectRootTest
{
    /// <summary>Root discovery walks upward from nested directories.</summary>
    [TestMethod]
    public void FindFrom_FindsSolutionRootFromNestedDirectory()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(ProjectRoot.SolutionFileName, "<Solution />");
        var nested = Path.Combine(workspace.Root, "scripts", "tool", "bin");
        Directory.CreateDirectory(nested);

        Assert.AreEqual(workspace.Root, ProjectRoot.FindFrom(nested));
    }

    /// <summary>Root discovery accepts a file path by starting from its containing directory.</summary>
    [TestMethod]
    public void FindFrom_AcceptsFilePath()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(ProjectRoot.SolutionFileName, "<Solution />");
        var file = workspace.Write("scripts/tool/file.txt", "content");

        Assert.AreEqual(workspace.Root, ProjectRoot.FindFrom(file));
    }

    /// <summary>Root discovery rejects blank starting paths before attempting filesystem access.</summary>
    [TestMethod]
    public void FindFrom_RejectsBlankStartPath()
    {
        var exception = Assert.ThrowsException<ArgumentException>(() => ProjectRoot.FindFrom(" "));

        StringAssert.Contains(exception.Message, "Start path");
    }

    /// <summary>Root discovery can require the repository resource directory.</summary>
    [TestMethod]
    public void FindFrom_RequiresResDirectoryWhenRequested()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(ProjectRoot.SolutionFileName, "<Solution />");

        Assert.ThrowsException<InvalidOperationException>(() => ProjectRoot.FindFrom(workspace.Root, requireResDirectory: true));

        Directory.CreateDirectory(Path.Combine(workspace.Root, "res"));

        Assert.AreEqual(workspace.Root, ProjectRoot.FindFrom(workspace.Root, requireResDirectory: true));
    }

    /// <summary>Candidate-root discovery skips missing roots until a later candidate matches.</summary>
    [TestMethod]
    public void FindFromCandidates_UsesLaterMatchingCandidate()
    {
        using var missing = TempWorkspace.Create();
        using var workspace = TempWorkspace.Create();
        workspace.Write(ProjectRoot.SolutionFileName, "<Solution />");

        Assert.AreEqual(workspace.Root, ProjectRoot.FindFromCandidates(["", missing.Root, workspace.Root]));
    }

    /// <summary>Candidate-root discovery reports a clear failure when no candidate matches.</summary>
    [TestMethod]
    public void FindFromCandidates_FailsClearlyWithoutResources()
    {
        using var workspace = TempWorkspace.Create();

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => ProjectRoot.FindFromCandidates([workspace.Root], requireResDirectory: true));

        StringAssert.Contains(exception.Message, "res");
    }

    /// <summary>Candidate-root discovery reports the solution marker when no resource directory is required.</summary>
    [TestMethod]
    public void FindFromCandidates_FailsClearlyWithoutSolution()
    {
        using var workspace = TempWorkspace.Create();

        var exception = Assert.ThrowsException<InvalidOperationException>(() => ProjectRoot.FindFromCandidates([workspace.Root]));

        StringAssert.Contains(exception.Message, ProjectRoot.SolutionFileName);
    }

    /// <summary>Missing repository markers fail with the marker name in the error message.</summary>
    [TestMethod]
    public void FindFrom_FailsClearlyWithoutSolutionFile()
    {
        using var workspace = TempWorkspace.Create();

        var exception = Assert.ThrowsException<InvalidOperationException>(() => ProjectRoot.FindFrom(workspace.Root));

        StringAssert.Contains(exception.Message, ProjectRoot.SolutionFileName);
    }
}
