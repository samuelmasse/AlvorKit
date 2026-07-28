namespace AlvorKit.Script.TestInterception;

[TestClass]
public sealed class InterceptionCommandLineTest
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
            () => parser.Parse([]));
        Assert.ThrowsExactly<ArgumentException>(
            () => parser.Parse(
                [
                    "--test-project",
                    "A.csproj",
                    "--exec-project",
                    "B.csproj"
                ]));
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
        Assert.AreEqual(TimeSpan.FromMinutes(5), options.Timeout);
        CollectionAssert.AreEqual(
            new[] { "--no-build" },
            options.ChildArguments.ToArray());
    }
}
