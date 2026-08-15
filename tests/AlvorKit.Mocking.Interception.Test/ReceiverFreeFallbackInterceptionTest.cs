namespace AlvorKit;

/// <summary>Exercises original behavior for every receiver-free operation without a session.</summary>
[TestClass]
public sealed class ReceiverFreeFallbackInterceptionTest
{
    /// <summary>Static, generic, property, construction, and field sites preserve original behavior.</summary>
    [TestMethod]
    public void NoSession_AllReceiverFreeSitesExecuteOriginalOperations()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledReceiverFreeRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledReceiverFreeRouteLifecycle.CreateRoutes();
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

            Assert.AreEqual(
                17,
                ProfiledStaticTransformCaller.Selected(7));
            Assert.AreEqual(
                "kept",
                ProfiledGenericStaticCaller.Selected("kept"));
            ProfiledSetStaticNumberCaller.Selected(31);
            Assert.AreEqual(
                31,
                ProfiledGetStaticNumberCaller.Selected());

            ProfiledWriteStaticFieldCaller.Selected(41);
            Assert.AreEqual(
                41,
                ProfiledReadStaticFieldCaller.Selected());
            var created =
                ProfiledReceiverFreeConstructionCaller.Selected(51);
            Assert.AreEqual(
                51,
                ProfiledReadInstanceFieldCaller.Selected(created));
            ProfiledWriteInstanceFieldCaller.Selected(created, 61);
            Assert.AreEqual(
                61,
                ProfiledReadInstanceFieldCaller.Selected(created));
            ProfiledWriteInstanceReferenceFieldCaller.Selected(
                created,
                "reference");
            Assert.AreEqual(
                "reference",
                ProfiledReadInstanceReferenceFieldCaller.Selected(
                    created));

            Assert.AreEqual(4, ProfiledReceiverFreeTarget.StaticCalls);
            Assert.AreEqual(
                1,
                ProfiledReceiverFreeTarget.ConstructorCalls);
            Assert.IsTrue(
                lifecycle.AllRewritten,
                "Every receiver-free assertion must enter its real " +
                "rewritten caller and production Mocking wrapper.");
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
