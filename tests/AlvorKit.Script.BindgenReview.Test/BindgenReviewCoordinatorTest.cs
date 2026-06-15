namespace AlvorKit.Script.BindgenReview.Test;

/// <summary>Tests bindgen review coordination with fake process execution.</summary>
[TestClass]
public sealed class BindgenReviewCoordinatorTest
{
    /// <summary>The default coordinator can execute help without launching external tools.</summary>
    [TestMethod]
    public async Task Execute_HelpWithDefaultCoordinator_ReturnsHelp()
    {
        var result = await BindgenReviewCoordinator.CreateDefault().ExecuteAsync(new(
            BindgenReviewCommandKind.Help,
            Directory.GetCurrentDirectory(),
            null,
            null,
            null,
            false));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), BindgenReviewCommandParser.HelpText);
    }

    /// <summary>Start creates a suffixed review directory, writes a manifest, and runs bindgen for before.</summary>
    [TestMethod]
    public async Task Execute_Start_CreatesManifestAndRunsBindgen()
    {
        using var workspace = TempWorkspace.Create();
        var runner = new FakeProcessRunner(bindgenResult: new ProcessResult(0, "generated\n"));
        var coordinator = Coordinator(runner, suffix: "a1b2c");

        var result = await coordinator.ExecuteAsync(new(
            BindgenReviewCommandKind.Start,
            workspace.Root,
            "xxhash",
            null,
            "overloads",
            false));

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsTrue(Directory.Exists(Path.Combine(workspace.Root, "out", "bindgen-review", "xxhash-overloads-a1b2c")));
        StringAssert.Contains(File.ReadAllText(ManifestPath(workspace.Root)), "\"Library\": \"xxhash\"");
        CollectionAssert.Contains(result.Lines.ToArray(), "generated");
        CollectionAssert.AreEqual(new[] { "xxhash", "--output-root", "out/bindgen-review/xxhash-overloads-a1b2c/before" }, runner.LastBindgenTail());
    }

    /// <summary>Start defaults the manifest case name to the selected library when no case is supplied.</summary>
    [TestMethod]
    public async Task Execute_StartWithoutCase_UsesLibraryCaseName()
    {
        using var workspace = TempWorkspace.Create();

        await Coordinator(new FakeProcessRunner(), suffix: "a1b2c").ExecuteAsync(new(
            BindgenReviewCommandKind.Start,
            workspace.Root,
            "xxhash",
            null,
            null,
            false));

        StringAssert.Contains(File.ReadAllText(DefaultManifestPath(workspace.Root)), "\"CaseName\": \"xxhash\"");
    }

    /// <summary>After uses the manifest library and writes into the after snapshot directory.</summary>
    [TestMethod]
    public async Task Execute_After_RunsBindgenForAfterSnapshot()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);
        var runner = new FakeProcessRunner();

        var result = await Coordinator(runner).ExecuteAsync(new(
            BindgenReviewCommandKind.After,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            false));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.AreEqual(new[] { "xxhash", "--output-root", "out/bindgen-review/xxhash-a1b2c/after" }, runner.LastBindgenTail());
    }

    /// <summary>Diff returns a friendly message when git reports no generated changes.</summary>
    [TestMethod]
    public async Task Execute_DiffWithoutOutput_ReturnsNoDiffMessage()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);

        var result = await Coordinator(new FakeProcessRunner(new ProcessResult(0, ""))).ExecuteAsync(new(
            BindgenReviewCommandKind.Diff,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            false));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "No generated diff.");
    }

    /// <summary>Diff preserves real git failures instead of treating them as expected generated changes.</summary>
    [TestMethod]
    public async Task Execute_DiffGitFailure_ReturnsFailure()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);

        var result = await Coordinator(new FakeProcessRunner(new ProcessResult(2, "fatal\n"))).ExecuteAsync(new(
            BindgenReviewCommandKind.Diff,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            false));

        Assert.AreEqual(2, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "fatal");
    }

    /// <summary>Clean validates the manifest and deletes a disposable review directory.</summary>
    [TestMethod]
    public async Task Execute_Clean_DeletesReviewDirectory()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);

        var result = await Coordinator(new FakeProcessRunner()).ExecuteAsync(new(
            BindgenReviewCommandKind.Clean,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            false));

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsFalse(Directory.Exists(Path.Combine(workspace.Root, "out", "bindgen-review", "xxhash-a1b2c")));
    }

    /// <summary>Finish captures after, treats git diff exit code one as a successful review, and deletes the review directory.</summary>
    [TestMethod]
    public async Task Execute_Finish_PrintsDiffAndDeletesReviewDirectory()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);
        var runner = new FakeProcessRunner(new ProcessResult(1, "diff --git a/before/file.cs b/after/file.cs\n"));
        var coordinator = Coordinator(runner);

        var result = await coordinator.ExecuteAsync(new(
            BindgenReviewCommandKind.Finish,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            false));

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsFalse(Directory.Exists(Path.Combine(workspace.Root, "out", "bindgen-review", "xxhash-a1b2c")));
        CollectionAssert.Contains(result.Lines.ToArray(), "diff --git a/before/file.cs b/after/file.cs");
        Assert.AreEqual(2, runner.Commands.Count);
    }

    /// <summary>Finish honors keep for debugging so callers can preserve a review directory explicitly.</summary>
    [TestMethod]
    public async Task Execute_FinishWithKeep_PreservesReviewDirectory()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);
        var coordinator = Coordinator(new FakeProcessRunner(new ProcessResult(0, "")));

        await coordinator.ExecuteAsync(new(
            BindgenReviewCommandKind.Finish,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            true));

        Assert.IsTrue(Directory.Exists(Path.Combine(workspace.Root, "out", "bindgen-review", "xxhash-a1b2c")));
    }

    /// <summary>Finish stops before diff and cleanup when the after bindgen run fails.</summary>
    [TestMethod]
    public async Task Execute_FinishAfterFailure_PreservesReviewDirectory()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);
        var runner = new FakeProcessRunner(bindgenResult: new ProcessResult(5, "bindgen failed\n"));

        var result = await Coordinator(runner).ExecuteAsync(new(
            BindgenReviewCommandKind.Finish,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            false));

        Assert.AreEqual(5, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "bindgen failed");
        Assert.IsTrue(Directory.Exists(Path.Combine(workspace.Root, "out", "bindgen-review", "xxhash-a1b2c")));
        Assert.AreEqual(1, runner.Commands.Count);
    }

    /// <summary>Finish preserves the review directory when git diff itself fails.</summary>
    [TestMethod]
    public async Task Execute_FinishDiffFailure_PreservesReviewDirectory()
    {
        using var workspace = TempWorkspace.Create();
        WriteManifest(workspace.Root);
        var runner = new FakeProcessRunner(new ProcessResult(2, "fatal\n"), new ProcessResult(0, "after warning\n"));

        var result = await Coordinator(runner).ExecuteAsync(new(
            BindgenReviewCommandKind.Finish,
            workspace.Root,
            null,
            "out/bindgen-review/xxhash-a1b2c",
            null,
            false));

        Assert.AreEqual(2, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "after warning");
        CollectionAssert.Contains(result.Lines.ToArray(), "fatal");
        Assert.IsTrue(Directory.Exists(Path.Combine(workspace.Root, "out", "bindgen-review", "xxhash-a1b2c")));
    }

    /// <summary>Unknown command kinds defensively return help.</summary>
    [TestMethod]
    public async Task Execute_UnknownCommand_ReturnsHelp()
    {
        var result = await Coordinator(new FakeProcessRunner()).ExecuteAsync(new(
            (BindgenReviewCommandKind)999,
            Directory.GetCurrentDirectory(),
            null,
            null,
            null,
            false));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), BindgenReviewCommandParser.HelpText);
    }

    /// <summary>Creates a coordinator with deterministic suffix and timestamp values.</summary>
    private static BindgenReviewCoordinator Coordinator(FakeProcessRunner runner, string suffix = "z9y8x") =>
        new(new BindgenReviewStore(), runner, () => suffix, () => new DateTimeOffset(2026, 6, 15, 18, 0, 0, TimeSpan.Zero));

    /// <summary>Returns the manifest path used by the start test.</summary>
    private static string ManifestPath(string repoRoot) =>
        Path.Combine(repoRoot, "out", "bindgen-review", "xxhash-overloads-a1b2c", ".bindgen-review.json");

    /// <summary>Returns the manifest path used by the default-case start test.</summary>
    private static string DefaultManifestPath(string repoRoot) =>
        Path.Combine(repoRoot, "out", "bindgen-review", "xxhash-a1b2c", ".bindgen-review.json");

    /// <summary>Writes a minimal manifest for an existing review session.</summary>
    private static void WriteManifest(string repoRoot)
    {
        var root = Path.Combine(repoRoot, "out", "bindgen-review", "xxhash-a1b2c");
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, ".bindgen-review.json"),
            """
            {
              "Library": "xxhash",
              "CaseName": "xxhash",
              "RelativeRoot": "out/bindgen-review/xxhash-a1b2c",
              "CreatedAt": "2026-06-15T18:00:00+00:00"
            }
            """);
    }

    /// <summary>Fake process runner that records commands and returns configured results.</summary>
    private sealed class FakeProcessRunner(ProcessResult? diffResult = null, ProcessResult? bindgenResult = null) : IProcessRunner
    {
        /// <summary>Commands received by the fake process runner.</summary>
        public List<CommandSpec> Commands { get; } = [];

        /// <inheritdoc />
        public Task<ProcessResult> RunAsync(CommandSpec command)
        {
            Commands.Add(command);
            if (command.FileName == "git")
                return Task.FromResult(diffResult ?? new ProcessResult(0, ""));

            return Task.FromResult(bindgenResult ?? new ProcessResult(0, ""));
        }

        /// <summary>Returns the bindgen-specific trailing arguments from the latest dotnet command.</summary>
        public string[] LastBindgenTail() =>
            [.. Commands.Last(command => command.FileName == "dotnet").Arguments.Skip(4)];
    }
}
