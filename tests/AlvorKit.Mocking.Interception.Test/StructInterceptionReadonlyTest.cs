namespace AlvorKit.Mocking.Interception.Test;

public sealed partial class StructInterceptionTest
{
    /// <summary>
    /// Readonly metadata preserves storage and rejects writable hooks.
    /// </summary>
    [TestMethod]
    public void Session_ReadonlyStructPreservesStorageAndRejectsMutation()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStructRouteLifecycle.CreateReadonlyRoutes();
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
            Assert.IsTrue(lifecycle.AllReadonlyPrepared);
            Assert.IsTrue(routes.All(route => route.IsActivated));

            ProfiledStructOriginalCounters.Reset();
            var entry = -1;
            using var session = Mock.Session();
            Mock.Struct<ProfiledReadonlyStructTarget>()
                .When<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructReadCaller.Selected(
                            in value,
                            Arg.Any<int>()))
                .SnapshotThisOnEntry(
                    (scoped in value) =>
                    {
                        entry = value.Value;
                        return value.Value;
                    })
                .Return(71);
            var target = new ProfiledReadonlyStructTarget(5);

            Assert.AreEqual(
                71,
                ProfiledStructReadCaller.Selected(in target, 3));
            Assert.AreEqual(5, target.Value);
            Assert.AreEqual(5, entry);
            Assert.AreEqual(0, ProfiledStructOriginalCounters.Read);

            Mock.Struct<ProfiledReadonlyStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructReadCaller.Selected(
                            in value,
                            3))
                .Once();
            Mock.Struct<ProfiledReadonlyStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledStructReadCaller.Selected(
                            in value,
                            Arg.Any<int>()))
                .Once();

            bool mutationRan = false;
            var rejected =
                Mock.Struct<ProfiledReadonlyStructTarget>()
                    .When<int>(
                        static (
                            scoped ref value) =>
                            ProfiledStructReadCaller.Selected(
                                in value,
                                9))
                    .MutateThisOnEntry(
                        (
                            scoped ref
                                value) =>
                        {
                            mutationRan = true;
                            _ = value;
                        });

            var error = Assert.ThrowsExactly<MockException>(
                rejected.Passthrough);
            Assert.AreEqual(
                $"Readonly struct receiver " +
                $"'{typeof(ProfiledReadonlyStructTarget)}' cannot use " +
                "entry or exit mutation.",
                error.Message);
            Assert.IsFalse(mutationRan);
            Assert.AreEqual(5, target.Value);
            Assert.AreEqual(0, ProfiledStructOriginalCounters.Read);
            Assert.IsTrue(
                lifecycle.ReadonlyWrapperEntriesAreExact,
                "Configured call history, exact/whole-operation " +
                "verification, and readonly rejection must enter only " +
                "the production wrapper: " +
                lifecycle.ReadonlyWrapperEntryCount);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllReadonlyRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }
}
