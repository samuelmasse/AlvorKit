namespace AlvorKit;

/// <summary>Exercises exact receiver-free construction interception.</summary>
[TestClass]
public sealed class ConstructionInterceptionTest
{
    /// <summary>
    /// A substitute preserves identity and state without running construction.
    /// </summary>
    [TestMethod]
    public void Session_SubstituteSkipsOriginalConstructionAndVerifies()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledSubstituteConstructionRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledSubstituteConstructionRouteLifecycle.CreateRoutes();
        MockInterceptionActivation? activation = null;
        var substitute = new ProfiledReceiverFreeTarget(401);
        var activeHandlerInvocations = 0;
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
            Assert.IsTrue(lifecycle.IsPrepared);
            Assert.IsTrue(routes.All(route => route.IsActivated));

            ProfiledReceiverFreeTarget.Reset();
            using (MockSession session = Mock.Session())
            {
                Mock.WhenNew(
                        () =>
                            ProfiledSubstituteConstructionCaller.Selected(
                                Arg.Any<int>()))
                    .Substitute(substitute);

                ProfiledReceiverFreeTarget actual =
                    ProfiledSubstituteConstructionCaller.Selected(17);

                Assert.AreSame(substitute, actual);
                Assert.AreEqual(
                    typeof(ProfiledReceiverFreeTarget),
                    actual.GetType());
                Assert.AreEqual(401, actual.InstanceField);
                Assert.AreEqual(
                    0,
                    ProfiledReceiverFreeTarget.ConstructorCalls);
                Mock.VerifyNew(
                        () =>
                            ProfiledSubstituteConstructionCaller.Selected(
                                17))
                    .Once();
                session.VerifySequence(
                    () =>
                        ProfiledSubstituteConstructionCaller.Selected(
                            17));
            }

            ProfiledReceiverFreeTarget noSession =
                ProfiledSubstituteConstructionCaller.Selected(23);
            Assert.AreNotSame(substitute, noSession);
            Assert.AreEqual(
                typeof(ProfiledReceiverFreeTarget),
                noSession.GetType());
            Assert.AreEqual(23, noSession.InstanceField);
            Assert.AreEqual(
                1,
                ProfiledReceiverFreeTarget.ConstructorCalls);
            Assert.IsTrue(lifecycle.HandlerInvocations >= 2);
            activeHandlerInvocations =
                lifecycle.HandlerInvocations;
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.IsRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));

        int constructorCallsAfterRemoval =
            ProfiledReceiverFreeTarget.ConstructorCalls;
        ProfiledReceiverFreeTarget restored =
            ProfiledSubstituteConstructionCaller.Selected(29);
        Assert.AreNotSame(substitute, restored);
        Assert.AreEqual(29, restored.InstanceField);
        Assert.AreEqual(
            constructorCallsAfterRemoval + 1,
            ProfiledReceiverFreeTarget.ConstructorCalls);
        Assert.AreEqual(
            activeHandlerInvocations,
            lifecycle.HandlerInvocations);
    }

    /// <summary>
    /// Two sites for one constructor substitute and pass through independently.
    /// </summary>
    [TestMethod]
    public void Session_ConstructionAtSiteDistinguishesSameConstructor()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructionAtSiteRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledConstructionAtSiteRouteLifecycle.CreateRoutes();
        MockInterceptionActivation? activation = null;
        var substitute = new ProfiledReceiverFreeTarget(503);
        var activeHandlerInvocations = 0;
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
            using (MockSession session = Mock.Session())
            {
                var first = Mock.Site(
                    () =>
                        ProfiledConstructionAtSiteFirstCaller.Selected(
                            0));
                var second = Mock.Site(
                    () =>
                        ProfiledConstructionAtSiteSecondCaller.Selected(
                            0));
                Mock.WhenNew(
                        () =>
                            ProfiledConstructionAtSiteFirstCaller.Selected(
                                Arg.Any<int>()))
                    .AtSite(first)
                    .Substitute(substitute);

                ProfiledReceiverFreeTarget firstResult =
                    ProfiledConstructionAtSiteFirstCaller.Selected(23);
                ProfiledReceiverFreeTarget secondResult =
                    ProfiledConstructionAtSiteSecondCaller.Selected(29);

                Assert.AreSame(substitute, firstResult);
                Assert.AreEqual(503, firstResult.InstanceField);
                Assert.AreNotSame(substitute, secondResult);
                Assert.AreEqual(29, secondResult.InstanceField);
                Assert.AreEqual(
                    1,
                    ProfiledReceiverFreeTarget.ConstructorCalls);
                Mock.VerifyNew(
                        () =>
                            ProfiledConstructionAtSiteFirstCaller.Selected(
                                23))
                    .AtSite(first)
                    .Once();
                Mock.VerifyNew(
                        () =>
                            ProfiledConstructionAtSiteSecondCaller.Selected(
                                29))
                    .AtSite(second)
                    .Once();
                session.VerifySequence(
                    () =>
                        ProfiledConstructionAtSiteFirstCaller.Selected(
                            23),
                    () =>
                        ProfiledConstructionAtSiteSecondCaller.Selected(
                            29));
            }

            ProfiledReceiverFreeTarget firstNoSession =
                ProfiledConstructionAtSiteFirstCaller.Selected(31);
            ProfiledReceiverFreeTarget secondNoSession =
                ProfiledConstructionAtSiteSecondCaller.Selected(37);
            Assert.AreNotSame(substitute, firstNoSession);
            Assert.AreEqual(31, firstNoSession.InstanceField);
            Assert.AreNotSame(substitute, secondNoSession);
            Assert.AreEqual(37, secondNoSession.InstanceField);
            Assert.AreEqual(
                3,
                ProfiledReceiverFreeTarget.ConstructorCalls);
            Assert.IsTrue(lifecycle.AllRewritten);
            activeHandlerInvocations =
                lifecycle.HandlerInvocations;
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));

        int constructorCallsAfterRemoval =
            ProfiledReceiverFreeTarget.ConstructorCalls;
        ProfiledReceiverFreeTarget restoredFirst =
            ProfiledConstructionAtSiteFirstCaller.Selected(41);
        ProfiledReceiverFreeTarget restoredSecond =
            ProfiledConstructionAtSiteSecondCaller.Selected(43);
        Assert.AreEqual(41, restoredFirst.InstanceField);
        Assert.AreEqual(43, restoredSecond.InstanceField);
        Assert.AreEqual(
            constructorCallsAfterRemoval + 2,
            ProfiledReceiverFreeTarget.ConstructorCalls);
        Assert.AreEqual(
            activeHandlerInvocations,
            lifecycle.HandlerInvocations);
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
