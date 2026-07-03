namespace AlvorKit.Engine.Test;

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
        Assert.IsTrue(double.IsNaN(metric.Value.Average));
        Assert.AreEqual(0, metric.Value.Max);
    }

    /// <summary>A timer-backed window publishes the source metric snapshot and tick delta.</summary>
    [TestMethod]
    public void Window_StartStop_SamplesSourceMetric()
    {
        var metric = new TickMetric(TimeSpan.FromSeconds(10));
        var window = new TickMetricWindow(metric);

        metric.Start();
        metric.End();
        window.Start();
        SpinWait.SpinUntil(() => window.Value.Ticks == 1, TimeSpan.FromMilliseconds(250));
        window.Stop();

        Assert.AreEqual(1, window.Value.Ticks);
        Assert.IsTrue(window.Value.Last >= 0);
        Assert.IsTrue(double.IsNaN(window.Value.Average));
        Assert.AreEqual(0, window.Value.Max);
    }
}
