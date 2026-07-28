namespace AlvorKit.Script.LiveWorkspace.Test;

/// <summary>Tests the filesystem contract shared by live-debug command-line tools.</summary>
[TestClass]
public sealed class LiveWorkspaceStoreTest
{
    /// <summary>Creation writes stable target identity, standard directories, and a human-readable session file.</summary>
    [TestMethod]
    public void Create_WritesWorkspaceContractWithoutCapabilityToken()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        var target = Target();

        var manifest = store.Create("orbit-debug", "Inspect the colony orbit", target, "sense-1", 42);

        Assert.AreEqual(Path.Combine(repository.Root, "tmp", "live", "orbit-debug"), manifest.WorkspacePath);
        Assert.AreEqual(target, store.Read("orbit-debug").LiveCode);
        Assert.AreEqual("sense-1", manifest.AlvorSenseSessionId);
        Assert.AreEqual(42, manifest.BaselineGraphRevision);
        foreach (var directory in new[] { "lc", "lp", "bridge", "puppet", "events", "evidence", "baseline" })
            Assert.IsTrue(Directory.Exists(Path.Combine(manifest.WorkspacePath, directory)), directory);

        var sessionText = File.ReadAllText(Path.Combine(manifest.WorkspacePath, "SESSION.md"));
        StringAssert.Contains(sessionText, "Inspect the colony orbit");
        StringAssert.Contains(sessionText, target.SessionId);
        Assert.IsFalse(File.ReadAllText(Path.Combine(manifest.WorkspacePath, "session.json"))
            .Contains("token", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Submission identity is confined to its area and includes exact bytes and SHA-256.</summary>
    [TestMethod]
    public void Source_ConfinesSubmissionAndHashesExactContent()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        var manifest = store.Create("orbit-debug", "Inspect the colony orbit", Target(), null, 1);
        const string content = "public sealed class InspectOrbit { }\n";
        var sourcePath = Path.Combine(manifest.WorkspacePath, "lc", "001-inspect-orbit.cs");
        File.WriteAllText(sourcePath, content);

        var source = store.Source("orbit-debug", sourcePath, "lc");

        Assert.AreEqual(sourcePath, source.Path);
        Assert.AreEqual(Encoding.UTF8.GetByteCount(content), source.Bytes);
        Assert.AreEqual(
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant(),
            source.Sha256);

        var outside = repository.Write("outside.cs", content);
        Assert.ThrowsExactly<InvalidOperationException>(() => store.Source("orbit-debug", outside, "lc"));
    }

    /// <summary>Recorded operations preserve request and result JSON in monotonically numbered event directories.</summary>
    [TestMethod]
    public void Record_WritesExactEventsAndAdvancesSequence()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        store.Create("orbit-debug", "Inspect the colony orbit", Target(), null, 1);

        var first = store.Record("orbit-debug", "livecode-exec", new { Scope = 4, Source = "001.cs" }, new { Value = 7 });
        var second = store.Record("orbit-debug", "alvorsense-send", new[] { "render", "state" }, new { Running = true });

        Assert.AreEqual(1, first.EventId);
        Assert.AreEqual(2, second.EventId);
        CollectionAssert.AreEqual(
            new[] { "request.json", "result.json" },
            Directory.GetFiles(first.EventPath).Select(Path.GetFileName).Order().ToArray());
        StringAssert.Contains(File.ReadAllText(Path.Combine(first.EventPath, "request.json")), "\"scope\": 4");
        StringAssert.Contains(File.ReadAllText(Path.Combine(first.EventPath, "result.json")), "\"value\": 7");
        Assert.AreEqual(3, store.Read("orbit-debug").NextEventId);
    }

