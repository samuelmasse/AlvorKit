namespace AlvorKit;

/// <summary>Exercises construction outcomes through one real profiled newobj site.</summary>
[TestClass]
public sealed class ReceiverFreeConstructionOutcomesInterceptionTest
{
    /// <summary>Passthrough and throw are explicit construction outcomes in the same session.</summary>
    [TestMethod]
    public void Session_ConstructionPassthroughAndThrowRecordOutcomes()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructionOutcomesRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructionOutcomesRouteLifecycle.CreateRoutes();
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

            ProfiledConstructionOutcomesTarget.Reset();
            var expected =
                new IOException("construction failure");
            using var session = Mock.Session();
            Mock.WhenNew(() =>
                    ProfiledConstructionOutcomesCaller.Selected(31))
                .Passthrough();
            Mock.WhenNew(() =>
                    ProfiledConstructionOutcomesCaller.Selected(37))
                .Throw(expected);
            Mock.WhenNew(() =>
                    ProfiledConstructionOutcomesCaller.Selected(39))
                .Strict();

            var created =
                ProfiledConstructionOutcomesCaller.Selected(31);
            var actual = Assert.ThrowsExactly<IOException>(
                () =>
                    ProfiledConstructionOutcomesCaller.Selected(37));
            Assert.ThrowsExactly<MockException>(
                () =>
                    ProfiledConstructionOutcomesCaller.Selected(39));

            Assert.AreEqual(31, created.Value);
            Assert.AreSame(expected, actual);
            Assert.AreEqual("construction failure", actual.Message);
            Assert.AreEqual(
                1,
                ProfiledConstructionOutcomesTarget.ConstructorCalls);
            Mock.VerifyNew(() =>
                    ProfiledConstructionOutcomesCaller.Selected(31))
                .Once();
            Mock.VerifyNew(() =>
                    ProfiledConstructionOutcomesCaller.Selected(37))
                .Once();
            Mock.VerifyNew(() =>
                    ProfiledConstructionOutcomesCaller.Selected(39))
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
