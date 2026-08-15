namespace AlvorKit;

/// <summary>Exercises exact construction-factory substitution through a profiled newobj site.</summary>
[TestClass]
public sealed class ReceiverFreeConstructionFactoryInterceptionTest
{
    /// <summary>An exact construction factory receives declared arguments and returns its substitute.</summary>
    [TestMethod]
    public void Session_SubstituteFactoryReceivesConstructorArguments()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructionFactoryRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructionFactoryRouteLifecycle.CreateRoutes();
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

            var substitute =
                new ProfiledConstructionFactoryTarget(409);
            ProfiledConstructionFactoryTarget.Reset();
            var observed = 0;
            using var session = Mock.Session();
            Mock.WhenNew(() =>
                    ProfiledConstructionFactoryCaller.Selected(
                        Arg.Any<int>()))
                .SubstituteFactory(
                    (Func<int, ProfiledConstructionFactoryTarget>)(
                        value =>
                        {
                            observed = value;
                            return substitute;
                        }));

            var actual =
                ProfiledConstructionFactoryCaller.Selected(19);

            Assert.AreSame(substitute, actual);
            Assert.IsInstanceOfType<
                ProfiledConstructionFactoryTarget>(actual);
            Assert.AreEqual(409, actual.Value);
            Assert.AreEqual(19, observed);
            Assert.AreEqual(
                0,
                ProfiledConstructionFactoryTarget.ConstructorCalls);
            Mock.VerifyNew(() =>
                    ProfiledConstructionFactoryCaller.Selected(19))
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
