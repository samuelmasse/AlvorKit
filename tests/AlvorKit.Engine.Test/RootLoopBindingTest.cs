namespace AlvorKit;

[TestClass]
public sealed class RootLoopBindingTest
{
    /// <summary>Backend surfaces bound above the root scope resolve into root-scoped consumers.</summary>
    [TestMethod]
    public void Injector_BackendsAboveRootScope_ResolveFromRootState()
    {
        using var host = RootHost.Start();
        var ft = new FtBackend();
        var ma = new MaBackend();
        var window = new GlfwWindow(123);
        host.Injector.Add<Ft>(ft);
        host.Injector.Add<Ma>(ma);
        host.Injector.Add(window);

        var consumer = host.Root.Get<RootBindingConsumer>();

        Assert.IsInstanceOfType<FnBackend>(consumer.Fn);
        Assert.AreSame(ft, consumer.Ft);
        Assert.AreSame(ma, consumer.Ma);
        Assert.AreEqual(window, consumer.Window);
    }

    /// <summary>A child resolves the host log, noise service, and scope graph shared with its root.</summary>
    [TestMethod]
    public void RootHost_Start_SharesServicesWithChildren()
    {
        using var host = RootHost.Start();
        var root = host.Root;
        var graph = root.Get<InjectorScopeGraph>();
        var child = graph.Scope<ChildScope>(root);

        var consumer = child.Get<ChildConsumer>();

        Assert.AreEqual(graph.RootId, graph.GetId(root));
        Assert.AreSame(root.Get<Log>(), consumer.Log);
        Assert.AreSame(root.Get<FnGraph>(), consumer.Noise);
        Assert.AreSame(graph, consumer.Scopes);
        Assert.AreSame(consumer.Noise, child.Get<FnGraph>());
    }

    [Root]
    private sealed record RootBindingConsumer(Fn Fn, Ft Ft, Ma Ma, GlfwWindow Window);

    private class ChildAttribute : InjectorAttribute;

    [Child]
    private class ChildScope : InjectorScope<ChildAttribute>;

    [Child]
    private record ChildConsumer(Log Log, FnGraph Noise, InjectorScopeGraph Scopes);
}
