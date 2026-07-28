namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exercises definition-wide constructor remainder routing through ABI v3.</summary>
[TestClass]
public sealed class ConstructorBodyInterceptionTest
{
    /// <summary>No session runs the base initializer and original remainder in order.</summary>
    [TestMethod]
    public void NoSession_ConstructorBodyRunsOriginalAfterBase()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructorBodyRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructorBodyRouteLifecycle.CreateRoutes();
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
            ProfiledConstructorBodyTarget.Reset();

            var target =
                ProfiledConstructorBodyFactory.Create(3);

            Assert.AreEqual(103, target.BaseValue);
            Assert.AreEqual(3, target.Value);
            Assert.AreEqual(
                1,
                ProfiledConstructorBodyBaseTarget.BaseCalls);
            Assert.AreEqual(
                1,
                ProfiledConstructorBodyTarget.BodyCalls);
            CollectionAssert.AreEqual(
                new[] { "base:103", "body" },
                ProfiledConstructorBodyBaseTarget.EventSnapshot());
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>An observer sees the allocated identity and initialized base before original execution.</summary>
    [TestMethod]
    public void Session_ObservePreservesIdentityArgumentsAndOrdering()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructorBodyRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructorBodyRouteLifecycle.CreateRoutes();
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

            ProfiledConstructorBodyTarget? observed = null;
            var observedArgument = 0;
            var observedBase = 0;
            using var session = Mock.Session();
            Mock.WhenConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(
                        Arg.Any<int>()))
                .Observe(
                    (Action<ProfiledConstructorBodyTarget, int>)(
                        (target, value) =>
                        {
                            observed = target;
                            observedArgument = value;
                            observedBase = target.BaseValue;
                            ProfiledConstructorBodyBaseTarget.Record(
                                $"observe:{value}");
                        }));
            ProfiledConstructorBodyTarget.Reset();

            var actual =
                ProfiledConstructorBodyFactory.Create(7);

            Assert.AreSame(actual, observed);
            Assert.AreEqual(7, observedArgument);
            Assert.AreEqual(107, observedBase);
            Assert.AreEqual(7, actual.Value);
            Assert.AreEqual(
                1,
                ProfiledConstructorBodyTarget.BodyCalls);
            CollectionAssert.AreEqual(
                new[]
                {
                    "base:107",
                    "observe:7",
                    "body"
                },
                ProfiledConstructorBodyBaseTarget.EventSnapshot());
            Mock.VerifyConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(7))
                .Once();
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>A replacement keeps the allocated derived instance but skips its original remainder.</summary>
    [TestMethod]
    public void Session_ReplacePreservesReceiverAndSkipsRemainder()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructorBodyRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructorBodyRouteLifecycle.CreateRoutes();
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

            ProfiledConstructorBodyTarget? replaced = null;
            using var session = Mock.Session();
            Mock.WhenConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(11))
                .Replace(
                    (Action<ProfiledConstructorBodyTarget, int>)(
                        (target, value) =>
                        {
                            replaced = target;
                            ProfiledConstructorBodyBaseTarget.Record(
                                $"replace:{value}");
                        }));
            ProfiledConstructorBodyTarget.Reset();

            var actual =
                ProfiledConstructorBodyFactory.Create(11);

            Assert.AreSame(actual, replaced);
            Assert.AreEqual(111, actual.BaseValue);
            Assert.AreEqual(0, actual.Value);
            Assert.AreEqual(
                1,
                ProfiledConstructorBodyBaseTarget.BaseCalls);
            Assert.AreEqual(
                0,
                ProfiledConstructorBodyTarget.BodyCalls);
            CollectionAssert.AreEqual(
                new[] { "base:111", "replace:11" },
                ProfiledConstructorBodyBaseTarget.EventSnapshot());
            Mock.VerifyConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(11))
                .Once();
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>Unmatched constructor arguments fall back to the original remainder.</summary>
    [TestMethod]
    public void Session_UnmatchedConstructorBodyRunsOriginal()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructorBodyRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructorBodyRouteLifecycle.CreateRoutes();
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

            using var session = Mock.Session();
            Mock.WhenConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(13))
                .Replace(_ =>
                    ProfiledConstructorBodyBaseTarget.Record(
                        "unexpected"));
            ProfiledConstructorBodyTarget.Reset();

            var target =
                ProfiledConstructorBodyFactory.Create(17);

            Assert.AreEqual(17, target.Value);
            Assert.AreEqual(
                1,
                ProfiledConstructorBodyBaseTarget.BaseCalls);
            Assert.AreEqual(
                1,
                ProfiledConstructorBodyTarget.BodyCalls);
            CollectionAssert.AreEqual(
                new[] { "base:117", "body" },
                ProfiledConstructorBodyBaseTarget.EventSnapshot());
            Mock.VerifyConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(17))
                .Once();
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>Passthrough, configured throw, strict, and original throw record independently.</summary>
    [TestMethod]
    public void Session_ConstructorBodyOutcomesPreserveBaseAndHistory()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructorBodyRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructorBodyRouteLifecycle.CreateRoutes();
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

            var configured =
                new IOException(
                    "configured constructor remainder");
            using var session = Mock.Session();
            Mock.WhenConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(19))
                .Passthrough();
            Mock.WhenConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(23))
                .Throw(configured);
            Mock.WhenConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(29))
                .Strict();
            ProfiledConstructorBodyTarget.Reset();

            var passthrough =
                ProfiledConstructorBodyFactory.Create(19);
            var actual = Assert.ThrowsExactly<IOException>(
                () => ProfiledConstructorBodyFactory.Create(23));
            Assert.ThrowsExactly<MockException>(
                () => ProfiledConstructorBodyFactory.Create(29));
            var original =
                Assert.ThrowsExactly<InvalidOperationException>(
                    () =>
                        ProfiledConstructorBodyFactory.Create(-31));

            Assert.AreEqual(19, passthrough.Value);
            Assert.AreSame(configured, actual);
            StringAssert.Contains(original.Message, "-31");
            Assert.AreEqual(
                4,
                ProfiledConstructorBodyBaseTarget.BaseCalls);
            Assert.AreEqual(
                2,
                ProfiledConstructorBodyTarget.BodyCalls);
            CollectionAssert.AreEqual(
                new[]
                {
                    "base:119",
                    "body",
                    "base:123",
                    "base:129",
                    "base:69",
                    "body"
                },
                ProfiledConstructorBodyBaseTarget.EventSnapshot());
            Mock.VerifyConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(19))
                .Once();
            Mock.VerifyConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(23))
                .Once();
            Mock.VerifyConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(29))
                .Once();
            Mock.VerifyConstructorBody(
                    () => ProfiledConstructorBodyFactory.Create(-31))
                .Once();
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

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
