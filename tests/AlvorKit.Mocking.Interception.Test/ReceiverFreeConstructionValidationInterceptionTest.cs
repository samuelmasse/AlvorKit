namespace AlvorKit;

/// <summary>Exercises construction-factory result validation through a profiled newobj site.</summary>
[TestClass]
public sealed class ReceiverFreeConstructionValidationInterceptionTest
{
    /// <summary>Construction factories reject wrong and null results without allocating.</summary>
    [TestMethod]
    public void Session_ConstructionFactoriesEnforceNonNullAssignableResults()
    {
        RequireProfiledHost();
        var profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructionValidationRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructionValidationRouteLifecycle.CreateRoutes();
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
                new ProfiledConstructionValidationTarget(47);
            ProfiledConstructionValidationTarget.Reset();
            var observed = 0;
            using (Mock.Session())
            {
                Mock.WhenNew(() =>
                        ProfiledConstructionValidationCaller.Selected(
                            Arg.Any<int>()))
                    .SubstituteFactory(
                        (Func<
                            int,
                            ProfiledConstructionValidationTarget>)(
                                value =>
                                {
                                    observed = value;
                                    return substitute;
                                }));
                var actual =
                    ProfiledConstructionValidationCaller.Selected(45);
                Assert.AreSame(substitute, actual);
                Assert.AreEqual(47, actual.Value);
                Assert.AreEqual(45, observed);
                Mock.VerifyNew(() =>
                        ProfiledConstructionValidationCaller.Selected(
                            45))
                    .Once();
            }

            using (Mock.Session())
            {
                MockException wrong =
                    Assert.ThrowsExactly<MockException>(() =>
                    {
                        Mock.WhenNew(() =>
                                ProfiledConstructionValidationCaller
                                    .Selected(41))
                            .SubstituteFactory(
                                (Func<int, object>)(
                                    _ => new object()));
                        _ = ProfiledConstructionValidationCaller
                            .Selected(41);
                    });
                StringAssert.Contains(
                    wrong.Message,
                    "callback Invoke return type does not match");
                Mock.VerifyNew(() =>
                        ProfiledConstructionValidationCaller.Selected(
                            41))
                    .Once();
            }

            using (Mock.Session())
            {
                MockException empty =
                    Assert.ThrowsExactly<MockException>(() =>
                    {
                        Mock.WhenNew(() =>
                                ProfiledConstructionValidationCaller
                                    .Selected(43))
                            .SubstituteFactory(
                                (Func<
                                    int,
                                    ProfiledConstructionValidationTarget>)(
                                        _ => null!));
                        _ = ProfiledConstructionValidationCaller
                            .Selected(43);
                    });
                StringAssert.Contains(empty.Message, "returned null");
                StringAssert.Contains(
                    empty.Message,
                    "non-null and assignable");
                Mock.VerifyNew(() =>
                        ProfiledConstructionValidationCaller.Selected(
                            43))
                    .Once();
            }

            Assert.AreEqual(
                0,
                ProfiledConstructionValidationTarget.ConstructorCalls);
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
