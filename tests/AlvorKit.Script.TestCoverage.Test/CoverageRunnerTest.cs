namespace AlvorKit.Script.TestCoverage.Test;

/// <summary>Integration tests for the coverage workflow coordinator.</summary>
[TestClass]
public sealed class CoverageRunnerTest
{
    /// <summary>The runner writes isolated reports through the normal coverage workflow.</summary>
    [TestMethod]
    public async Task RunAsync_FocusedWorkspaceRun_WritesRunScopedReports()
    {
        using var workspace = TempWorkspace.Create();
        using var fixture = CoverageRunnerFixture.Create(workspace, "Focused");
        var outputRoot = workspace.PathFor("coverage-output");
        var options = CoverageOptions.Parse(
        [
            "--agent",
            "--threshold",
            "0",
            "--max-parallel",
            "1",
            "--source-project",
            fixture.SourceProjectName,
            "--test-project",
            fixture.TestProjectName,
            "--output-root",
            outputRoot,
            "--run-id",
            "nested",
        ]);

        var exitCode = await new CoverageRunner(options).RunAsync();
        var runRoot = Path.Combine(outputRoot, "runs", "nested");

        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "coverage-summary.json")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "coverage-summary.md")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "projects", fixture.TestProjectName, "coverage.json")));
    }

    /// <summary>Multiple selected test projects use the prebuild path and can still generate HTML reports.</summary>
    [TestMethod]
    public async Task RunAsync_MultipleProjectsWithHtml_WritesHtmlReport()
    {
        using var workspace = TempWorkspace.Create();
        using var firstFixture = CoverageRunnerFixture.Create(workspace, "HtmlOne");
        using var secondFixture = CoverageRunnerFixture.Create(workspace, "HtmlTwo");
        var outputRoot = workspace.PathFor("coverage-output");
        var options = CoverageOptions.Parse(
        [
            "--threshold",
            "0",
            "--no-lcov",
            "--test-project",
            firstFixture.TestProjectName,
            "--test-project",
            secondFixture.TestProjectName,
            "--output-root",
            outputRoot,
            "--run-id",
            "prebuild-html",
        ]);

        var exitCode = await new CoverageRunner(options).RunAsync();
        var runRoot = Path.Combine(outputRoot, "runs", "prebuild-html");

        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "html", "index.html")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "projects", firstFixture.TestProjectName, "dotnet-build.log")));
        Assert.IsTrue(File.Exists(Path.Combine(runRoot, "projects", secondFixture.TestProjectName, "dotnet-build.log")));
    }

    /// <summary>A prebuild failure stops before test execution but still writes diagnostic reports.</summary>
    [TestMethod]
    public async Task RunAsync_FailedPrebuild_WritesFailureReport()
    {
        using var workspace = TempWorkspace.Create();
        using var fixture = CoverageRunnerFixture.Create(workspace, "FailedPrebuild");
        var repoRoot = RepositoryPaths.FindRoot();
        var badProjectName = CoverageRunnerFixture.UniqueProjectName("BrokenCoverageRunner");
        var badProjectRoot = Path.Combine(
            repoRoot,
            "tests",
            ".coverage-runner-temp",
            badProjectName);
        var badProject = Path.Combine(badProjectRoot, badProjectName + ".csproj");
        var outputRoot = workspace.PathFor("coverage-output");

        try
        {
            Directory.CreateDirectory(badProjectRoot);
            var badProjectBin = CoverageRunnerFixture.CreateBuildPath(workspace, badProjectName, "bin");
            var badProjectObj = CoverageRunnerFixture.CreateBuildPath(workspace, badProjectName, "obj");
            File.WriteAllText(
                badProject,
                $$"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <BaseOutputPath>{{badProjectBin}}/</BaseOutputPath>
                    <BaseIntermediateOutputPath>{{badProjectObj}}/</BaseIntermediateOutputPath>
                  </PropertyGroup>
                </Project>
                """);
            File.WriteAllText(Path.Combine(badProjectRoot, "Broken.cs"), "public class Broken {");

            var options = CoverageOptions.Parse(
            [
                "--agent",
                "--threshold",
                "0",
                "--test-project",
                RepositoryPaths.Relative(repoRoot, badProject),
                "--test-project",
                fixture.TestProjectName,
                "--output-root",
                outputRoot,
                "--run-id",
                "failed-prebuild",
            ]);

            var exitCode = await new CoverageRunner(options).RunAsync();
            var runRoot = Path.Combine(outputRoot, "runs", "failed-prebuild");

            Assert.AreEqual(1, exitCode);
            Assert.IsTrue(File.Exists(Path.Combine(runRoot, "coverage-summary.json")));
            Assert.IsFalse(File.Exists(Path.Combine(runRoot, "projects", badProjectName, "dotnet-test.log")));
        }
        finally
        {
            if (Directory.Exists(badProjectRoot))
                Directory.Delete(badProjectRoot, recursive: true);
        }
    }

    /// <summary>Creates isolated projects for nested coverage runner integration tests.</summary>
    private sealed class CoverageRunnerFixture : IDisposable
    {
        private readonly string sourceRoot;
        private readonly string testRoot;

        private CoverageRunnerFixture(string sourceProjectName, string testProjectName, string sourceRoot, string testRoot)
        {
            SourceProjectName = sourceProjectName;
            TestProjectName = testProjectName;
            this.sourceRoot = sourceRoot;
            this.testRoot = testRoot;
        }

        /// <summary>Project name for the disposable source module.</summary>
        public string SourceProjectName { get; }

        /// <summary>Project name for the disposable test module.</summary>
        public string TestProjectName { get; }

        /// <summary>Creates a source/test project pair under the repository so normal discovery can find it.</summary>
        public static CoverageRunnerFixture Create(TempWorkspace workspace, string name)
        {
            var repoRoot = RepositoryPaths.FindRoot();
            var sourceProjectName = UniqueProjectName("CoverageRunnerFixture" + name);
            var testProjectName = sourceProjectName + ".Test";
            var sourceRoot = Path.Combine(repoRoot, "scripts", ".coverage-runner-temp", sourceProjectName);
            var testRoot = Path.Combine(repoRoot, "tests", ".coverage-runner-temp", testProjectName);

            Directory.CreateDirectory(sourceRoot);
            Directory.CreateDirectory(testRoot);
            WriteSourceProject(workspace, sourceRoot, sourceProjectName);
            WriteTestProject(workspace, testRoot, sourceRoot, sourceProjectName, testProjectName);

            return new(sourceProjectName, testProjectName, sourceRoot, testRoot);
        }

        /// <summary>Builds a unique MSBuild-safe project name.</summary>
        public static string UniqueProjectName(string prefix) =>
            prefix + Guid.NewGuid().ToString("N");

        /// <summary>Returns an absolute path formatted for MSBuild property values.</summary>
        public static string MsBuildPath(string path) =>
            Path.GetFullPath(path).Replace('\\', '/');

        /// <summary>Creates and returns an isolated build directory formatted for MSBuild.</summary>
        public static string CreateBuildPath(TempWorkspace workspace, string projectName, string name) =>
            MsBuildPath(workspace.CreateDirectory("build", projectName, name));

        /// <summary>Deletes the disposable project directories after the nested run finishes.</summary>
        public void Dispose()
        {
            DeleteIfExists(sourceRoot);
            DeleteIfExists(testRoot);
        }

        /// <summary>Writes the source project and a tiny covered API.</summary>
        private static void WriteSourceProject(TempWorkspace workspace, string sourceRoot, string sourceProjectName)
        {
            File.WriteAllText(
                Path.Combine(sourceRoot, sourceProjectName + ".csproj"),
                $$"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <OutputType>Library</OutputType>
                    <BaseOutputPath>{{CreateBuildPath(workspace, sourceProjectName, "bin")}}/</BaseOutputPath>
                    <BaseIntermediateOutputPath>{{CreateBuildPath(workspace, sourceProjectName, "obj")}}/</BaseIntermediateOutputPath>
                  </PropertyGroup>
                </Project>
                """);

            File.WriteAllText(
                Path.Combine(sourceRoot, "Calculator.cs"),
                $$"""
                namespace {{sourceProjectName}};

                /// <summary>Small fixture API measured by coverage runner integration tests.</summary>
                public static class Calculator
                {
                    /// <summary>Returns the sum of two integers.</summary>
                    public static int Add(int left, int right) => left + right;
                }
                """);
        }

        /// <summary>Writes the test project that exercises the source fixture.</summary>
        private static void WriteTestProject(
            TempWorkspace workspace,
            string testRoot,
            string sourceRoot,
            string sourceProjectName,
            string testProjectName)
        {
            var sourceProject = Path.Combine(sourceRoot, sourceProjectName + ".csproj");
            File.WriteAllText(
                Path.Combine(testRoot, testProjectName + ".csproj"),
                $$"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <BaseOutputPath>{{CreateBuildPath(workspace, testProjectName, "bin")}}/</BaseOutputPath>
                    <BaseIntermediateOutputPath>{{CreateBuildPath(workspace, testProjectName, "obj")}}/</BaseIntermediateOutputPath>
                  </PropertyGroup>

                  <ItemGroup>
                    <ProjectReference Include="{{MsBuildPath(Path.GetRelativePath(testRoot, sourceProject))}}" />
                  </ItemGroup>

                  <ItemGroup>
                    <Using Include="{{sourceProjectName}}" />
                  </ItemGroup>
                </Project>
                """);

            File.WriteAllText(
                Path.Combine(testRoot, "CalculatorTest.cs"),
                $$"""
                namespace {{testProjectName.Replace('.', '_')}};

                /// <summary>Tests the disposable coverage source fixture.</summary>
                [TestClass]
                public sealed class CalculatorTest
                {
                    /// <summary>The covered method returns the expected sum.</summary>
                    [TestMethod]
                    public void Add_ReturnsSum()
                    {
                        Assert.AreEqual(5, Calculator.Add(2, 3));
                    }
                }
                """);
        }

        /// <summary>Deletes a directory when it exists.</summary>
        private static void DeleteIfExists(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
    }
}
