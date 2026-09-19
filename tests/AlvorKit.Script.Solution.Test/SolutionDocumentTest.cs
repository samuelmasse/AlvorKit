namespace AlvorKit;

/// <summary>Verifies folder presentation and conditional build mappings without changing project paths.</summary>
[TestClass]
public class SolutionDocumentTest
{
    /// <summary>Local and engine source is flattened; other areas retain capitalized directory labels.</summary>
    [TestMethod]
    public void GroupsProjectsWithoutChangingTheirPaths()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.CreateDirectory("Game");
        var game = Path.Combine(root, "src/Game/Game.csproj");
        var engine = Path.Combine(workspace.Root, "AlvorKit/src/Engine/Engine.csproj");
        var test = Path.Combine(root, "tests/Game.Test/Game.Test.csproj");
        var dependency = Path.Combine(workspace.Root, "External/Shared.csproj");
        var projects = new SolutionProject[]
        {
            new(game, true, true, new HashSet<string> { "Debug", "Release" }),
            new(engine, false, false, new HashSet<string> { "Debug", "Release" }),
            new(test, false, false, new HashSet<string> { "Debug", "Release" }),
            new(dependency, false, false, new HashSet<string> { "Debug" }),
        };
        var document = XDocument.Parse(SolutionDocument.Format(root, projects));
        var solution = document.Root!;

        Assert.AreEqual("src/Game/Game.csproj", solution.Element("Project")!.Attribute("Path")!.Value);
        Assert.AreEqual("true", solution.Element("Project")!.Attribute("DefaultStartup")!.Value);
        var folders = solution.Elements("Folder").ToDictionary(folder => folder.Attribute("Name")!.Value);
        CollectionAssert.AreEquivalent(new[] { "/Engine/", "/Tests/", "/Dependencies/" }, folders.Keys.ToArray());
        Assert.AreEqual("../AlvorKit/src/Engine/Engine.csproj", folders["/Engine/"].Element("Project")!.Attribute("Path")!.Value);
        Assert.AreEqual("tests/Game.Test/Game.Test.csproj", folders["/Tests/"].Element("Project")!.Attribute("Path")!.Value);
        var conditional = folders["/Dependencies/"].Element("Project")!;
        Assert.AreEqual("Release|*", conditional.Element("Build")!.Attribute("Solution")!.Value);
        Assert.AreEqual("false", conditional.Element("Build")!.Attribute("Project")!.Value);
        Assert.AreEqual(SolutionDocument.Format(root, projects), SolutionDocument.Format(root, projects.Reverse().ToArray()));
    }

    /// <summary>Nested demo folders retain hierarchy, including a project placed directly in its area.</summary>
    [TestMethod]
    public void PreservesNestedAreaFolders()
    {
        using var workspace = TempWorkspace.Create();
        var project = new SolutionProject(Path.Combine(workspace.Root, "demos/world/Preview.csproj"),
            false, true, new HashSet<string> { "Debug", "Release" });
        var document = XDocument.Parse(SolutionDocument.Format(workspace.Root, [project]));

        CollectionAssert.AreEqual(new[] { "/Demos/", "/Demos/World/" },
            document.Descendants("Folder").Select(folder => folder.Attribute("Name")!.Value).ToArray());
        Assert.IsNull(document.Descendants("Project").Single().Attribute("DefaultStartup"));
    }
}
