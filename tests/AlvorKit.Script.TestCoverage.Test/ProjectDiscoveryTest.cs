namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for discovering targeted test projects and their source modules.</summary>
[TestClass]
public sealed class ProjectDiscoveryTest
{
    /// <summary>Repository root discovery works from the coverage test process output directory.</summary>
    [TestMethod]
    public void FindRoot_ReturnsRepositoryRootForTestRun()
    {
        var root = RepositoryPaths.FindRoot();

        Assert.IsTrue(File.Exists(Path.Combine(root, "AlvorKit.slnx")));
    }

    /// <summary>Test project filters match project names and repository-relative paths.</summary>
    [TestMethod]
    public void TestProjects_WithFilters_ReturnsMatchingProjects()
    {
        using var workspace = TempWorkspace.Create();
        var first = workspace.WriteProject("tests", "One.Test", []);
        var second = workspace.WriteProject("tests", "Two.Test", []);

        var byName = ProjectDiscovery.TestProjects(workspace.Root, ["One.Test"]);
        var byPath = ProjectDiscovery.TestProjects(workspace.Root, ["tests/Two.Test/Two.Test.csproj"]);

        CollectionAssert.AreEqual(new[] { first }, byName.ToArray());
        CollectionAssert.AreEqual(new[] { second }, byPath.ToArray());
    }

    /// <summary>Shared helper projects under tests can opt out of coverage execution.</summary>
    [TestMethod]
    public void TestProjects_SkipsProjectsMarkedNotTest()
    {
        using var workspace = TempWorkspace.Create();
        var test = workspace.WriteProject("tests", "Tool.Test", []);
        workspace.WriteProject("tests", "AlvorKit.Testing", [], isTestProject: false);

        var projects = ProjectDiscovery.TestProjects(workspace.Root, []);

        CollectionAssert.AreEqual(new[] { test }, projects.ToArray());
    }

    /// <summary>Missing tests roots simply produce no runnable projects.</summary>
    [TestMethod]
    public void TestProjects_MissingTestsRoot_ReturnsEmpty()
    {
        using var workspace = TempWorkspace.Create();

        var projects = ProjectDiscovery.TestProjects(workspace.Root, []);

        Assert.AreEqual(0, projects.Count);
    }

    /// <summary>Source assembly discovery returns unique names from source and script roots.</summary>
    [TestMethod]
    public void SourceAssemblyNames_ReturnsDistinctNamesFromSourceRoots()
    {
        using var workspace = TempWorkspace.Create();
        workspace.WriteProject("src", "Shared", []);
        workspace.WriteProject("scripts", "Shared", []);
        workspace.WriteProject("scripts", "Tool", []);

        var modules = ProjectDiscovery.SourceAssemblyNames(workspace.Root);

        CollectionAssert.AreEqual(new[] { "Shared", "Tool" }, modules.ToArray());
    }

    /// <summary>Missing source roots simply produce no measured assemblies.</summary>
    [TestMethod]
    public void SourceAssemblyNames_MissingSourceRoots_ReturnsEmpty()
    {
        using var workspace = TempWorkspace.Create();

        var modules = ProjectDiscovery.SourceAssemblyNames(workspace.Root);

        Assert.AreEqual(0, modules.Count);
    }

    /// <summary>Targeted coverage includes transitive source project references only.</summary>
    [TestMethod]
    public void SourceAssemblyNamesForTests_ReturnsTransitiveSourceReferences()
    {
        using var workspace = TempWorkspace.Create();
        var core = workspace.WriteProject("scripts", "Core", []);
        var tool = workspace.WriteProject("scripts", "Tool", [core]);
        workspace.WriteProject("scripts", "Other", []);
        var test = workspace.WriteProject("tests", "Tool.Test", [tool]);

        var modules = ProjectReferenceDiscovery.SourceAssemblyNamesForTests(workspace.Root, [test]);

        CollectionAssert.AreEqual(new[] { "Core", "Tool" }, modules.ToArray());
    }
}
