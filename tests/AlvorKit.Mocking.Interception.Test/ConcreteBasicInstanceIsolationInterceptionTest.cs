namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exercises receiver isolation through one real profiled caller.</summary>
[TestClass]
public sealed class ConcreteBasicInstanceIsolationInterceptionTest
{
    /// <summary>Concurrent calls remain isolated by receiver across two sealed full mocks.</summary>
    [TestMethod]
    public void InstanceIsolation_ConcurrentCallsDoNotCrossMocks()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledInstanceIsolationRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes = ProfiledInstanceIsolationRouteLifecycle.CreateRoutes();
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

            const int callCount = 64;
            var left = Mock.Create<ProfiledInstanceIsolationTarget>();
            var right = Mock.Create<ProfiledInstanceIsolationTarget>();
            Mock.When(() => ProfiledInstanceIsolationCaller.Selected(
                    left,
                    Arg.Any<int>(),
                    1))
                .Answer(call => call.Argument<int>(0) + 1000);
            Mock.When(() => ProfiledInstanceIsolationCaller.Selected(
                    right,
                    Arg.Any<int>(),
                    1))
                .Answer(call => call.Argument<int>(0) + 2000);

            Parallel.For(
                0,
                callCount,
                index =>
                {
                    Assert.AreEqual(
                        index + 1000,
                        ProfiledInstanceIsolationCaller.Selected(
                            left,
                            index,
                            1));
                    Assert.AreEqual(
                        index + 2000,
                        ProfiledInstanceIsolationCaller.Selected(
                            right,
                            index,
                            1));
                });

            Mock.Verify(() => ProfiledInstanceIsolationCaller.Selected(
                    left,
                    Arg.Any<int>(),
                    1))
                .Exactly(callCount);
            Mock.Verify(() => ProfiledInstanceIsolationCaller.Selected(
                    right,
                    Arg.Any<int>(),
                    1))
                .Exactly(callCount);
            Mock.VerifyNoOtherCalls(left);
            Mock.VerifyNoOtherCalls(right);
            Assert.AreEqual(0, left.OriginalCalls);
            Assert.AreEqual(0, right.OriginalCalls);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

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
