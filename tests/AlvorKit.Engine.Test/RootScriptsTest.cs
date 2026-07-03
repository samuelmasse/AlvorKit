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

    /// <summary>Scripts can provide a non-integer virtual order.</summary>
    [TestMethod]
    public void Add_UsesVirtualOrder()
    {
        var scripts = new RootScripts();
        var high = new OrderedScript(1.5f);
        var low = new OrderedScript(0.5f);

        scripts.Add(high);
        scripts.Add(low);

        Assert.AreSame(low, scripts.Span[0]);
        Assert.AreSame(high, scripts.Span[1]);
    }

    /// <summary>Removing a script unloads it once and leaves other scripts active.</summary>
    [TestMethod]
    public void Remove_UnloadsOnlyRemovedScript()
    {
        var scripts = new RootScripts();
        var first = new TrackingScript(0);
        var second = new TrackingScript(1);
        scripts.Add(first);
        scripts.Add(second);

        scripts.Remove(first);

        Assert.AreEqual(1, first.Unloads);
        Assert.AreEqual(0, second.Unloads);
        Assert.AreSame(second, scripts.Span[0]);
    }

    /// <summary>Removing a script that is not present still unloads the requested script.</summary>
    [TestMethod]
    public void Remove_WhenScriptIsMissing_UnloadsRequestedScript()
    {
        var scripts = new RootScripts();
        var active = new TrackingScript(0);
        var missing = new TrackingScript(1);
        scripts.Add(active);

        scripts.Remove(missing);

        Assert.AreSame(active, scripts.Span[0]);
        Assert.AreEqual(0, active.Unloads);
        Assert.AreEqual(1, missing.Unloads);
    }

    private sealed class TrackingScript(float order) : Script
    {
        public int Loads { get; private set; }

        public int Unloads { get; private set; }

        public override float Order => order;

        public override void Load() => Loads++;

        public override void Unload() => Unloads++;
    }

    private sealed class OrderedScript(float order) : Script
    {
        public override float Order => order;
    }
}
