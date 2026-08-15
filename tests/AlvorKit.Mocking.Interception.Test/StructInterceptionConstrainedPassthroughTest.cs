namespace AlvorKit;

public sealed partial class StructInterceptionTest
{
    /// <summary>
    /// Constrained passthrough and hooks mutate the caller's exact storage.
    /// </summary>
    [TestMethod]
    public void Session_ConstrainedStructPassthroughMutatesLiveStorage()
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
            var exitValues = new List<int>();
            using var session = Mock.Session();
            Mock.Struct<ProfiledMutableStructTarget>()
                .When<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructConstrainedCaller.Selected(
                            ref value,
                            3))
                .SnapshotThisOnEntry(
                    (scoped in value) =>
                    {
                        entryValues.Add(value.Value);
                        return value.Value;
                    })
                .MutateThisOnEntry(
                    static (
                        scoped ref value) =>
                        value.Value += 10)
                .MutateThisOnExit(
                    static (
                        scoped ref value) =>
                        value.Value += 100)
                .SnapshotThisOnExit(
                    (scoped in value) =>
                    {
                        exitValues.Add(value.Value);
                        return value.Value;
                    })
                .Passthrough();
            var target = new ProfiledMutableStructTarget(2);

            Assert.AreEqual(
                15,
                ProfiledStructConstrainedCaller.Selected(
                    ref target,
                    3));
            Assert.AreEqual(115, target.Value);
            CollectionAssert.AreEqual(
                new[] { 2 },
                entryValues);
            CollectionAssert.AreEqual(
                new[] { 115 },
                exitValues);
            Assert.AreEqual(
                1,
                ProfiledStructOriginalCounters.Constrained);

            Mock.Struct<ProfiledMutableStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructConstrainedCaller.Selected(
                            ref value,
                            3))
                .Once();
            Mock.Struct<ProfiledMutableStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructConstrainedCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .Exactly(1);

            Assert.AreEqual(115, target.Value);
            Assert.AreEqual(
                1,
                ProfiledStructOriginalCounters.Constrained);
            Assert.IsTrue(
                lifecycle
                    .ConstrainedPassthroughWrapperEntriesAreExact,
                "Exact constrained passthrough setup, call, and " +
                "verification must enter only the closed generic " +
                "production wrapper: " +
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
