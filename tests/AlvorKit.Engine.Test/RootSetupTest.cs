namespace AlvorKit;

[TestClass]
public class RootSetupTest
{
    /// <summary>Setups run in declaration order and their registrations are available when constructing the boot state.</summary>
    [TestMethod]
    public void LoadState_AppliesSetupsBeforeConstructingAndLoadingState()
    {
        var root = new Injector().Scope<RootScope>();
        var events = new List<string>();
        root.Add(events);

        RootLoop.LoadState(root, typeof(BootState),
        [
            new Setup(scope =>
            {
                scope.Add(new StartupValue("configured"));
                events.Add("first");
            }),
            new Setup(scope => events.Add(scope.Get<StartupValue>().Value)),
        ]);

        CollectionAssert.AreEqual(new[] { "first", "configured", "loaded:configured" }, events);
        Assert.IsInstanceOfType<BootState>(root.Get<RootState>().Current);
    }

    /// <summary>A host without optional features can still load its boot state with an empty setup span.</summary>
    [TestMethod]
    public void LoadState_WithoutSetupsLoadsState()
    {
        var root = new Injector().Scope<RootScope>();
        var events = new List<string>();
        root.Add(events);
        root.Add(new StartupValue("plain"));

        RootLoop.LoadState(root, typeof(BootState), []);

        CollectionAssert.AreEqual(new[] { "loaded:plain" }, events);
    }

    private class Setup(Action<RootScope> apply) : IRootSetup
    {
        public void Apply(RootScope root) => apply(root);
    }

    private record StartupValue(string Value);

    [Root]
    private class BootState(StartupValue value, List<string> events) : State
    {
        public override void Load() => events.Add($"loaded:{value.Value}");
    }
}
