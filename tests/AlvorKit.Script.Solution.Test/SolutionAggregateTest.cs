namespace AlvorKit;

/// <summary>Verifies aggregate membership, configuration union, output ownership, and freshness.</summary>
[TestClass]
[DoNotParallelize]
public class SolutionAggregateTest
{
    /// <summary>Combines conditional shared dependencies once, groups by checkout, and preserves individual startup choices.</summary>
    [TestMethod]
    public void CombinesEvaluatedGraphsAndChecksFreshness()
    {
        using var workspace = TempWorkspace.Create();
        var engine = workspace.CreateDirectory("AlvorKit");
        var first = workspace.CreateDirectory("First");
        var second = workspace.CreateDirectory("Second");

        foreach (var root in new[] { engine, first, second })
            GitRepositoryFixture.Initialize(root);

        workspace.Write("AlvorKit/src/Engine/Engine.csproj", SolutionGeneratorTest.Project(""));
        workspace.Write("AlvorKit/.gitignore", "out/\n");
        workspace.Write("AlvorKit/out/Binding.csproj", SolutionGeneratorTest.Project(""));
        workspace.Write("First/src/First/First.csproj", SolutionGeneratorTest.Project("""
            <PropertyGroup><OutputType>Exe</OutputType></PropertyGroup>
            <ItemGroup Condition="'$(Configuration)' == 'Debug'">
              <ProjectReference Include="../../../AlvorKit/out/Binding.csproj" />
              <ProjectReference Include="../../../External/DebugOnly.csproj" />
            </ItemGroup>
            """));
        workspace.Write("Second/src/Second/Second.csproj", SolutionGeneratorTest.Project("""
            <PropertyGroup><OutputType>Exe</OutputType></PropertyGroup>
            <ItemGroup Condition="'$(Configuration)' == 'Release'">
              <ProjectReference Include="../../../AlvorKit/out/Binding.csproj" />
            </ItemGroup>
            """));
        workspace.Write("External/DebugOnly.csproj", SolutionGeneratorTest.Project(""));
        var output = Path.Combine(workspace.Root, "Workspace.slnx");
        var options = SolutionOptions.Create([], workspace.Root, false, false, false, output);
        Assert.IsTrue(SolutionGenerator.Generate(options));
        var document = XDocument.Load(output);
        var projects = document.Descendants("Project").ToArray();
        Assert.AreEqual(5, projects.Length);
        var binding = projects.Single(project => project.Attribute("Path")!.Value == "AlvorKit/out/Binding.csproj");
        Assert.AreEqual("/AlvorKit/Out/", binding.Parent!.Attribute("Name")!.Value);
        Assert.IsFalse(binding.Elements("Build").Any());
        var conditional = projects.Single(project => project.Attribute("Path")!.Value == "External/DebugOnly.csproj");
        Assert.AreEqual("/Dependencies/", conditional.Parent!.Attribute("Name")!.Value);
        Assert.AreEqual("Release|*", conditional.Element("Build")!.Attribute("Solution")!.Value);
        Assert.IsFalse(projects.Any(project => project.Attribute("DefaultStartup") != null));

        foreach (var root in new[] { first, second })
        {
            Assert.AreEqual(1, XDocument.Load(RepositoryProjects.SolutionPath(root)).Descendants("Project")
                .Count(project => project.Attribute("DefaultStartup")?.Value == "true"));
        }

        var content = File.ReadAllText(output);
        var written = File.GetLastWriteTimeUtc(output);
        Assert.IsTrue(SolutionGenerator.Generate(options with { Check = true }));
        Assert.IsTrue(SolutionGenerator.Generate(options));
        Assert.AreEqual(written, File.GetLastWriteTimeUtc(output));
        var reversed = options with { ParentDirectory = null, RepositoryRoots = [second, first, engine] };
        Assert.IsTrue(SolutionGenerator.Generate(reversed));
        Assert.AreEqual(content, File.ReadAllText(output));

        File.AppendAllText(output, "\n");
        Assert.IsFalse(SolutionGenerator.Generate(options with { Check = true }));
        Assert.AreEqual(content + "\n", File.ReadAllText(output));
        Assert.IsTrue(SolutionGenerator.Generate(options));
        Assert.AreEqual(content, File.ReadAllText(output));
    }

