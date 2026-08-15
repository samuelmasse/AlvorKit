namespace AlvorKit;

/// <summary>Exercises session-owned static method and property interception.</summary>
[TestClass]
public sealed class StaticInterceptionTest
{
    /// <summary>
    /// Static return, callback, throw, generic, property, and verification share one session ledger.
    /// </summary>
    [TestMethod]
    public void Session_StaticBehaviorsExecuteAndVerifyWithoutOriginalCalls()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledStaticRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes = ProfiledStaticRouteLifecycle.CreateRoutes();
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

            ProfiledReceiverFreeTarget.Reset();
            var expected = new IOException("static failure");
            var observedPropertyValue = 0;
            using var session = Mock.Session();
            Mock.When(() => ProfiledStaticTransformCaller.Selected(
                    Arg.Any<int>()))
                .Answer((int value) => value * 3);
            Mock.When(() => ProfiledStaticTransformCaller.Selected(99))
                .Throw(expected);
            Mock.When(() => ProfiledStaticTransformCaller.Selected(103))
                .Strict();
            Mock.When(() => ProfiledGenericStaticCaller.Selected(
                    Arg.Any<string>()))
                .Return("configured");
            Mock.When(() => ProfiledGetStaticNumberCaller.Selected())
                .Return(73);
            Mock.When(() => ProfiledSetStaticNumberCaller.Selected(
                    Arg.Any<int>()))
                .Do<int>(value => observedPropertyValue = value);

            Assert.AreEqual(
                21,
                ProfiledStaticTransformCaller.Selected(7));
            Assert.AreSame(
                expected,
                Assert.ThrowsExactly<IOException>(
                    () => ProfiledStaticTransformCaller.Selected(99)));
            Assert.ThrowsExactly<MockException>(
                () => ProfiledStaticTransformCaller.Selected(103));
            Assert.AreEqual(
                "configured",
                ProfiledGenericStaticCaller.Selected("input"));
            ProfiledSetStaticNumberCaller.Selected(41);
            Assert.AreEqual(
                73,
                ProfiledGetStaticNumberCaller.Selected());

            Assert.AreEqual(41, observedPropertyValue);
            Assert.AreEqual(0, ProfiledReceiverFreeTarget.StaticCalls);
            Mock.Verify(() => ProfiledStaticTransformCaller.Selected(7))
                .Once();
            Mock.Verify(() => ProfiledStaticTransformCaller.Selected(99))
                .Once();
            Mock.Verify(() => ProfiledStaticTransformCaller.Selected(103))
                .Once();
            Mock.Verify(() => ProfiledGenericStaticCaller.Selected("input"))
                .Once();
            Mock.Verify(() => ProfiledSetStaticNumberCaller.Selected(41))
                .Once();
            Mock.Verify(() => ProfiledGetStaticNumberCaller.Selected())
                .Once();
            Assert.IsTrue(
                lifecycle.AllRewritten,
                "Every static assertion must enter its real rewritten " +
                "caller and production Mocking wrapper.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>
    /// Two callers targeting the same static method can be configured and verified independently.
    /// </summary>
    [TestMethod]
    public void Session_AtSiteDistinguishesSameStaticTarget()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledStaticAtSiteRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStaticAtSiteRouteLifecycle.CreateRoutes();
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

            ProfiledReceiverFreeTarget.Reset();
            using var session = Mock.Session();
            var first = Mock.Site(
                () => ProfiledStaticTransformCaller.Selected(0));
            var second = Mock.Site(
                () => ProfiledStaticTransformSecondCaller.Selected(0));
            Mock.When(() => ProfiledStaticTransformCaller.Selected(
                    Arg.Any<int>()))
                .AtSite(first)
                .Return(101);

            Assert.AreEqual(
                101,
                ProfiledStaticTransformCaller.Selected(3));
            Assert.AreEqual(
                14,
                ProfiledStaticTransformSecondCaller.Selected(4));

            Assert.AreEqual(1, ProfiledReceiverFreeTarget.StaticCalls);
            Mock.Verify(() => ProfiledStaticTransformCaller.Selected(3))
                .AtSite(first)
                .Once();
            Mock.Verify(
                    () =>
                        ProfiledStaticTransformSecondCaller.Selected(4))
                .AtSite(second)
                .Once();
            Assert.IsTrue(
                lifecycle.AllRewritten,
                "Both same-target callers must enter their distinct " +
                "rewritten production wrappers.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>
    /// Parallel ambient sessions retain independent static setups and release them on disposal.
    /// </summary>
    [TestMethod]
    public async Task ParallelSessions_StaticSetupsDoNotLeak()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledParallelStaticRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var route =
            ProfiledParallelStaticRouteLifecycle.CreateRoute();
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
            Assert.IsTrue(lifecycle.IsPrepared);
            Assert.IsTrue(route.IsActivated);

            ProfiledReceiverFreeTarget.Reset();
            var start = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var ready = 0;

            Task<int> Run(int configured) => Task.Run(async () =>
            {
                using var session = Mock.Session();
                Mock.When(() => ProfiledStaticTransformCaller.Selected(
                        Arg.Any<int>()))
                    .Return(configured);
                if (Interlocked.Increment(ref ready) == 2)
                    start.SetResult();
                await start.Task;
                int result =
                    ProfiledStaticTransformCaller.Selected(5);
                Mock.Verify(() =>
                        ProfiledStaticTransformCaller.Selected(5))
                    .Once();
                return result;
            });

            var results = await Task.WhenAll(
                Run(211),
                Run(307));

            CollectionAssert.AreEquivalent(
                new[] { 211, 307 },
                results);
            Assert.AreEqual(
                15,
                ProfiledStaticTransformCaller.Selected(5));
            Assert.AreEqual(1, ProfiledReceiverFreeTarget.StaticCalls);
            Assert.IsTrue(
                lifecycle.WasRewritten,
                "Both ambient sessions must share the real rewritten " +
                "caller while retaining independent setup state.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.IsRemoved);
        Assert.IsFalse(route.IsActivated);
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
