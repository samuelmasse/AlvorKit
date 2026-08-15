namespace AlvorKit;

public sealed partial class FieldInterceptionTest
{
    /// <summary>
    /// Static writes transform before storage and reads transform after loading.
    /// </summary>
    [TestMethod]
    public void Session_StaticFieldTransformPreservesOriginalStorage()
    {
        RequireProfiledHost();
        ProfiledReceiverFreeTarget.Reset();
        Assert.AreEqual(0, ProfiledReceiverFreeTarget.StaticField);

        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledStaticFieldTransformRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStaticFieldTransformRouteLifecycle.CreateRoutes();
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
            Assert.AreEqual(
                0,
                ProfiledReceiverFreeTarget.StaticField,
                "Inert preparation must preserve the initial storage.");

            var field = Mock.Field<ProfiledReceiverFreeTarget, int>(
                nameof(ProfiledReceiverFreeTarget.StaticField));
            using var session = Mock.Session();
            Mock.WhenFieldWrite(
                    field,
                    () => Arg.Any<int>())
                .Transform(Increment);
            Mock.WhenFieldRead(field)
                .Transform(Double);

            ProfiledStaticFieldTransformWriteCaller.Selected(10);
            int transformed =
                ProfiledStaticFieldTransformReadCaller.Selected();

            Assert.AreEqual(11, ProfiledReceiverFreeTarget.StaticField);
            Assert.AreEqual(22, transformed);
            Mock.VerifyFieldWrite(field, () => 10)
                .Once();
            Mock.VerifyFieldRead(field)
                .Once();
            Assert.IsTrue(
                lifecycle.AllWrappersEnteredExactlyOnce,
                "Both rewritten field sites must enter their exact " +
                "production receiver-free wrappers once.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
        Assert.AreEqual(
            11,
            ProfiledReceiverFreeTarget.StaticField,
            "Restoring the rewritten callers must not change storage.");
    }

    private static int Increment(scoped in int value) =>
        value + 1;
}
