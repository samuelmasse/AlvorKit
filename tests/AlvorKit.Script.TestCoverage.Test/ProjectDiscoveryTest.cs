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

    /// <summary>Source project filters match source project names, files, and directories.</summary>
    [TestMethod]
    public void SourceAssemblyNames_WithFilters_ReturnsMatchingProjects()
    {
        using var workspace = TempWorkspace.Create();
        workspace.WriteProject("scripts", "Tool", []);
        workspace.WriteProject("scripts", "Other", []);

        var byName = ProjectDiscovery.SourceAssemblyNames(workspace.Root, ["Tool"]);
        var byFile = ProjectDiscovery.SourceAssemblyNames(workspace.Root, ["scripts/Other/Other.csproj"]);
        var byDirectory = ProjectDiscovery.SourceAssemblyNames(workspace.Root, ["scripts/Tool"]);

        CollectionAssert.AreEqual(new[] { "Tool" }, byName.ToArray());
        CollectionAssert.AreEqual(new[] { "Other" }, byFile.ToArray());
        CollectionAssert.AreEqual(new[] { "Tool" }, byDirectory.ToArray());
    }

    /// <summary>An empty source project filter returns all discovered source projects.</summary>
    [TestMethod]
    public void SourceAssemblyNames_WithEmptyFilters_ReturnsAllProjects()
    {
        using var workspace = TempWorkspace.Create();
        workspace.WriteProject("scripts", "Tool", []);

        var modules = ProjectDiscovery.SourceAssemblyNames(workspace.Root, []);

        CollectionAssert.AreEqual(new[] { "Tool" }, modules.ToArray());
    }

    /// <summary>Binding source discovery reads API and backend modules from native bindgen configuration.</summary>
    [TestMethod]
    public void BindingSourceAssemblyNames_WithFilter_ReturnsApiAndBackendModules()
    {
        using var workspace = TempWorkspace.Create();
        WriteBindingConfig(workspace, "xxhash", "AlvorKit.XxHash");
        WriteBindingConfig(workspace, "freetype", "AlvorKit.FreeType");

        var modules = BindingProjectDiscovery.SourceAssemblyNames(workspace.Root, ["xxhash"]);

        CollectionAssert.AreEqual(new[] { "AlvorKit.XxHash", "AlvorKit.XxHash.Backend" }, modules.ToArray());
    }

    /// <summary>Binding source discovery returns no modules when the repository has no native directory.</summary>
    [TestMethod]
    public void BindingSourceAssemblyNames_MissingNativeRoot_ReturnsEmpty()
    {
        using var workspace = TempWorkspace.Create();

        var modules = BindingProjectDiscovery.SourceAssemblyNames(workspace.Root, ["xxhash"]);

        Assert.AreEqual(0, modules.Count);
    }

    /// <summary>Binding filters can match config paths, native directories, and generated assembly names.</summary>
    [TestMethod]
    public void BindingSourceAssemblyNames_WithAlternateFilters_ReturnsMatchingModules()
    {
        using var workspace = TempWorkspace.Create();
        WriteBindingConfig(workspace, "xxhash", "AlvorKit.XxHash");

        var byConfig = BindingProjectDiscovery.SourceAssemblyNames(workspace.Root, ["native/xxhash/conf/bindgen.yml"]);
        var byDirectory = BindingProjectDiscovery.SourceAssemblyNames(workspace.Root, ["native/xxhash"]);
        var byAssembly = BindingProjectDiscovery.SourceAssemblyNames(workspace.Root, ["AlvorKit.XxHash.Backend"]);

        CollectionAssert.AreEqual(new[] { "AlvorKit.XxHash", "AlvorKit.XxHash.Backend" }, byConfig.ToArray());
        CollectionAssert.AreEqual(new[] { "AlvorKit.XxHash", "AlvorKit.XxHash.Backend" }, byDirectory.ToArray());
        CollectionAssert.AreEqual(new[] { "AlvorKit.XxHash", "AlvorKit.XxHash.Backend" }, byAssembly.ToArray());
    }

    /// <summary>Binding source discovery without filters returns every configured binding module.</summary>
    [TestMethod]
    public void BindingSourceAssemblyNames_WithEmptyFilters_ReturnsAllModules()
    {
        using var workspace = TempWorkspace.Create();
        WriteBindingConfig(workspace, "xxhash", "AlvorKit.XxHash");
        WriteBindingConfig(workspace, "freetype", "AlvorKit.FreeType");

        var modules = BindingProjectDiscovery.SourceAssemblyNames(workspace.Root, []);

        CollectionAssert.AreEqual(
            new[] { "AlvorKit.FreeType", "AlvorKit.FreeType.Backend", "AlvorKit.XxHash", "AlvorKit.XxHash.Backend" },
            modules.ToArray());
    }

    /// <summary>Binding source discovery tolerates configs that omit one generated project path.</summary>
    [TestMethod]
    public void BindingSourceAssemblyNames_WithPartialConfig_ReturnsPresentModules()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write(
            "native/partial/conf/bindgen.yml",
            """
            apiProject: out/bindgen/AlvorKit.Partial
            """);

        var modules = BindingProjectDiscovery.SourceAssemblyNames(workspace.Root, ["partial"]);

        CollectionAssert.AreEqual(new[] { "AlvorKit.Partial" }, modules.ToArray());
    }

    /// <summary>Binding test discovery matches package and generated project references by module name.</summary>
    [TestMethod]
    public void TestProjectsReferencingBindingModules_ReturnsMatchingTests()
    {
        using var workspace = TempWorkspace.Create();
        var packageTest = WriteBindingTestProject(workspace, "Package.Test", packageReference: "AlvorKit.XxHash.Backend");
        var projectTest = WriteBindingTestProject(
            workspace,
            "Project.Test",
            projectReference: "$(BindingsRoot)\\AlvorKit.XxHash.Backend\\AlvorKit.XxHash.Backend.csproj");
        WriteBindingTestProject(workspace, "Other.Test", packageReference: "AlvorKit.FreeType.Backend");
        var tests = ProjectDiscovery.TestProjects(workspace.Root, []);

        var selected = BindingProjectDiscovery.TestProjectsReferencingBindingModules(tests, ["AlvorKit.XxHash", "AlvorKit.XxHash.Backend"]);

        CollectionAssert.AreEqual(new[] { packageTest, projectTest }, selected.ToArray());
    }

    /// <summary>Binding test discovery ignores missing projects and references without an Include value.</summary>
    [TestMethod]
    public void TestProjectsReferencingBindingModules_IgnoresMissingAndEmptyReferences()
    {
        using var workspace = TempWorkspace.Create();
        var test = workspace.Write(
            "tests/Empty.Test/Empty.Test.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference />
                <ProjectReference />
              </ItemGroup>
            </Project>
            """);

        var selected = BindingProjectDiscovery.TestProjectsReferencingBindingModules(
            [test, Path.Combine(workspace.Root, "tests", "Missing.Test", "Missing.Test.csproj")],
            ["AlvorKit.XxHash.Backend"]);

        Assert.AreEqual(0, selected.Count);
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

    /// <summary>Missing referenced test projects are ignored during source reference discovery.</summary>
    [TestMethod]
    public void SourceAssemblyNamesForTests_IgnoresMissingTestProjects()
    {
        using var workspace = TempWorkspace.Create();

        var modules = ProjectReferenceDiscovery.SourceAssemblyNamesForTests(workspace.Root, ["tests/Missing/Missing.csproj"]);

        Assert.AreEqual(0, modules.Count);
    }

    /// <summary>Duplicate test projects are visited once during source reference discovery.</summary>
    [TestMethod]
    public void SourceAssemblyNamesForTests_IgnoresDuplicateTestProjects()
    {
        using var workspace = TempWorkspace.Create();
        var tool = workspace.WriteProject("scripts", "Tool", []);
        var test = workspace.WriteProject("tests", "Tool.Test", [tool]);

        var modules = ProjectReferenceDiscovery.SourceAssemblyNamesForTests(workspace.Root, [test, test]);

        CollectionAssert.AreEqual(new[] { "Tool" }, modules.ToArray());
    }

    /// <summary>Project references without an Include attribute are ignored.</summary>
    [TestMethod]
    public void SourceAssemblyNamesForTests_IgnoresProjectReferencesWithoutInclude()
    {
        using var workspace = TempWorkspace.Create();
        var test = workspace.Write(
            "tests/Tool.Test/Tool.Test.csproj",
            "<Project><ItemGroup><ProjectReference /></ItemGroup></Project>");

        var modules = ProjectReferenceDiscovery.SourceAssemblyNamesForTests(workspace.Root, [test]);

        Assert.AreEqual(0, modules.Count);
    }

    /// <summary>Source-scoped coverage can discover tests that reference selected modules.</summary>
    [TestMethod]
    public void TestProjectsReferencingSourceModules_ReturnsReferencingTests()
    {
        using var workspace = TempWorkspace.Create();
        var tool = workspace.WriteProject("scripts", "Tool", []);
        workspace.WriteProject("scripts", "Other", []);
        var toolTest = workspace.WriteProject("tests", "Tool.Test", [tool]);
        workspace.WriteProject("tests", "Other.Test", []);
        var tests = ProjectDiscovery.TestProjects(workspace.Root, []);

        var selected = ProjectReferenceDiscovery.TestProjectsReferencingSourceModules(workspace.Root, tests, ["Tool"]);

        CollectionAssert.AreEqual(new[] { toolTest }, selected.ToArray());
    }

    /// <summary>Coverage selection gates source-filtered runs only on selected source modules.</summary>
    [TestMethod]
    public void CoverageSelection_WithSourceFilter_SelectsReferencingTestsAndSourceOnly()
    {
        using var workspace = TempWorkspace.Create();
        var shared = workspace.WriteProject("scripts", "Shared", []);
        var tool = workspace.WriteProject("scripts", "Tool", [shared]);
        workspace.WriteProject("tests", "Tool.Test", [tool]);

        var options = CoverageOptions.Parse(["--source-project", "Tool"]);
        var selection = CoverageSelection.FromOptions(workspace.Root, options);

        Assert.AreEqual(1, selection.TestProjects.Count);
        CollectionAssert.AreEqual(new[] { "Tool" }, selection.SourceModules.ToArray());
    }

    /// <summary>Coverage selection with a binding filter gates generated API and backend modules and finds matching tests.</summary>
    [TestMethod]
    public void CoverageSelection_WithBindingFilter_SelectsBindingTestsAndModules()
    {
        using var workspace = TempWorkspace.Create();
        WriteBindingConfig(workspace, "xxhash", "AlvorKit.XxHash");
        var test = WriteBindingTestProject(workspace, "AlvorKit.XxHash.Test", packageReference: "AlvorKit.XxHash.Backend");

        var options = CoverageOptions.Parse(["--binding", "xxhash"]);
        var selection = CoverageSelection.FromOptions(workspace.Root, options);

        CollectionAssert.AreEqual(new[] { test }, selection.TestProjects.ToArray());
        CollectionAssert.AreEqual(new[] { "AlvorKit.XxHash", "AlvorKit.XxHash.Backend" }, selection.SourceModules.ToArray());
    }

    /// <summary>Coverage selection without filters includes all tests and source modules.</summary>
    [TestMethod]
    public void CoverageSelection_WithoutFilters_SelectsAllTestsAndSources()
    {
        using var workspace = TempWorkspace.Create();
        workspace.WriteProject("scripts", "Tool", []);
        workspace.WriteProject("tests", "Tool.Test", []);

        var selection = CoverageSelection.FromOptions(workspace.Root, CoverageOptions.Parse([]));

        Assert.AreEqual(1, selection.TestProjects.Count);
        CollectionAssert.AreEqual(new[] { "Tool" }, selection.SourceModules.ToArray());
    }

    /// <summary>Coverage selection without source filters gates modules referenced by selected tests.</summary>
    [TestMethod]
    public void CoverageSelection_WithTestFilter_SelectsReferencedSources()
    {
        using var workspace = TempWorkspace.Create();
        var tool = workspace.WriteProject("scripts", "Tool", []);
        workspace.WriteProject("scripts", "Other", []);
        workspace.WriteProject("tests", "Tool.Test", [tool]);

        var options = CoverageOptions.Parse(["--test-project", "Tool.Test"]);
        var selection = CoverageSelection.FromOptions(workspace.Root, options);

        CollectionAssert.AreEqual(new[] { "Tool" }, selection.SourceModules.ToArray());
    }

    /// <summary>Coverage selection can combine explicit test and source filters.</summary>
    [TestMethod]
    public void CoverageSelection_WithTestAndSourceFilters_UsesBoth()
    {
        using var workspace = TempWorkspace.Create();
        var tool = workspace.WriteProject("scripts", "Tool", []);
        workspace.WriteProject("tests", "Tool.Test", [tool]);

        var options = CoverageOptions.Parse(["--test-project", "Tool.Test", "--source-project", "Tool"]);
        var selection = CoverageSelection.FromOptions(workspace.Root, options);

        Assert.AreEqual(1, selection.TestProjects.Count);
        CollectionAssert.AreEqual(new[] { "Tool" }, selection.SourceModules.ToArray());
    }

    /// <summary>Coverage selection fails clearly when filters match no projects.</summary>
    [TestMethod]
    public void CoverageSelection_WithMissingSourceFilter_Throws()
    {
        using var workspace = TempWorkspace.Create();
        workspace.WriteProject("tests", "Tool.Test", []);

        var options = CoverageOptions.Parse(["--source-project", "Missing"]);

        Assert.ThrowsException<InvalidOperationException>(() => CoverageSelection.FromOptions(workspace.Root, options));
    }

    /// <summary>Coverage selection fails clearly when selected tests reference no source modules.</summary>
    [TestMethod]
    public void CoverageSelection_WithNoSourceModules_Throws()
    {
        using var workspace = TempWorkspace.Create();
        workspace.WriteProject("tests", "Tool.Test", []);

        var options = CoverageOptions.Parse(["--test-project", "Tool.Test"]);

        Assert.ThrowsException<InvalidOperationException>(() => CoverageSelection.FromOptions(workspace.Root, options));
    }

    /// <summary>Writes a minimal native bindgen configuration for generated binding discovery tests.</summary>
    private static void WriteBindingConfig(TempWorkspace workspace, string name, string apiAssembly) =>
        workspace.Write(
            $"native/{name}/conf/bindgen.yml",
            $$"""
            apiProject: out/bindgen/{{apiAssembly}}
            backendProject: out/bindgen/{{apiAssembly}}.Backend
            """);

    /// <summary>Writes a test project with optional binding package and project references.</summary>
    private static string WriteBindingTestProject(
        TempWorkspace workspace,
        string name,
        string? packageReference = null,
        string? projectReference = null)
    {
        var items = new List<string>();
        if (packageReference is not null)
            items.Add($"""<PackageReference Include="{packageReference}" />""");
        if (projectReference is not null)
            items.Add($"""<ProjectReference Include="{projectReference}" />""");

        return workspace.Write(
            $"tests/{name}/{name}.csproj",
            $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                {{string.Join(Environment.NewLine + "    ", items)}}
              </ItemGroup>
            </Project>
            """);
    }
}
