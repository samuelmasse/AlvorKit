namespace AlvorKit;

/// <summary>Exercises exact typed instance-field interception.</summary>
[TestClass]
public sealed partial class FieldInterceptionTest
{
    /// <summary>
    /// Instance field observe and transform use the selected receiver and typed values.
    /// </summary>
    [TestMethod]
    public void Session_InstanceFieldObserveAndTransformRemainReceiverScoped()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledInstanceFieldRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledInstanceFieldRouteLifecycle.CreateRoutes();
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

            var target = new ProfiledReceiverFreeTarget(13);
            var other = new ProfiledReceiverFreeTarget(17);
            var field = Mock.Field<
                ProfiledReceiverFreeTarget,
                int>(
                nameof(ProfiledReceiverFreeTarget.InstanceField));
            var observed = 0;
            using var session = Mock.Session();
            Mock.WhenFieldWrite(
                    target,
                    field,
                    () => Arg.Any<int>())
                .Observe(
                    (scoped in value) => observed = value);
            Mock.WhenFieldRead(target, field)
                .Transform(Double);

            ProfiledWriteInstanceFieldCaller.Selected(
                target,
                19);
            int transformed =
                ProfiledReadInstanceFieldCaller.Selected(target);
            int untouched =
                ProfiledReadInstanceFieldCaller.Selected(other);

            Assert.AreEqual(19, observed);
            Assert.AreEqual(19, target.InstanceField);
            Assert.AreEqual(38, transformed);
            Assert.AreEqual(17, untouched);
            Mock.VerifyFieldWrite(target, field, () => 19)
                .Once();
            Mock.VerifyFieldRead(target, field)
                .Once();
            Mock.VerifyFieldRead(other, field)
                .Once();
            Assert.IsTrue(
                lifecycle.AllRewritten,
                "Both field opcodes must enter their real rewritten " +
                "production wrappers.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>
    /// Two read sites for one static field retain independent site behavior.
    /// </summary>
    [TestMethod]
    public void Session_FieldReadAtSiteDistinguishesSameField()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledStaticFieldAtSiteRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStaticFieldAtSiteRouteLifecycle.CreateRoutes();
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
            ProfiledReceiverFreeTarget.StaticField = 43;
            var field = Mock.Field<
                ProfiledReceiverFreeTarget,
                int>(
                nameof(ProfiledReceiverFreeTarget.StaticField));
            using var session = Mock.Session();
            var first = Mock.Site(
                () => ProfiledReadStaticFieldCaller.Selected());
            var second = Mock.Site(
                () => ProfiledReadStaticFieldSecondCaller.Selected());
            Mock.WhenFieldRead(field)
                .AtSite(first)
                .Return(107);

            Assert.AreEqual(
                107,
                ProfiledReadStaticFieldCaller.Selected());
            Assert.AreEqual(
                43,
                ProfiledReadStaticFieldSecondCaller.Selected());

            Mock.VerifyFieldRead(field)
                .AtSite(first)
                .Once();
            Mock.VerifyFieldRead(field)
                .AtSite(second)
                .Once();
            Assert.IsTrue(
                lifecycle.AllRewritten,
                "Both field-read opcodes must enter their real rewritten " +
                "production wrappers.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    private static int Double(scoped in int value) =>
        value * 2;

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
