namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class TickMetricTest
{
    /// <summary>A metric window reports min, average, max, and latest values from its retained samples.</summary>
    [TestMethod]
    public void Window_Value_UsesBoundedSampleWindow()
    {
        var window = new TickMetricWindow(3);

        Assert.AreEqual(default, window.Value);

        window.Add(1);
        window.Add(2);
        window.Add(3);
        window.Add(4);

        Assert.AreEqual(new TickMetricValue(3, 4000, 3000, 4000), window.Value);
    }

    /// <summary>A metric window requires positive sample capacity.</summary>
    [TestMethod]
    public void Window_WithInvalidCapacity_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TickMetricWindow(0));
    }

    /// <summary>A tick metric records samples into each configured window.</summary>
    [TestMethod]
    public void Metric_Add_RecordsEveryWindow()
    {
        var metric = new TickMetric(1, 2);

        metric.Add(2);
        metric.Add(4);

        Assert.AreEqual(new TickMetricValue(1, 4000, 4000, 4000), metric[0]);
        Assert.AreEqual(new TickMetricValue(2, 4000, 3000, 4000), metric[1]);
        Assert.AreEqual(2, metric.Ticks);
        Assert.AreEqual(4000, metric.Last);
        Assert.AreEqual(3000, metric.Average);
        Assert.AreEqual(4000, metric.Max);
    }

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
        Assert.IsTrue(metric.Value.Average >= 0);
        Assert.IsTrue(metric.Value.Max >= 0);
    }

    /// <summary>A timer-backed window publishes the source metric snapshot and tick delta.</summary>
    [TestMethod]
    public void Window_StartStop_SamplesSourceMetric()
    {
        var metric = new TickMetric(TimeSpan.FromSeconds(10));
        var window = new TickMetricWindow(metric);

        metric.Add(0.25);
        window.Start();
        SpinWait.SpinUntil(() => window.Value.Ticks == 1, TimeSpan.FromMilliseconds(250));
        window.Stop();

        Assert.AreEqual(1, window.Value.Ticks);
        Assert.AreEqual(250, window.Value.Last);
        Assert.AreEqual(250, window.Value.Average);
        Assert.AreEqual(250, window.Value.Max);
    }

    /// <summary>A fixed-period window only refreshes its published values after the configured interval elapses.</summary>
    [TestMethod]
    public void PeriodWindow_Add_PublishesCompletedPeriod()
    {
        var window = new TickMetricPeriodWindow(TimeSpan.FromSeconds(1));

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TickMetricPeriodWindow(TimeSpan.Zero));
        window.Add(0.25);
        Assert.AreEqual(0, window.Ticks);

        window.Add(0.75);

        Assert.AreEqual(2, window.Ticks);
        Assert.AreEqual(750, window.Last);
        Assert.AreEqual(500, window.Average);
        Assert.AreEqual(750, window.Max);
    }
}
