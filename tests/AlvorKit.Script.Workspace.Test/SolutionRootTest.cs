namespace AlvorKit.Script.Workspace.Test;

/// <summary>Tests generic solution-root discovery shared by script tools.</summary>
[TestClass]
public sealed class SolutionRootTest
{
    /// <summary>Specific solution discovery supports non-AlvorKit repository markers.</summary>
    [TestMethod]
    public void FindFrom_UsesNamedSolutionFile()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("Rombadil.slnx", "<Solution />");
        var nested = Path.Combine(workspace.Root, "src", "Tool");
        Directory.CreateDirectory(nested);

        Assert.AreEqual(workspace.Root, SolutionRoot.FindFrom(nested, "Rombadil.slnx"));
    }

    /// <summary>Primary solution discovery ignores generated development solutions.</summary>
    [TestMethod]
    public void FindPrimaryFrom_IgnoresGeneratedDevSolution()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("Rombadil.slnx", "<Solution />");
        workspace.Write("Rombadil.Dev.slnx", "<Solution />");
        var nested = Path.Combine(workspace.Root, "tests", "Tool.Test");
        Directory.CreateDirectory(nested);

        Assert.AreEqual(workspace.Root, SolutionRoot.FindPrimaryFrom(nested));
        Assert.AreEqual("Rombadil.slnx", SolutionRoot.PrimarySolutionFileName(workspace.Root));
        Assert.AreEqual(Path.Combine(workspace.Root, "Rombadil.slnx"), SolutionRoot.PrimarySolutionPath(workspace.Root));
    }

    /// <summary>Primary solution discovery reports ambiguity when a repository owns multiple non-generated solutions.</summary>
    [TestMethod]
    public void PrimarySolutionFileName_FailsForAmbiguousRootSolutions()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("One.slnx", "<Solution />");
        workspace.Write("Two.slnx", "<Solution />");

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => SolutionRoot.PrimarySolutionFileName(workspace.Root));

        StringAssert.Contains(exception.Message, "Multiple primary solution files");
    }

    /// <summary>Primary solution discovery fails clearly when only generated development solutions exist.</summary>
    [TestMethod]
    public void PrimarySolutionFileName_FailsWithoutPrimarySolution()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("Rombadil.Dev.slnx", "<Solution />");

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => SolutionRoot.PrimarySolutionFileName(workspace.Root));

        StringAssert.Contains(exception.Message, "No primary solution file");
    }

    /// <summary>Specific solution discovery rejects path-shaped markers.</summary>
    [TestMethod]
    public void FindFrom_RejectsSolutionPathMarker()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsExactly<ArgumentException>(() => SolutionRoot.FindFrom(workspace.Root, "subdir/Rombadil.slnx"));
    }
}
