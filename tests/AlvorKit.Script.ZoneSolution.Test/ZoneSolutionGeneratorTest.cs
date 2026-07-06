namespace AlvorKit.Script.ZoneSolution.Test;

/// <summary>Tests generated AlvorZone solution structure.</summary>
[TestClass]
public sealed class ZoneSolutionGeneratorTest
{
    /// <summary>Groups sibling repository projects under repository-named solution folders.</summary>
    [TestMethod]
    public void GenerateGroupsRepositoriesAndPreservesAttributes()
    {
        using var workspace = TempWorkspace.Create();
        var zoneRoot = workspace.CreateDirectory("AlvorZone");
        workspace.Write(
            "AlvorZone/AlvorKit/AlvorKit.slnx",
            """
            <Solution>
                <Folder Name="/Demos/">
                    <Project Path="demos/AlvorKit.Demo/AlvorKit.Demo.csproj" DefaultStartup="true" />
                </Folder>
                <Project Path="src/AlvorKit.Engine/AlvorKit.Engine.csproj" Id="engine-id" />
            </Solution>
            """);
        workspace.Write(
            "AlvorZone/AlvorPong/AlvorPong.slnx",
            """
            <Solution>
                <Project Path="src/AlvorPong/AlvorPong.csproj" DefaultStartup="true" />
                <Folder Name="/Tests/">
                    <Project Path="tests/AlvorPong.Test/AlvorPong.Test.csproj" Id="test-id" />
                </Folder>
            </Solution>
            """);
        workspace.Write("AlvorZone/AlvorPong/AlvorPong.Dev.slnx", "<Solution />");
        workspace.Write("AlvorZone/GeneratedOnly/GeneratedOnly.Dev.slnx", "<Solution />");
        var output = workspace.PathFor("AlvorZone/AlvorZone.slnx");

        var result = new ZoneSolutionGenerator().Generate(new(zoneRoot, output, [], ListOnly: false));

        Assert.IsTrue(result.Changed);
        Assert.AreEqual(2, result.RepositoryCount);
        Assert.AreEqual(4, result.ProjectCount);
        var document = XDocument.Load(output);
        var engineProject = AssertProject(document.Root!, "/AlvorKit/", "AlvorKit/src/AlvorKit.Engine/AlvorKit.Engine.csproj", "engine-id");
        var engineDemoProject = AssertProject(document.Root!, "/AlvorKit/Demos/", "AlvorKit/demos/AlvorKit.Demo/AlvorKit.Demo.csproj");
        var pongProject = AssertProject(document.Root!, "/AlvorPong/", "AlvorPong/src/AlvorPong/AlvorPong.csproj");
        AssertProject(document.Root!, "/AlvorPong/Tests/", "AlvorPong/tests/AlvorPong.Test/AlvorPong.Test.csproj", "test-id");
        Assert.IsNull(engineProject.Attribute("DefaultStartup"));
        Assert.AreEqual("true", engineDemoProject.Attribute("DefaultStartup")?.Value);
        Assert.AreEqual("true", pongProject.Attribute("DefaultStartup")?.Value);
        Assert.IsFalse(document.Root!.Elements("Folder").Any(folder => folder.Attribute("Name")?.Value == "/GeneratedOnly/"));
    }

    /// <summary>Repository filters preserve the requested order.</summary>
    [TestMethod]
    public void GenerateFiltersRepositoriesByName()
    {
        using var workspace = TempWorkspace.Create();
        var zoneRoot = workspace.CreateDirectory("AlvorZone");
        workspace.Write("AlvorZone/AlvorKit/AlvorKit.slnx", "<Solution><Project Path=\"src/AlvorKit/AlvorKit.csproj\" /></Solution>");
        workspace.Write("AlvorZone/Craftdig/Craftdig.slnx", "<Solution><Project Path=\"src/Craftdig/Craftdig.csproj\" /></Solution>");
        workspace.Write("AlvorZone/Rombadil/Rombadil.slnx", "<Solution><Project Path=\"src/Rombadil/Rombadil.csproj\" /></Solution>");
        var output = workspace.PathFor("AlvorZone/AlvorZone.slnx");

        var result = new ZoneSolutionGenerator().Generate(new(zoneRoot, output, ["Rombadil", "AlvorKit"], ListOnly: false));

        CollectionAssert.AreEqual(new[] { "Rombadil", "AlvorKit" }, result.Repositories.Select(repository => repository.Name).ToArray());
        var document = XDocument.Load(output);
        CollectionAssert.AreEqual(
            new[] { "/Rombadil/", "/AlvorKit/" },
            document.Root!.Elements("Folder").Select(folder => folder.Attribute("Name")!.Value).ToArray());
    }

    /// <summary>Leaves an existing output untouched when the generated content is unchanged.</summary>
    [TestMethod]
    public void GenerateReportsUnchangedWhenContentMatches()
    {
        using var workspace = TempWorkspace.Create();
        var zoneRoot = workspace.CreateDirectory("AlvorZone");
        workspace.Write("AlvorZone/AlvorKit/AlvorKit.slnx", "<Solution><Project Path=\"src/AlvorKit/AlvorKit.csproj\" /></Solution>");
        var output = workspace.PathFor("AlvorZone/AlvorZone.slnx");
        var generator = new ZoneSolutionGenerator();

        var first = generator.Generate(new(zoneRoot, output, [], ListOnly: false));
        var second = generator.Generate(new(zoneRoot, output, [], ListOnly: false));

        Assert.IsTrue(first.Changed);
        Assert.IsFalse(second.Changed);
    }

    /// <summary>Fails clearly when a requested repository is missing.</summary>
    [TestMethod]
    public void GenerateRejectsMissingRepoFilter()
    {
        using var workspace = TempWorkspace.Create();
        var zoneRoot = workspace.CreateDirectory("AlvorZone");
        workspace.Write("AlvorZone/AlvorKit/AlvorKit.slnx", "<Solution />");
        var output = workspace.PathFor("AlvorZone/AlvorZone.slnx");

        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => new ZoneSolutionGenerator().Generate(new(zoneRoot, output, ["Craftdig"], ListOnly: false)));

        StringAssert.Contains(exception.Message, "Repository 'Craftdig' was not found");
    }

    /// <summary>Asserts that a generated project appears in the expected solution folder.</summary>
    private static XElement AssertProject(XElement root, string folderName, string path, string? id = null)
    {
        var folder = root.Elements("Folder").Single(element => element.Attribute("Name")?.Value == folderName);
        var project = folder.Elements("Project").Single(element => element.Attribute("Path")?.Value == path);
        if (id is not null)
            Assert.AreEqual(id, project.Attribute("Id")?.Value);

        return project;
    }
}
