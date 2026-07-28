using System.Reflection;

namespace AlvorKit.Script.TestInterception;

/// <summary>Accepts repeated profiled VSTest launches and a following plain launch.</summary>
[TestClass]
[DoNotParallelize]
public sealed class ProfiledVSTestLauncherTest
{
    /// <summary>The exact child evidence test selected by every acceptance leg.</summary>
    private const string ChildEvidenceFilter =
        "FullyQualifiedName=AlvorKit.Script.TestInterception." +
        "ProfiledVSTestLauncherTest.ChildEvidence_ReportsCurrentProfilerActivation";

    /// <summary>The VSTest parameter that declares the activation expected by the parent.</summary>
    private const string ExpectedProfilerParameter =
        "AlvorKitExpectedProfiler";

    /// <summary>The CI marker that turns a missing native proof asset into a failure.</summary>
    private const string RequiredProofVariable =
        "ALVORKIT_INTERCEPTION_PROOF_REQUIRED";

    /// <summary>Gets or sets the current MSTest context and supplied run parameters.</summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Proves two launcher-owned VSTest children load the profiler while a later ordinary child
    /// inherits no profiler activation.
    /// </summary>
    [TestMethod]
    public async Task ProfiledRunsFollowedByPlainRun_IsolateProfilerActivation()
    {
        var repositoryRoot = FindRepositoryRoot();
        var profilerPath = ResolveCheckedInProfilerOrSkip(repositoryRoot);
        var projectPath = Path.Combine(
            repositoryRoot,
            "tests",
            "AlvorKit.Script.TestInterception.Test",
            "AlvorKit.Script.TestInterception.Test.csproj");
        var configuration = Path.GetFileName(
            Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory));
        var options = new InterceptionLaunchOptions(
            repositoryRoot,
            projectPath,
            ExecutableProject: null,
            configuration,
            ChildEvidenceFilter,
            profilerPath,
            [Assembly.GetExecutingAssembly().GetName().Name!],
            TimeSpan.FromMinutes(2),
            [
                "--no-build",
                "--no-restore",
                "--logger",
                "console;verbosity=detailed",
                "--",
                ExpectedProfilerArgument(expected: true)
            ]);
        var launcher = new InterceptionLauncher();

        var firstExitCode = await launcher.RunAsync(options);
        var secondExitCode = await launcher.RunAsync(options);
        var plainExitCode = await RunPlainChildAsync(
            repositoryRoot,
            projectPath,
            configuration);

