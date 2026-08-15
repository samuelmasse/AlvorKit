namespace AlvorKit;

/// <summary>Exercises advanced concrete behavior through real profiled callers.</summary>
[TestClass]
public sealed class ConcreteAdvancedInterceptionTest
{
    /// <summary>
    /// A configured concrete task answer completes asynchronously and verifies through the same source owner.
    /// </summary>
    [TestMethod]
    public async Task AsyncAnswer_CompletesAndVerifies()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledAsyncRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var route = ProfiledAsyncRouteLifecycle.CreateRoute();
        MockInterceptionActivation? activation = null;
        try
        {
            var result = coordinator.PrepareAndActivate([route]);
            activation = result.Activation;
            Assert.IsTrue(
                result.IsSuccessful,
                string.Join(
                    Environment.NewLine,
                    result.Diagnostics.Select(
                        diagnostic => diagnostic.Message)));
            Assert.IsNotNull(activation);
            Assert.IsTrue(activation.IsActive);
            Assert.IsTrue(route.IsActivated);
            Assert.IsTrue(lifecycle.IsPrepared);

            var target = Mock.Create<ProfiledAsyncTarget>();
            Mock.When(() =>
                    ProfiledAsyncCaller.Selected(
                        target,
                        Arg.Any<int>()))
                .Answer((int value) =>
                    Task.FromResult(value + 100));

            Assert.AreEqual(
                109,
                await ProfiledAsyncCaller.Selected(target, 9));
            Mock.Verify(() =>
                    ProfiledAsyncCaller.Selected(target, 9))
                .Once();
            Mock.VerifyNoOtherCalls(target);
            Assert.AreEqual(0, target.OriginalCalls);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.IsRemoved);
        Assert.IsFalse(route.IsActivated);
    }

    /// <summary>
    /// Configured and original partial returns, throws, and ref/out exits each complete one invocation.
    /// </summary>
    [TestMethod]
    public void Partial_ConfiguredAndOriginalPathsCompleteExactlyOnce()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledPartialRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes = ProfiledPartialRouteLifecycle.CreateRoutes();
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

            var expected =
                new IOException("original partial failure");
            var target = Mock.Partial(
                new ProfiledPartialTarget(expected));
            Mock.When(() =>
                    ProfiledAddCaller.Selected(target, 2, 3))
                .Return(83);

            Assert.AreEqual(
                83,
                ProfiledAddCaller.Selected(target, 2, 3));
            Assert.AreEqual(
                7,
                ProfiledAddCaller.Selected(target, 3, 4));
            Assert.AreEqual(
                47,
                ProfiledNeighborCaller.Selected(target, 7));
            Exception actual = Assert.ThrowsExactly<IOException>(
                () => ProfiledThrowCaller.Selected(target));
            var value = 4;
            Assert.AreEqual(
                7,
                ProfiledMutateCaller.Selected(
                    target,
                    ref value,
                    out var doubled));

            Assert.AreSame(expected, actual);
            Assert.AreEqual(7, value);
            Assert.AreEqual(14, doubled);
            Assert.AreEqual(4, target.OriginalCalls);
            Mock.Verify(() =>
                    ProfiledAddCaller.Selected(target, 2, 3))
                .Once();
            Mock.Verify(() =>
                    ProfiledAddCaller.Selected(target, 3, 4))
                .Once();
            Mock.Verify(() =>
                    ProfiledNeighborCaller.Selected(target, 7))
                .Once();
            Mock.Verify(() =>
                    ProfiledThrowCaller.Selected(target))
                .Once();
            var verificationValue = 4;
            Mock.Verify(() =>
                    ProfiledMutateCaller.Selected(
                        target,
                        ref verificationValue,
                        out _))
                .Once();
            Mock.VerifyNoOtherCalls(target);
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
