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

        Assert.AreEqual(new TickMetricValue(2, 3, 4, 4), window.Value);
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

        Assert.AreEqual(new TickMetricValue(4, 4, 4, 4), metric[0]);
        Assert.AreEqual(new TickMetricValue(2, 3, 4, 4), metric[1]);
    }
}
