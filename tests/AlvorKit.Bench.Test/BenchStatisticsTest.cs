namespace AlvorKit;

[TestClass]
public class BenchStatisticsTest
{
    /// <summary>Aggregated JSON retains schema-valid means across all launches, including distinct allocation scopes.</summary>
    [TestMethod]
    public void CombinedSamplesRecomputeTheirSummary()
    {
        BenchJsonSample[] samples =
        [
            new(1, 100, 10, 100000000, null, new(1000, null, 20)),
            new(2, 300, 10, 33333333, null, new(2000, null, 40)),
        ];
        var result = BenchStatistics.SummarizeSamples(samples);
        Assert.AreEqual(200, result.MeanElapsedNanoseconds);
        Assert.AreEqual(50000000, result.OperationsPerSecond);
        Assert.AreEqual(1500, result.MeanAllocations.MeasurementBytes);
        Assert.AreEqual(30, result.MeanAllocations.WorkloadBytes);
        Assert.IsNull(result.MeanAllocations.MeasurementObjects);
        Assert.ThrowsExactly<ArgumentException>(() => BenchStatistics.SummarizeSamples([]));
        samples[1] = samples[1] with { OperationCount = 11 };
        Assert.ThrowsExactly<ArgumentException>(() => BenchStatistics.SummarizeSamples(samples));
    }

    /// <summary>Uncertainty uses independent launch means and is absent for a single launch.</summary>
    [TestMethod]
    public void LaunchIntervalsUseSampleVariance()
    {
        var single = BenchStatistics.Estimate([10]);
        Assert.IsNull(single.Lower95);
        Assert.IsNull(single.Upper95);
        var estimate = BenchStatistics.Estimate([8, 10, 12]);
        Assert.AreEqual(10, estimate.Mean);
        Assert.AreEqual(2, estimate.StandardDeviation);
        Assert.AreEqual(10 - 4.303 * 2 / Math.Sqrt(3), estimate.Lower95!.Value, 0.00001);
        Assert.AreEqual(10 + 4.303 * 2 / Math.Sqrt(3), estimate.Upper95!.Value, 0.00001);
        var identical = BenchStatistics.Estimate([5, 5, 5]);
        Assert.AreEqual(5, identical.Lower95);
        Assert.AreEqual(5, identical.Upper95);
    }

    /// <summary>Invalid launch estimates cannot silently generate charts or significance claims.</summary>
    [TestMethod]
    public void InvalidLaunchesAreRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BenchStatistics.Estimate([]));
        Assert.ThrowsExactly<ArgumentException>(() => BenchStatistics.Estimate([double.NaN]));
        Assert.ThrowsExactly<ArgumentException>(() => BenchStatistics.Estimate([-1]));
    }
}
