namespace AlvorKit;

/// <summary>Exercises evaluated membership, deterministic output, and invalid graph handling.</summary>
[TestClass]
[DoNotParallelize]
public class SolutionGeneratorTest
{
    /// <summary>Imports, conditions, and transitive dependencies determine membership, including generated projects.</summary>
    [TestMethod]
    public void EvaluatesImportedConditionalDependencies()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.CreateDirectory("Game");
        GitRepositoryFixture.Initialize(root);
        workspace.Write("Game/src/Game/Game.csproj", Project("""
            <Import Project="../../References.props" />
            <PropertyGroup><OutputType>Exe</OutputType></PropertyGroup>
            """));
        workspace.Write("Game/References.props", """
            <Project><ItemGroup Condition="'$(Configuration)' == 'Debug'">
              <ProjectReference Include="../../../AlvorKit/src/Engine/Engine.csproj" />
            </ItemGroup></Project>
            """);
        workspace.Write("AlvorKit/src/Engine/Engine.csproj", Project("""
            <ItemGroup><ProjectReference Include="../../out/bindgen/Binding.csproj" /></ItemGroup>
            """));
        workspace.Write("AlvorKit/out/bindgen/Binding.csproj", Project(""));
        workspace.Write("AlvorKit/demos/Unrelated.csproj", Project(""));
        var projects = SolutionGraph.Read(root, null);
        Assert.AreEqual(3, projects.Count);
        Assert.IsTrue(projects.Single(project => project.Path.EndsWith("Game.csproj")).Startup);
        Assert.IsFalse(projects.Single(project => project.Path.EndsWith("Binding.csproj")).Configurations.Contains("Release"));
        Assert.IsTrue(SolutionGenerator.Generate(root, false, null));
        var solution = RepositoryProjects.SolutionPath(root);
        var document = XDocument.Load(solution);
        Assert.AreEqual(3, document.Descendants("Project").Count());
        Assert.AreEqual(2, document.Descendants("Build").Count());
        var written = File.GetLastWriteTimeUtc(solution);
        Assert.IsTrue(SolutionGenerator.Generate(root, true, null));
        Assert.IsTrue(SolutionGenerator.Generate(root, false, null));
        Assert.AreEqual(written, File.GetLastWriteTimeUtc(solution));
    }

    /// <summary>A missing reference removes generated stale output while check mode remains read-only.</summary>
    [TestMethod]
    public void InvalidGraphDoesNotLeaveStaleSolution()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.CreateDirectory("Game");
        GitRepositoryFixture.Initialize(root);
        workspace.Write("Game/src/Game/Game.csproj", Project(""));
        Assert.IsFalse(SolutionGenerator.Generate(root, true, null));
        Assert.IsFalse(File.Exists(RepositoryProjects.SolutionPath(root)));
        SolutionGenerator.Generate(root, false, null);
        workspace.Write("Game/src/Game/Game.csproj", Project("""
            <ItemGroup><ProjectReference Include="Missing.csproj" /></ItemGroup>
            """));
        Assert.Throws<Exception>(() => SolutionGenerator.Generate(root, true, null));
        Assert.IsTrue(File.Exists(RepositoryProjects.SolutionPath(root)));
        Assert.Throws<Exception>(() => SolutionGenerator.Generate(root, false, null));
        Assert.IsFalse(File.Exists(RepositoryProjects.SolutionPath(root)));
    }

    /// <summary>Opt-out metadata excludes independent tooling roots but never hides a required dependency.</summary>
    [TestMethod]
    public void ExclusionsApplyOnlyToRootSelection()
    {
        using var workspace = TempWorkspace.Create();
        GitRepositoryFixture.Initialize(workspace.Root);
        workspace.Write("src/Game/Game.csproj", Project("""
            <ItemGroup><ProjectReference Include="../Required/Required.csproj" /></ItemGroup>
            """));
        workspace.Write("src/Required/Required.csproj", Project("""
            <PropertyGroup><WorkspaceProject>false</WorkspaceProject></PropertyGroup>
            """));
        workspace.Write("scripts/Hidden/Hidden.csproj", Project("""
            <PropertyGroup><WorkspaceProject>false</WorkspaceProject></PropertyGroup>
            """));
        Assert.AreEqual(2, SolutionGraph.Read(workspace.Root, null).Count);
    }

    /// <summary>Ignored generated projects enter through references, while unrelated ignored projects stay outside the graph.</summary>
    [TestMethod]
    public void IncludesReferencedIgnoredProjectsOnly()
    {
        using var workspace = TempWorkspace.Create();
        GitRepositoryFixture.Initialize(workspace.Root);
        workspace.Write(".gitignore", "src/Generated/\n");
        workspace.Write("src/Game/Game.csproj", Project("""
            <ItemGroup><ProjectReference Include="../Generated/Binding.csproj" /></ItemGroup>
            """));
        workspace.Write("src/Generated/Binding.csproj", Project(""));
        workspace.Write("src/Generated/Unused.csproj", "not a project");

        var projects = SolutionGraph.Read(workspace.Root, null);

        CollectionAssert.AreEquivalent(new[] { "Game.csproj", "Binding.csproj" },
            projects.Select(project => Path.GetFileName(project.Path)).ToArray());
    }

    /// <summary>Rejects contradictory startup selection instead of silently picking an executable.</summary>
    [TestMethod]
    public void RejectsAmbiguousStartup()
    {
        using var workspace = TempWorkspace.Create();
        GitRepositoryFixture.Initialize(workspace.Root);
        var executable = Project("""
            <PropertyGroup><OutputType>Exe</OutputType><WorkspaceStartup>true</WorkspaceStartup></PropertyGroup>
            """);
        workspace.Write("src/One/One.csproj", executable);
        workspace.Write("src/Two/Two.csproj", executable);
        Assert.ThrowsExactly<InvalidOperationException>(() => SolutionGraph.Read(workspace.Root, null));
    }

    /// <summary>Creates SDK fixtures without importing repository build configuration.</summary>
    internal static string Project(string body) =>
        "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework>" +
        "</PropertyGroup>" + body + "</Project>";

    /// <summary>Renaming a checkout replaces its old generated filename without leaving a second solution.</summary>
    [TestMethod]
    public void RepositoryRenameRemovesPreviousGeneratedName()
    {
        using var workspace = TempWorkspace.Create();
        var oldRoot = workspace.CreateDirectory("OldName");
        GitRepositoryFixture.Initialize(oldRoot);
        workspace.Write("OldName/src/Game/Game.csproj", Project(""));
        SolutionGenerator.Generate(oldRoot, false, null);
        var newRoot = Path.Combine(workspace.Root, "NewName");
        Directory.Move(oldRoot, newRoot);
        Assert.IsFalse(SolutionGenerator.Generate(newRoot, true, null));
        SolutionGenerator.Generate(newRoot, false, null);
        CollectionAssert.AreEqual(new[] { "NewName.slnx" },
            Directory.GetFiles(newRoot, "*.slnx").Select(Path.GetFileName).ToArray());
    }

    /// <summary>Existing authored solutions are rejected instead of being overwritten or treated as input.</summary>
    [TestMethod]
    public void RefusesToOverwriteAuthoredSolution()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("src/Game/Game.csproj", Project(""));
        var authored = workspace.Write("Game.slnx", "<Solution />");
        Assert.ThrowsExactly<InvalidOperationException>(() => SolutionGenerator.Generate(workspace.Root, false, null));
        Assert.AreEqual("<Solution />", File.ReadAllText(authored));
    }
}
