namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootLoopBindingTest
{
    /// <summary>Backend surfaces bound above the root scope resolve into root-scoped consumers.</summary>
    [TestMethod]
    public void Injector_BackendsAboveRootScope_ResolveFromRootState()
    {
        var injector = new Injector();
        var fn = new FnBackend();
        var ft = new FtBackend();
        var ma = new MaBackend();
        var xxh = new XxhBackend();
        var window = new GlfwWindow(123);
        injector.Add<Fn>(fn);
        injector.Add<Ft>(ft);
        injector.Add<Ma>(ma);
        injector.Add<Xxh>(xxh);
        injector.Add(window);
        var root = injector.Scope<RootScope>();

        var consumer = root.Get<RootBindingConsumer>();

        Assert.AreSame(fn, consumer.Fn);
        Assert.AreSame(ft, consumer.Ft);
        Assert.AreSame(ma, consumer.Ma);
        Assert.AreSame(xxh, consumer.Xxh);
        Assert.AreEqual(window, consumer.Window);
    }

    [Root]
    private sealed record RootBindingConsumer(Fn Fn, Ft Ft, Ma Ma, Xxh Xxh, GlfwWindow Window);
}
