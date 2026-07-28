namespace AlvorKit.Mocking.Interception.Test;

public sealed partial class FieldInterceptionTest
{
    /// <summary>
    /// Reference field reads and writes preserve their exact nullable type.
    /// </summary>
    [TestMethod]
    public void Session_ReferenceFieldTransformsStayTyped()
    {
        RequireProfiledHost();
        var target = new ProfiledReceiverFreeTarget(47);
        Assert.IsNull(target.InstanceReferenceField);

        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledReferenceFieldTransformRouteLifecycle(
                profiler,
                target);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledReferenceFieldTransformRouteLifecycle.CreateRoutes();
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
            Assert.IsNull(
                target.InstanceReferenceField,
                "Inert preparation must preserve the receiver's storage.");

            var field = Mock.Field<
                ProfiledReceiverFreeTarget,
                string?>(
                nameof(
                    ProfiledReceiverFreeTarget
                        .InstanceReferenceField));
            using var session = Mock.Session();
            Mock.WhenFieldWrite(
                    target,
                    field,
                    () => Arg.Any<string?>())
                .Transform(AppendStored);
            Mock.WhenFieldRead(target, field)
                .Transform(AppendReturned);

            ProfiledReferenceFieldTransformWriteCaller.Selected(
                target,
                "value");
            string? transformed =
                ProfiledReferenceFieldTransformReadCaller.Selected(
                    target);

            Assert.AreEqual(
                "value:stored",
                target.InstanceReferenceField);
            Assert.AreEqual(
                "value:stored:returned",
                transformed);
            Mock.VerifyFieldWrite(
                    target,
                    field,
                    () => "value")
                .Once();
            Mock.VerifyFieldRead(target, field)
                .Once();
            Assert.IsTrue(
                lifecycle.AllWrappersEnteredExactlyOnce,
                "Both rewritten reference-field sites must enter their " +
                "exact production receiver-free wrappers once.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
        Assert.AreEqual(
            "value:stored",
            target.InstanceReferenceField,
            "Restoring the rewritten callers must preserve the receiver.");
    }

    private static string? AppendStored(
        scoped in string? value) =>
        value + ":stored";

    private static string? AppendReturned(
        scoped in string? value) =>
        value + ":returned";
}
