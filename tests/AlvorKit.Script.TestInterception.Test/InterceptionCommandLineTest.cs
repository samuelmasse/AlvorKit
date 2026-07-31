namespace AlvorKit.Script.TestInterception;

[TestClass]
public class InterceptionCommandLineTest
{
    /// <summary>Preserves arguments following the launcher separator.</summary>
    [TestMethod]
    public void SplitPreservesForwardedArguments()
    {
        var (launcherArguments, childArguments) =
            InterceptionCommandLine.Split(
                [
                    "--test-project",
                    "Sample.csproj",
                    "--",
                    "--no-build",
                    "--filter",
                    "Exact"
                ]);

        CollectionAssert.AreEqual(
            new[] { "--test-project", "Sample.csproj" },
            launcherArguments);
        CollectionAssert.AreEqual(
            new[] { "--no-build", "--filter", "Exact" },
            childArguments);
    }

    /// <summary>Rejects commands that do not select exactly one child mode.</summary>
    [TestMethod]
    public void ParserRequiresOneChildMode()
    {
        var parser = new InterceptionOptionsParser();

        Assert.ThrowsExactly<ArgumentException>(
            () => parser.Parse([], childArguments: null));
        Assert.ThrowsExactly<ArgumentException>(
            () => parser.Parse(
                [
                    "--test-project",
                    "A.csproj",
                    "--exec-project",
                    "B.csproj"
                ],
                childArguments: null));
    }

    /// <summary>Preserves child arguments while applying launcher defaults.</summary>
    [TestMethod]
    public void ParserKeepsChildArgumentsAndDefaults()
    {
        var parser = new InterceptionOptionsParser();

        var options = parser.Parse(
            ["--test-project", "Sample.csproj"],
            ["--no-build"]);

        Assert.IsTrue(options.IsTest);
        Assert.AreEqual("Sample.csproj", options.Project);
        Assert.AreEqual("Debug", options.Configuration);
        Assert.IsFalse(options.AllocationProfiling);
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.Timeout);
        CollectionAssert.AreEqual(
            new[] { "--no-build" },
            options.ChildArguments.ToArray());
    }

    /// <summary>Enables startup allocation callbacks only when explicitly requested.</summary>
    [TestMethod]
    public void ParserReadsAllocationProfilingOptIn()
    {
        var parser = new InterceptionOptionsParser();

        var options = parser.Parse(
            [
                "--exec-project",
                "Sample.csproj",
                "--allocation-profiling"
            ],
            childArguments: null);

        Assert.IsTrue(options.AllocationProfiling);
    }
}
