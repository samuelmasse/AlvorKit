namespace AlvorKit.Engine.SourceUpdate.Test;

/// <summary>Verifies exact identity, forward generations, idempotency, and poisoned apply handling.</summary>
[TestClass]
public sealed class SourceUpdateModuleLedgerTest
{
    /// <summary>Two valid generations advance once each and a repeated update id returns the recorded result.</summary>
    [TestMethod]
    public void AppliesForwardGenerationsAndReportsDuplicateId()
    {
        var runtime = new TestSourceUpdateRuntime();
        var ledger = CreateLedger(runtime);
        var first = Request(ledger, "update-1", 0, "source-0", "source-1");
        var second = Request(ledger, "update-2", 1, "source-1", "source-2");

        var firstResult = ledger.Apply(first);
        var duplicateResult = ledger.Apply(first);
        var secondResult = ledger.Apply(second);

        Assert.AreEqual(SourceUpdateApplyStatus.Applied, firstResult.Status);
        Assert.AreEqual(1, firstResult.Generation);
        Assert.AreSame(firstResult, duplicateResult);
        Assert.AreEqual(SourceUpdateApplyStatus.Applied, secondResult.Status);
        Assert.AreEqual(2, secondResult.Generation);
        Assert.AreEqual(2, runtime.ApplyCount);
    }

    /// <summary>A stale generation is rejected before the runtime receives any delta.</summary>
    [TestMethod]
    public void RejectsStaleGenerationBeforeApply()
    {
        var runtime = new TestSourceUpdateRuntime();
        var ledger = CreateLedger(runtime);
        var request = Request(ledger, "stale", 1, "source-0", "source-1");

        var result = ledger.Apply(request);

        Assert.AreEqual(SourceUpdateApplyStatus.Rejected, result.Status);
        StringAssert.Contains(result.Error, "Expected generation 1");
        Assert.AreEqual(0, runtime.ApplyCount);
    }

    /// <summary>A forward generation must name the source hash acknowledged by the prior apply.</summary>
    [TestMethod]
    public void RejectsConflictingPreviousSourceHash()
    {
        var runtime = new TestSourceUpdateRuntime();
        var ledger = CreateLedger(runtime);
        _ = ledger.Apply(Request(ledger, "first", 0, "source-0", "source-1"));

        var result = ledger.Apply(Request(ledger, "conflict", 1, "other-source", "source-2"));

        Assert.AreEqual(SourceUpdateApplyStatus.Rejected, result.Status);
        StringAssert.Contains(result.Error, "previous source hash");
        Assert.AreEqual(1, runtime.ApplyCount);
    }

    /// <summary>An ApplyUpdate exception makes the module restart-required and rejects later generations.</summary>
    [TestMethod]
    public void ApplyFailurePoisonsModule()
    {
        var runtime = new TestSourceUpdateRuntime { Failure = new NotSupportedException("fixture failure") };
        var ledger = CreateLedger(runtime);

        var failed = ledger.Apply(Request(ledger, "failed", 0, "source-0", "source-1"));
        runtime.Failure = null;
        var later = ledger.Apply(Request(ledger, "later", 0, "source-0", "source-2"));

        Assert.AreEqual(SourceUpdateApplyStatus.RestartRequired, failed.Status);
        Assert.IsTrue(failed.RestartRequired);
        Assert.AreEqual(SourceUpdateApplyStatus.Rejected, later.Status);
        StringAssert.Contains(later.Error, "restart-required");
        Assert.AreEqual(1, runtime.ApplyCount);
    }

    private static SourceUpdateModuleLedger CreateLedger(TestSourceUpdateRuntime runtime)
    {
        var assembly = typeof(SourceUpdateModuleLedgerTest).Assembly;
        var assemblyPath = assembly.Location;
        var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
        var launch = new SourceUpdateEditableLaunchManifest(
            1,
            "fixture.csproj",
            assemblyPath,
            pdbPath,
            HashFile(assemblyPath),
            HashFile(pdbPath),
            assembly.ManifestModule.ModuleVersionId.ToString("N"),
            "fixture-project");
        return new(
            SourceUpdateHostOptions.ForTest(assembly, launch),
            runtime,
            _ => [],
            validateProcessMode: false);
    }

    private static SourceUpdateApplyRequest Request(
        SourceUpdateModuleLedger ledger,
        string updateId,
        int generation,
        string previousSource,
        string resultSource)
    {
        var module = ledger.Capabilities().Modules.Single();
        var metadata = new byte[] { 1, 2, 3 };
        var il = new byte[] { 4, 5 };
        var pdb = new byte[] { 6 };
        return new(
            module.ModuleMvid,
            generation,
            updateId,
            previousSource,
            resultSource,
            typeof(SourceUpdateModuleLedgerTest).GetMethod(
                nameof(AppliesForwardGenerationsAndReportsDuplicateId))!.MetadataToken,
            [typeof(SourceUpdateModuleLedgerTest).MetadataToken],
            metadata,
            il,
            pdb,
            Hash(metadata),
            Hash(il),
            Hash(pdb),
            module.ProjectIdentityHash);
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Hash(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

/// <summary>Records metadata-apply calls and optionally throws one deterministic failure.</summary>
internal sealed class TestSourceUpdateRuntime : ISourceUpdateRuntime
{
    internal int ApplyCount { get; private set; }

    internal Exception? Failure { get; set; }

    public bool IsSupported => true;

    public void ApplyUpdate(
        Assembly assembly,
        byte[] metadataDelta,
        byte[] ilDelta,
        byte[] pdbDelta)
    {
        ApplyCount++;
        if (Failure is { } failure)
            throw failure;
    }
}
