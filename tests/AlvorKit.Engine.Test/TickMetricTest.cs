namespace AlvorKit;

[TestClass]
public sealed class TickMetricTest
{
    /// <summary>A stopwatch-backed metric records elapsed duration into its rolling value.</summary>
    [TestMethod]
    public void Metric_StartEnd_RecordsStopwatchValue()
    {
        var metric = new TickMetric(TimeSpan.FromSeconds(1));

        metric.Start();
        metric.End();

        Assert.AreEqual(TimeSpan.FromSeconds(1), metric.Duration);
        Assert.AreEqual(1, metric.Value.Ticks);
        Assert.IsTrue(metric.Value.Last >= 0);
        Assert.AreEqual(metric.Value.Last, metric.Value.Average);
        Assert.AreEqual(metric.Value.Last, metric.Value.Max);
    }

    /// <summary>Simultaneous ticks both count, and expired durations leave the rolling statistics.</summary>
    [TestMethod]
    public void Record_IncludesLatestAndExpiresOldSamples()
    {
        var metric = new TickMetric(TimeSpan.FromSeconds(1));

        metric.Record(0, 8);
        metric.Record(0, 2);
        Assert.AreEqual(new TickMetricValue(2, 2, 5, 8), metric.Value);

        metric.Record(Stopwatch.Frequency / 2, 5);
        metric.Record(Stopwatch.Frequency, 3);
        Assert.AreEqual(new TickMetricValue(4, 3, 4, 5), metric.Value);
    }

    /// <summary>Interval counts refresh once per duration and expire while idle.</summary>
    [TestMethod]
    public void Window_SamplesCompletedTicksOnOwnerThread()
    {
        var metric = new TickMetric(TimeSpan.FromSeconds(1));
        var window = new TickMetricWindow(metric);
        window.Start();

        long end = Stopwatch.GetTimestamp() + Stopwatch.Frequency;
        metric.Record(end, 4);
        metric.Record(end, 6);
        Assert.AreEqual(new TickMetricValue(2, 6, 5, 6), window.Read(end));
        metric.Record(end + 1, 2);
        Assert.AreEqual(new TickMetricValue(2, 6, 5, 6), window.Read(end + 1));
        Assert.AreEqual(default, window.Read(end + 2 * Stopwatch.Frequency));
    }

    /// <summary>Starting publishes completed ticks immediately and stopping freezes the published sample.</summary>
    [TestMethod]
    public void Window_StartAndStop_PublishesAndFreezesSample()
    {
        var metric = new TickMetric(TimeSpan.FromMinutes(1));
        var window = new TickMetricWindow(metric);
        metric.Start();
        metric.End();
        window.Start();
        var published = window.Value;
        Assert.AreEqual(metric.Value, published);

        window.Stop();
        metric.Start();
        metric.End();
        Assert.AreEqual(published, window.Read(Stopwatch.GetTimestamp() + 60 * Stopwatch.Frequency));
    }
}
