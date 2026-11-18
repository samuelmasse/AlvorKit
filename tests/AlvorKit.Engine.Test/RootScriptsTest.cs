namespace AlvorKit;

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

    /// <summary>Only the owner of a registration can unload it, and repeated removal is harmless.</summary>
    [TestMethod]
    public void Remove_WhenScriptIsMissing_LeavesItAlone()
    {
        var scripts = new RootScripts();
        var active = new TrackingScript(0);
        var missing = new TrackingScript(1);
        scripts.Add(active);

        scripts.Remove(missing);

        Assert.AreSame(active, scripts.Span[0]);
        Assert.AreEqual(0, active.Unloads);
        Assert.AreEqual(0, missing.Unloads);
        scripts.Remove(active);
        scripts.Remove(active);
        Assert.AreEqual(1, active.Unloads);
    }

    /// <summary>Replacing a pending registration skips it for this pass and loads it in order for the next.</summary>
    [TestMethod]
    public void Dispatch_RemoveAndReaddBeforeTurn_SkipsOldRegistration()
    {
        var scripts = new RootScripts();
        var first = new TrackingScript(0);
        var pending = new TrackingScript(1);
        var last = new TrackingScript(2);
        var added = new TrackingScript(-1);
        scripts.Add(first);
        scripts.Add(pending);
        scripts.Add(last);
        var visited = new List<Script>();
        scripts.Dispatch(script =>
        {
            visited.Add(script);
            if (script == first)
            {
                scripts.Remove(pending);
                scripts.Add(pending);
                scripts.Add(added);
            }
        });

        CollectionAssert.AreEqual(new Script[] { first, last }, visited);
        Assert.AreEqual(1, pending.Unloads);
        Assert.AreEqual(2, pending.Loads);
        Assert.AreEqual(1, added.Loads);
        visited.Clear();
        scripts.Dispatch(visited.Add);
        CollectionAssert.AreEqual(new Script[] { added, first, pending, last }, visited);
    }

    /// <summary>Nested callbacks see removals, and an exception releases the pass so additions can run next time.</summary>
    [TestMethod]
    public void Dispatch_NestedFailure_PreservesRegistrationChanges()
    {
        var scripts = new RootScripts();
        var first = new TrackingScript(0);
        var survivor = new TrackingScript(1);
        var added = new TrackingScript(-1);
        scripts.Add(first);
        scripts.Add(survivor);
        var visited = new List<Script>();

        Assert.ThrowsExactly<InvalidOperationException>(() => scripts.Dispatch(script =>
        {
            scripts.Remove(script);
            scripts.Add(added);
            scripts.Dispatch(nested =>
            {
                visited.Add(nested);
                throw new InvalidOperationException();
            });
        }));

        CollectionAssert.AreEqual(new Script[] { survivor }, visited);
        visited.Clear();
        scripts.Dispatch(visited.Add);
        CollectionAssert.AreEqual(new Script[] { added, survivor }, visited);
    }

    /// <summary>Continuous frame dispatch reuses the ordered registrations without allocating per pass.</summary>
    [TestMethod]
    public void Dispatch_SteadyState_AllocatesNoManagedMemory()
    {
        var scripts = new RootScripts();
        scripts.Add(new Script());
        void DispatchUpdate() => scripts.Dispatch(0.01, static (script, delta) => script.Update(delta));

        DispatchUpdate();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 256; i++)
            DispatchUpdate();
        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
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
