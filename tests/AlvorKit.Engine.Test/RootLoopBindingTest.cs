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
        var window = new GlfwWindow(123);
        injector.Add<Fn>(fn);
        injector.Add<Ft>(ft);
        injector.Add<Ma>(ma);
        injector.Add(window);
        var root = injector.Scope<RootScope>();

        var consumer = root.Get<RootBindingConsumer>();

        Assert.AreSame(fn, consumer.Fn);
        Assert.AreSame(ft, consumer.Ft);
        Assert.AreSame(ma, consumer.Ma);
        Assert.AreEqual(window, consumer.Window);
    }

    /// <summary>The application log registered above the engine root resolves unchanged in nested game scopes.</summary>
    [TestMethod]
    public void Injector_LogAboveRootScope_ResolvesFromRootState()
    {
        using var logging = new LogRuntime(TextWriter.Null);
        var injector = new Injector();
        injector.Add(logging.Log);
        var root = injector.Scope<RootScope>();

        var consumer = root.Get<RootLogConsumer>();

        Assert.AreSame(logging.Log, consumer.Log);
    }

    [Root]
    private sealed record RootBindingConsumer(Fn Fn, Ft Ft, Ma Ma, GlfwWindow Window);

    [Root]
    private sealed record RootLogConsumer(Log Log);
}
