namespace AlvorKit.Mocking.Interception.Test;

public sealed partial class StructInterceptionTest
{
    /// <summary>
    /// A real generic constrained caller preserves its concrete live receiver.
    /// </summary>
    [TestMethod]
    public void Session_ConstrainedStructUsesConfiguredResult()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStructRouteLifecycle.CreateConstrainedRoutes();
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
            Assert.IsTrue(lifecycle.AllConstrainedPrepared);
            Assert.IsTrue(
                lifecycle.ConstrainedCallerHasExactConcreteReceiver);
            Assert.IsTrue(routes.All(route => route.IsActivated));

            ProfiledStructOriginalCounters.Reset();
            var entryValues = new List<int>();
            using var session = Mock.Session();
            Mock.Struct<ProfiledMutableStructTarget>()
                .When<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructConstrainedCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .SnapshotThisOnEntry(
                    (scoped in value) =>
                    {
                        entryValues.Add(value.Value);
                        return value.Value;
                    })
                .Return(89);
            var target = new ProfiledMutableStructTarget(13);

            Assert.AreEqual(13, target.Value);
            Assert.AreEqual(
                89,
                ProfiledStructConstrainedCaller.Selected(
                    ref target,
                    5));
            Assert.AreEqual(13, target.Value);
            CollectionAssert.AreEqual(
                new[] { 13 },
                entryValues);
            Assert.AreEqual(
                0,
                ProfiledStructOriginalCounters.Constrained);

            Mock.Struct<ProfiledMutableStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructConstrainedCaller.Selected(
                            ref value,
                            5))
                .Once();
            Mock.Struct<ProfiledMutableStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructConstrainedCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .Exactly(1);

            Assert.AreEqual(13, target.Value);
            Assert.AreEqual(
                0,
                ProfiledStructOriginalCounters.Constrained);
            Assert.IsTrue(
                lifecycle.ConstrainedWrapperEntriesAreExact,
                "Configured dispatch, exact verification, and " +
                "whole-operation verification must enter only the " +
                "closed generic constrained wrapper: " +
                lifecycle.ConstrainedWrapperEntryCount);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllConstrainedRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }
}
