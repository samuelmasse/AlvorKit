namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Tests for discovering targeted test projects and their source modules.</summary>
[TestClass]
public sealed class ProjectDiscoveryTest
{
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