    /// <summary>Invalid graphs remove the aggregate while check mode preserves existing output.</summary>
    [TestMethod]
    public void InvalidGraphInvalidatesAggregateAndRecovers()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.CreateDirectory("Game");
        GitRepositoryFixture.Initialize(root);
        var project = workspace.Write("Game/src/Game.csproj", SolutionGeneratorTest.Project(""));
        var output = Path.Combine(workspace.Root, "Workspace.slnx");
        var options = SolutionOptions.Create([root], null, false, false, false, output);
        Assert.IsTrue(SolutionGenerator.Generate(options));
        var content = File.ReadAllText(output);
        File.WriteAllText(project, SolutionGeneratorTest.Project(
            "<ItemGroup><ProjectReference Include=\"Missing.csproj\" /></ItemGroup>"));
        Assert.Throws<Exception>(() => SolutionGenerator.Generate(options with { Check = true }));
        Assert.AreEqual(content, File.ReadAllText(output));
        Assert.Throws<Exception>(() => SolutionGenerator.Generate(options));
        Assert.IsFalse(File.Exists(output));
        File.WriteAllText(project, SolutionGeneratorTest.Project(""));
        Assert.IsTrue(SolutionGenerator.Generate(options));
        Assert.AreEqual(content, File.ReadAllText(output));
    }

    /// <summary>Rejects authored output and repository-local aggregate paths before changing any solution.</summary>
    [TestMethod]
    public void ProtectsAuthoredAndRepositorySolutions()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.CreateDirectory("Game");
        GitRepositoryFixture.Initialize(root);
        workspace.Write("Game/src/Game.csproj", SolutionGeneratorTest.Project(""));
        var output = workspace.Write("Workspace.slnx", "<Solution />");
        var options = SolutionOptions.Create([root], null, false, false, false, output);
        Assert.ThrowsExactly<InvalidOperationException>(() => SolutionGenerator.Generate(options));
        Assert.AreEqual("<Solution />", File.ReadAllText(output));
        Assert.IsFalse(File.Exists(RepositoryProjects.SolutionPath(root)));
        SolutionGenerator.Generate(root, false, null);
        var individual = RepositoryProjects.SolutionPath(root);
        var content = File.ReadAllText(individual);
        Assert.ThrowsExactly<ArgumentException>(() => SolutionGenerator.Generate(options with { AggregateSolution = individual }));
        Assert.AreEqual(content, File.ReadAllText(individual));
    }

    /// <summary>Clears removed repositories from the aggregate, including when no managed checkout remains.</summary>
    [TestMethod]
    public void EmptyDiscoveryPublishesEmptyAggregate()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.CreateDirectory("Game");
        GitRepositoryFixture.Initialize(root);
        workspace.Write("Game/src/Game.csproj", SolutionGeneratorTest.Project(
            "<PropertyGroup><OutputType>Exe</OutputType></PropertyGroup>"));
        var output = Path.Combine(workspace.Root, "Workspace.slnx");
        var options = SolutionOptions.Create([], workspace.Root, false, false, false, output);
        Assert.IsTrue(SolutionGenerator.Generate(options));
        Assert.AreEqual("true", XDocument.Load(output).Descendants("Project").Single().Attribute("DefaultStartup")!.Value);
        Directory.Delete(root, true);
        Assert.IsFalse(SolutionGenerator.Generate(options with { Check = true }));
        Assert.IsTrue(SolutionGenerator.Generate(options));
        Assert.IsFalse(XDocument.Load(output).Descendants("Project").Any());
    }

    /// <summary>A non-repository output directory below the discovery parent remains a supported aggregate location.</summary>
    [TestMethod]
    public void AllowsSeparateOutputDirectory()
    {
        using var workspace = TempWorkspace.Create();
        var output = Path.Combine(workspace.CreateDirectory("Solutions"), "Workspace.slnx");
        var options = SolutionOptions.Create([], workspace.Root, false, false, false, output);
        Assert.IsTrue(SolutionGenerator.Generate(options));
        Assert.IsFalse(XDocument.Load(output).Descendants("Project").Any());
    }
}
