using AlvorKit.Interception.CoreClr.Advanced;

namespace AlvorKit.Interception.Test;

[TestClass]
public class InterceptionAllocationReportTest
{
    /// <summary>Rejects capture settings that cannot fit the requested sampled stacks.</summary>
    [TestMethod]
    public void CaptureOptions_RejectInvalidBounds()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new InterceptionAllocationCaptureOptions
            {
                SampleInterval = 0
            }.Validate());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new InterceptionAllocationCaptureOptions
            {
                MaximumSamples = 1,
                MaximumFramesPerSample = 0
            }.Validate());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new InterceptionAllocationCaptureOptions
            {
                MaximumSamples = 65_537
            }.Validate());
    }

    /// <summary>Emits weighted source stacks as a valid Speedscope sampled profile.</summary>
    [TestMethod]
    public void SourceReport_WritesWeightedSpeedscopeProfile()
    {
        var report = new InterceptionAllocationSourceReport(
            exactTotalObjectAllocations: 11,
            sampleInterval: 10,
            droppedSamples: 0,
            failedStackWalks: 0,
            [
                new(
                    10,
                    [
                        new("Game.Update", "Game.cs", 12),
                        new("Game.Spawn", "Game.cs", 42)
                    ]),
                new(
                    1,
                    [
                        new("Game.Update", "Game.cs", 12),
                        new("Game.Spawn", "Game.cs", 42)
                    ])
            ]);
        using var workspace = TempWorkspace.Create();
        var path = workspace.PathFor("allocations.speedscope.json");

        report.WriteSpeedscope(path);

        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        var root = document.RootElement;
        var profile = root.GetProperty("profiles")[0];
        Assert.AreEqual("sampled", profile.GetProperty("type").GetString());
        CollectionAssert.AreEqual(
            new ulong[] { 10, 1 },
            profile.GetProperty("weights")
                .EnumerateArray()
                .Select(static value => value.GetUInt64())
                .ToArray());
        Assert.AreEqual(11UL, report.AttributedObjectAllocations);
        Assert.AreEqual(11UL, report.TopLines[0].AttributedObjectAllocations);
        Assert.IsFalse(report.IsLineAttributionExact);
    }

    /// <summary>Maps a selected assembly's method token and IL offset through its Portable PDB.</summary>
    [TestMethod]
    public void CaptureResult_ResolvesSelectedAssemblySourceLine()
    {
        var method = typeof(InterceptionAllocationReportTest).GetMethod(
            nameof(SourceProbe),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var capture = new InterceptionAllocationCaptureResult(
            totalObjectAllocations: 1,
            sampleInterval: 1,
            droppedSamples: 0,
            failedStackWalks: 0,
            unresolvedFrames: 0,
            firstFrameResolutionHResult: null,
            [
                new(
                    1,
                    0,
                    0,
                    [
                        new(
                            method.Module.ModuleVersionId,
                            method.MetadataToken,
                            0)
                    ])
            ]);

        var report = capture.ResolveSources(
            Assembly.GetExecutingAssembly());

        Assert.HasCount(1, report.Samples);
        Assert.HasCount(1, report.Samples[0].Frames);
        StringAssert.Contains(
            report.Samples[0].Frames[0].Method,
            nameof(SourceProbe));
        Assert.IsNotNull(report.Samples[0].Frames[0].Document);
        Assert.IsNotNull(report.Samples[0].Frames[0].Line);
        Assert.IsTrue(report.IsLineAttributionExact);
    }

    private static object SourceProbe() =>
        new();
}
