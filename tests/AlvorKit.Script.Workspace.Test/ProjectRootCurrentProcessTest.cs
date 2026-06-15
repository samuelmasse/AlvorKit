namespace AlvorKit.Script.Workspace.Test;

/// <summary>Tests process-based repository-root discovery.</summary>
[TestClass]
[DoNotParallelize]
public sealed class ProjectRootCurrentProcessTest
{
    /// <summary>Resource directory discovery returns the root <c>res</c> path.</summary>
    [TestMethod]
    public void ResDirectory_ReturnsRootResourceDirectory()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(ProjectRoot.SolutionFileName, "<Solution />");
        var resDirectory = Path.Combine(workspace.Root, "res");
        Directory.CreateDirectory(resDirectory);

        WithCurrentDirectory(workspace.Root, () => Assert.AreEqual(resDirectory, ProjectRoot.ResDirectory()));
    }

    /// <summary>Process-root discovery checks the current directory first.</summary>
    [TestMethod]
    public void FindFromCurrentProcess_UsesCurrentDirectory()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(ProjectRoot.SolutionFileName, "<Solution />");

        WithCurrentDirectory(workspace.Root, () => Assert.AreEqual(workspace.Root, ProjectRoot.FindFromCurrentProcess()));
    }

    /// <summary>Process-root discovery checks the anchor assembly when the current directory is not in the repository.</summary>
    [TestMethod]
    public void FindFromCurrentProcess_UsesAnchorAssemblyDirectory()
    {
        using var workspace = TempWorkspace.Create();

        WithCurrentDirectory(workspace.Root, () => Assert.AreEqual(
            ProjectRoot.FindFrom(typeof(ProjectRootCurrentProcessTest).Assembly.Location),
            ProjectRoot.FindFromCurrentProcess(typeof(ProjectRootCurrentProcessTest))));
    }

    /// <summary>Process-root discovery falls back to the executable base directory when no anchor is provided.</summary>
    [TestMethod]
    public void FindFromCurrentProcess_UsesAppBaseDirectory()
    {
        using var workspace = TempWorkspace.Create();

        WithCurrentDirectory(workspace.Root, () => Assert.AreEqual(
            ProjectRoot.FindFrom(AppContext.BaseDirectory),
            ProjectRoot.FindFromCurrentProcess()));
    }

    /// <summary>Runs an assertion with a temporary current directory and restores the previous process state.</summary>
    private static void WithCurrentDirectory(string directory, Action assertion)
    {
        var previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = directory;
            assertion();
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }
}