    /// <summary>Close rejects unresolved live effects and succeeds only after cleanup is explicitly recorded.</summary>
    [TestMethod]
    public void Close_RequiresEveryInterventionToBeResolved()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        store.Create("orbit-debug", "Inspect the colony orbit", Target(), null, 1);
        store.UpsertIntervention(
            "orbit-debug",
            new(
                "orbit-rate",
                LiveWorkspaceInterventionKind.LiveCode,
                "Changed the selected colony orbit rate",
                LiveWorkspaceInterventionState.Active,
                null,
                "lc/002-adjust-orbit.cs",
                "run lc/099-restore-orbit.cs"));

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => store.Close("orbit-debug"));
        StringAssert.Contains(exception.Message, "orbit-rate");

        store.ResolveIntervention("orbit-debug", "orbit-rate");
        var closed = store.Close("orbit-debug");

        Assert.AreEqual(LiveWorkspaceStatus.Closed, closed.Status);
        Assert.ThrowsExactly<InvalidOperationException>(
            () => store.Record("orbit-debug", "late-event", new { }, new { }));
    }

    /// <summary>Creation rejects unsafe identities, blank purposes, and duplicate workspace directories.</summary>
    [TestMethod]
    public void Create_RejectsInvalidIdentityPurposeAndDuplicates()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);

        Assert.ThrowsExactly<ArgumentException>(
            () => store.Create("", "Inspect", Target(), null, 1));
        Assert.ThrowsExactly<ArgumentException>(
            () => store.Create(".", "Inspect", Target(), null, 1));
        Assert.ThrowsExactly<ArgumentException>(
            () => store.Create("bad/name", "Inspect", Target(), null, 1));
        Assert.ThrowsExactly<ArgumentException>(
            () => store.Create("bad\0name", "Inspect", Target(), null, 1));
        Assert.ThrowsExactly<ArgumentException>(
            () => store.Create("valid", " ", Target(), null, 1));

        var manifest = store.Create("valid", "Inspect", Target(), null, 1);
        var read = store.Read(manifest.WorkspacePath);

        Assert.AreEqual(manifest.Id, read.Id);
        Assert.AreEqual(manifest.LiveCode, read.LiveCode);
        Assert.HasCount(0, read.Interventions);
        Assert.ThrowsExactly<InvalidOperationException>(
            () => store.Create("valid", "Inspect again", Target(), null, 1));
    }

    /// <summary>Reading reports missing, malformed, and incompatible workspace manifests distinctly.</summary>
    [TestMethod]
    public void Read_RejectsMissingMalformedAndUnsupportedManifests()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);

        Assert.ThrowsExactly<ArgumentException>(() => store.Read(" "));
        Assert.ThrowsExactly<InvalidOperationException>(() => store.Read("missing"));

        var manifest = store.Create("orbit-debug", "Inspect", Target(), null, 1);
        var manifestPath = Path.Combine(manifest.WorkspacePath, "session.json");
        var validJson = File.ReadAllText(manifestPath);

        File.WriteAllText(manifestPath, "null");
        var malformed = Assert.ThrowsExactly<InvalidOperationException>(() => store.Read("orbit-debug"));
        StringAssert.Contains(malformed.Message, "Invalid live workspace manifest");

        File.WriteAllText(
            manifestPath,
            validJson.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 2", StringComparison.Ordinal));
        var unsupported = Assert.ThrowsExactly<InvalidOperationException>(() => store.Read("orbit-debug"));
        StringAssert.Contains(unsupported.Message, "schema 2 is unsupported");
    }

    /// <summary>Baseline writes preserve JSON while rejecting unsafe filenames and closed workspaces.</summary>
    [TestMethod]
    public void WriteBaseline_ValidatesNameAndWorkspaceState()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        var manifest = store.Create("orbit-debug", "Inspect", Target(), null, 1);

        var path = store.WriteBaseline(manifest, "graph.json", new { Revision = 7 });

        StringAssert.Contains(File.ReadAllText(path), "\"revision\": 7");
        Assert.ThrowsExactly<ArgumentException>(
            () => store.WriteBaseline(manifest, "", new { Revision = 8 }));
        Assert.ThrowsExactly<ArgumentException>(
            () => store.WriteBaseline(manifest, "../graph.json", new { Revision = 8 }));

        var closed = store.Close("orbit-debug");
        Assert.ThrowsExactly<InvalidOperationException>(
            () => store.WriteBaseline(closed, "late.json", new { Revision = 8 }));
    }

    /// <summary>Event recording skips occupied sequence directories and validates operation names.</summary>
    [TestMethod]
    public void Record_SkipsOccupiedSequenceAndValidatesOperation()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        var manifest = store.Create("orbit-debug", "Inspect", Target(), null, 1);
        var occupied = Path.Combine(manifest.WorkspacePath, "events", "0001-livecode-exec");
        Directory.CreateDirectory(occupied);
        File.WriteAllText(Path.Combine(occupied, "sentinel.txt"), "preserve");

        var recorded = store.Record(
            "orbit-debug",
            "livecode-exec",
            new { Scope = 4 },
            new { Value = 7 });

        Assert.AreEqual(2, recorded.EventId);
        Assert.AreEqual("livecode-exec", recorded.Operation);
        Assert.IsTrue(File.Exists(Path.Combine(occupied, "sentinel.txt")));
        Assert.ThrowsExactly<ArgumentException>(
            () => store.Record("orbit-debug", "bad/name", new { }, new { }));
    }

    /// <summary>Source inspection distinguishes invalid areas, missing submissions, and files outside the workspace.</summary>
    [TestMethod]
    public void Source_RejectsInvalidAreaAndMissingSubmission()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        var manifest = store.Create("orbit-debug", "Inspect", Target(), null, 1);
        var missing = Path.Combine(manifest.WorkspacePath, "lc", "missing.cs");

        Assert.ThrowsExactly<ArgumentException>(
            () => store.Source("orbit-debug", missing, "../lc"));
        Assert.ThrowsExactly<FileNotFoundException>(
            () => store.Source("orbit-debug", missing, "lc"));
    }

    /// <summary>Intervention updates replace by id, sort deterministically, and reject unknown cleanup ids.</summary>
    [TestMethod]
    public void Interventions_ReplaceSortAndRejectUnknownResolution()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        store.Create("orbit-debug", "Inspect", Target(), null, 1);
        store.UpsertIntervention("orbit-debug", Intervention("zeta", "first"));
        store.UpsertIntervention("orbit-debug", Intervention("alpha", "second"));

        var updated = store.UpsertIntervention("orbit-debug", Intervention("zeta", "replacement"));

        CollectionAssert.AreEqual(new[] { "alpha", "zeta" }, updated.Interventions.Select(value => value.Id).ToArray());
        Assert.AreEqual("replacement", updated.Interventions[1].Description);
        Assert.ThrowsExactly<InvalidOperationException>(
            () => store.ResolveIntervention("orbit-debug", "missing"));
    }

    /// <summary>AlvorSense association trims explicit ids and clears blank associations.</summary>
    [TestMethod]
    public void AssociateAlvorSense_TrimsAndClearsSessionId()
    {
        using var repository = TempWorkspace.Create();
        var store = new LiveWorkspaceStore(repository.Root);
        store.Create("orbit-debug", "Inspect", Target(), null, 1);

        var associated = store.AssociateAlvorSense("orbit-debug", " sense-2 ");
        var cleared = store.AssociateAlvorSense("orbit-debug", " ");

        Assert.AreEqual("sense-2", associated.AlvorSenseSessionId);
        Assert.IsNull(cleared.AlvorSenseSessionId);
    }

    /// <summary>Creates one active intervention fixture.</summary>
    private static LiveWorkspaceIntervention Intervention(string id, string description) =>
        new(
            id,
            LiveWorkspaceInterventionKind.LiveCode,
            description,
            LiveWorkspaceInterventionState.Active,
            null,
            null,
            "restore");

    /// <summary>Creates a stable target identity for workspace fixtures.</summary>
    private static LiveWorkspaceTarget Target() =>
        new(
            "11111111111111111111111111111111",
            "mycelial-observatory",
            1234,
            new DateTimeOffset(2026, 7, 27, 12, 0, 0, TimeSpan.Zero));
}