        Assert.AreEqual(0, firstExitCode, "The first profiled VSTest child failed.");
        Assert.AreEqual(0, secondExitCode, "The second profiled VSTest child failed.");
        Assert.AreEqual(0, plainExitCode, "The plain VSTest child inherited profiler activation.");
    }

    /// <summary>
    /// Reports whether the current VSTest child has the exact launcher profiler loaded and rejects
    /// partial or inherited activation.
    /// </summary>
    [TestMethod]
    public void ChildEvidence_ReportsCurrentProfilerActivation()
    {
        var expectsProfiler = string.Equals(
            TestContext.Properties[ExpectedProfilerParameter]?.ToString(),
            bool.TrueString,
            StringComparison.OrdinalIgnoreCase);
        var profilerPath = Environment.GetEnvironmentVariable(
            InterceptionProfilerAsset.PathVariable);
        var profilerModule = Process.GetCurrentProcess()
            .Modules
            .Cast<ProcessModule>()
            .SingleOrDefault(static module =>
                string.Equals(
                    Path.GetFileName(module.FileName),
                    InterceptionProfilerAsset.FileName,
                    StringComparison.OrdinalIgnoreCase));

        if (expectsProfiler)
        {
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(profilerPath),
                "The profiled VSTest child did not receive the launcher asset marker.");
            Assert.AreEqual(
                "1",
                Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING"));
            Assert.AreEqual(
                InterceptionRunSettings.ProfilerClsid,
                Environment.GetEnvironmentVariable("CORECLR_PROFILER"));
            Assert.AreEqual(
                Path.GetFullPath(profilerPath),
                Path.GetFullPath(
                    Environment.GetEnvironmentVariable(
                        "CORECLR_PROFILER_PATH")!));
            Assert.AreEqual(
                Path.GetFullPath(profilerPath),
                Path.GetFullPath(
                    Environment.GetEnvironmentVariable(
                        "CORECLR_PROFILER_PATH_64")!));
            Assert.IsNotNull(
                profilerModule,
                "The launcher variables were present but CoreCLR did not load the profiler.");
            Assert.AreEqual(
                Path.GetFullPath(profilerPath),
                Path.GetFullPath(profilerModule.FileName));
            Console.WriteLine(
                $"PROFILED_TESTHOST PID={Environment.ProcessId} PROFILER={profilerModule.FileName}");
            return;
        }

        var activeVariables = InterceptionProfilerEnvironment.ActiveVariables(
            Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(
                    static entry => (string)entry.Key,
                    static entry => entry.Value?.ToString(),
                    StringComparer.OrdinalIgnoreCase));
        CollectionAssert.AreEqual(
            Array.Empty<string>(),
            activeVariables,
            "The plain VSTest child inherited profiler activation variables.");
        Assert.IsTrue(
            string.IsNullOrWhiteSpace(profilerPath),
            "The plain VSTest child inherited the launcher asset marker.");
        Assert.IsNull(
            profilerModule,
            "The plain VSTest child loaded the interception profiler.");
        Console.WriteLine($"PLAIN_TESTHOST PID={Environment.ProcessId}");
    }

    /// <summary>Finds the repository containing the running test assembly.</summary>
    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlvorKit.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the AlvorKit repository from the test output directory.");
    }

    /// <summary>Resolves the checked-in native asset or marks this acceptance inconclusive.</summary>
    private static string ResolveCheckedInProfilerOrSkip(string repositoryRoot)
    {
        var checkedInPath = Path.Combine(
            repositoryRoot,
            "native",
            "interception-profiler",
            "runtimes",
            InterceptionProfilerAsset.RuntimeIdentifier,
            "native",
            InterceptionProfilerAsset.FileName);
        try
        {
            return InterceptionProfilerAsset.Resolve(repositoryRoot, checkedInPath);
        }
        catch (Exception exception) when (
            !string.Equals(
                Environment.GetEnvironmentVariable(RequiredProofVariable),
                "1",
                StringComparison.Ordinal) &&
            exception is PlatformNotSupportedException or FileNotFoundException)
        {
            Assert.Inconclusive(
                $"MIR-70b requires the checked-in {InterceptionProfilerAsset.RuntimeIdentifier} .NET 10 profiler asset: " +
                exception.Message);
            throw;
        }
    }

    /// <summary>Runs the evidence filter through ordinary VSTest without profiler settings.</summary>
    private static Task<int> RunPlainChildAsync(
        string repositoryRoot,
        string projectPath,
        string configuration)
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        Add(
            startInfo,
            "test",
            projectPath,
            "--configuration",
            configuration,
            "--filter",
            ChildEvidenceFilter,
            "--no-build",
            "--no-restore",
            "--logger",
            "console;verbosity=detailed",
            "--",
            ExpectedProfilerArgument(expected: false));
        return InterceptionChildProcess.RunAsync(
            startInfo,
            TimeSpan.FromMinutes(2));
    }

    /// <summary>Creates one VSTest run-parameter argument for the expected child mode.</summary>
    private static string ExpectedProfilerArgument(bool expected) =>
        $"TestRunParameters.Parameter(" +
        $"name=\"{ExpectedProfilerParameter}\"," +
        $"value=\"{expected.ToString().ToLowerInvariant()}\")";

    /// <summary>Adds literal arguments to one child command.</summary>
    private static void Add(
        ProcessStartInfo startInfo,
        params IEnumerable<string> arguments)
    {
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);
    }
}
