namespace AlvorKit;

public sealed partial class StructInterceptionTest
{
    /// <summary>
    /// Entry and exit hooks mutate the same storage observed by the caller.
    /// </summary>
    [TestMethod]
    public void Session_MutableStructPassthroughMutatesCallerStorage()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStructRouteLifecycle.CreateMutableRoutes();
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
            Assert.IsTrue(lifecycle.AllMutablePrepared);
            Assert.IsTrue(routes.All(route => route.IsActivated));

            ProfiledStructOriginalCounters.Reset();
            var entry = -1;
            var exit = -1;
            using var session = Mock.Session();
            Mock.Struct<ProfiledMutableStructTarget>()
                .When<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructAddCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .SnapshotThisOnEntry(
                    (scoped in value) =>
                    {
                        entry = value.Value;
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
                        exit = value.Value;
                        return value.Value;
                    })
                .Passthrough();
            var target = new ProfiledMutableStructTarget(3);

            int resultValue =
                ProfiledStructAddCaller.Selected(ref target, 2);

            Assert.AreEqual(15, resultValue);
            Assert.AreEqual(115, target.Value);
            Assert.AreEqual(3, entry);
            Assert.AreEqual(115, exit);
            Assert.AreEqual(1, ProfiledStructOriginalCounters.Add);
            Mock.Struct<ProfiledMutableStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructAddCaller.Selected(
                            ref value,
                            2))
                .Once();

            var storage = new ProfiledStructStorage(4);
            Assert.AreEqual(
                16,
                ProfiledStructFieldAddCaller.Selected(
                    storage,
                    2));
            Assert.AreEqual(116, storage.Target.Value);
            ProfiledStructStorage.StaticTarget =
                new ProfiledMutableStructTarget(5);
            Assert.AreEqual(
                17,
                ProfiledStructStaticFieldAddCaller.Selected(2));
            Assert.AreEqual(
                117,
                ProfiledStructStorage.StaticTarget.Value);
            var targets =
                new[] { new ProfiledMutableStructTarget(6) };
            Assert.AreEqual(
                18,
                ProfiledStructArrayAddCaller.Selected(
                    targets,
                    0,
                    2));
            Assert.AreEqual(118, targets[0].Value);

            Assert.AreEqual(4, ProfiledStructOriginalCounters.Add);
            Assert.IsTrue(
                lifecycle.MutableWrapperEntriesAreExact,
                "Setup, four writable calls, and verification must enter " +
                "only their exact production wrappers: " +
                lifecycle.MutableWrapperEntryCounts);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllMutableRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }
}
