namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootScriptsTest
{
    /// <summary>Scripts are loaded when added and then exposed in execution order.</summary>
    [TestMethod]
    public void Add_LoadsAndSortsByOrder()
    {
        var scripts = new RootScripts();
        var high = new TrackingScript(10);
        var low = new TrackingScript(-1);

        scripts.Add(high);
        scripts.Add(low);

        Assert.AreEqual(1, high.Loads);
        Assert.AreEqual(1, low.Loads);
        Assert.AreSame(low, scripts.Span[0]);
        Assert.AreSame(high, scripts.Span[1]);
    }

    /// <summary>Scripts can provide a non-integer virtual order without changing priority compatibility state.</summary>
    [TestMethod]
    public void Add_UsesVirtualOrder()
    {
        var scripts = new RootScripts();
        var high = scripts.Add(new OrderedScript(1.5f));
        var low = scripts.Add(new OrderedScript(0.5f));

        Assert.AreSame(low, scripts.Span[0]);
        Assert.AreSame(high, scripts.Span[1]);
    }

    /// <summary>Removing a script unloads it once and leaves other scripts active.</summary>
    [TestMethod]
    public void Remove_UnloadsOnlyRemovedScript()
    {
        var scripts = new RootScripts();
        var first = scripts.Add(new TrackingScript(0));
        var second = scripts.Add(new TrackingScript(1));

        scripts.Remove(first);

        Assert.AreEqual(1, first.Unloads);
        Assert.AreEqual(0, second.Unloads);
        Assert.AreSame(second, scripts.Span[0]);
    }

    /// <summary>Removing a script that is not present leaves the list unchanged.</summary>
    [TestMethod]
    public void Remove_WhenScriptIsMissing_DoesNothing()
    {
        var scripts = new RootScripts();
        var active = scripts.Add(new TrackingScript(0));

        scripts.Remove(new TrackingScript(1));

        Assert.AreSame(active, scripts.Span[0]);
        Assert.AreEqual(0, active.Unloads);
    }

    /// <summary>Root-loop teardown unloads scripts in reverse execution order.</summary>
    [TestMethod]
    public void RemoveAllReverse_UnloadsFromLastToFirst()
    {
        var scripts = new RootScripts();
        var order = new List<int>();
        scripts.Add(new TrackingScript(0, order));
        scripts.Add(new TrackingScript(1, order));
        scripts.Add(new TrackingScript(2, order));

        scripts.RemoveAllReverse();

        CollectionAssert.AreEqual(new[] { 2, 1, 0 }, order);
        Assert.AreEqual(0, scripts.Span.Length);
    }

    private sealed class TrackingScript : Script
    {
        private readonly List<int>? unloadOrder;

        public TrackingScript(int priority, List<int>? unloadOrder = null)
        {
            Priority = priority;
            this.unloadOrder = unloadOrder;
        }

        public int Loads { get; private set; }

        public int Unloads { get; private set; }

        public override void Load() => Loads++;

        public override void Unload()
        {
            Unloads++;
            unloadOrder?.Add(Priority);
        }
    }

    private sealed class OrderedScript(float order) : Script
    {
        public override float Order => order;
    }
}
