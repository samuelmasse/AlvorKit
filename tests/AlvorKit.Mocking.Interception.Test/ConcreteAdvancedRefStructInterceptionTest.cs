namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exercises ref-struct input and return behavior through real profiled callers.</summary>
[TestClass]
public sealed class ConcreteAdvancedRefStructInterceptionTest
{
    /// <summary>
    /// Ref-struct inputs, return factories, and strict fallback execute through exact frames.
    /// </summary>
    [TestMethod]
    public void RefStructInputAndReturn_UseFactoryAndStrictFallback()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledRefStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes = ProfiledRefStructRouteLifecycle.CreateRoutes();
        MockInterceptionActivation? activation = null;
        try
        {
            var result = coordinator.PrepareAndActivate(routes);
            activation = result.Activation;
            Assert.IsTrue(
                result.IsSuccessful,
                string.Join(
                    Environment.NewLine,
                    result.Diagnostics.Select(
                        diagnostic => diagnostic.Message)));
            Assert.IsNotNull(activation);
            Assert.IsTrue(activation.IsActive);
            Assert.IsTrue(lifecycle.AllPrepared);
            Assert.IsTrue(routes.All(route => route.IsActivated));

            var configured = Mock.Create<ProfiledRefStructTarget>();
            var owner = new ProfiledWindowOwner([3, 5, 8]);
            Mock.When(() => ProfiledObserveCaller.Selected(
                    configured,
                    Arg.Any<ProfiledWindow>(0)))
                .Return(91);
            Mock.When(() => ProfiledWindowCaller.Selected(configured))
                .ReturnFactory(owner.Create);

            Assert.AreEqual(
                91,
                ProfiledObserveCaller.Selected(
                    configured,
                    new([13, 21])));
            ProfiledWindow returned =
                ProfiledWindowCaller.Selected(configured);
            Assert.IsTrue(
                returned.Values.SequenceEqual([3, 5, 8]));
            Assert.AreEqual(1, owner.Calls);

            var strict = Mock.Create<ProfiledRefStructTarget>();
            Assert.ThrowsExactly<MockException>(
                () => InvokeStrictObserve(strict));
            Assert.ThrowsExactly<MockException>(
                () => ProfiledWindowCaller.Selected(strict));
            Assert.AreEqual(0, strict.OriginalCalls);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    private static void InvokeStrictObserve(
        ProfiledRefStructTarget target) =>
        ProfiledObserveCaller.Selected(
            target,
            new([1]));

    /// <summary>Skips ordinary hosts that cannot honor ReJIT requests.</summary>
    private static void RequireProfiledHost()
    {
        if (Environment.GetEnvironmentVariable(
                InterceptionProfiler.PathEnvironmentVariable) is null)
        {
            Assert.Inconclusive(
                "Run through AlvorKit.Script.TestInterception.");
        }
    }
}
