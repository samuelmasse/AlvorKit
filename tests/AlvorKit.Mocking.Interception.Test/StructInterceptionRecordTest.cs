namespace AlvorKit.Mocking.Interception.Test;

public sealed partial class StructInterceptionTest
{
    /// <summary>
    /// Type-wide record setup applies to equal values in distinct live storage.
    /// </summary>
    [TestMethod]
    public void Session_RecordStructUsesConfiguredTypeWideResult()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStructRouteLifecycle.CreateRecordRoutes();
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
            Assert.IsTrue(lifecycle.AllRecordPrepared);
            Assert.IsTrue(routes.All(route => route.IsActivated));

            ProfiledStructOriginalCounters.Reset();
            var entryValues = new List<int>();
            using var session = Mock.Session();
            Mock.Struct<ProfiledRecordStructTarget>()
                .When<int>(
                    static (
                        scoped ref value) =>
                        ProfiledRecordStructReadCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .SnapshotThisOnEntry(
                    (scoped in value) =>
                    {
                        entryValues.Add(value.Value);
                        return value.Value;
                    })
                .Return(83);
            var target = new ProfiledRecordStructTarget(7);
            var assignedCopy = target;
            var equalValue = new ProfiledRecordStructTarget(7);

            Assert.AreEqual(
                83,
                ProfiledRecordStructReadCaller.Selected(
                    ref target,
                    4));
            Assert.AreEqual(
                83,
                ProfiledRecordStructReadCaller.Selected(
                    ref assignedCopy,
                    5));
            Assert.AreEqual(
                83,
                ProfiledRecordStructReadCaller.Selected(
                    ref equalValue,
                    6));

            Assert.AreEqual(7, target.Value);
            Assert.AreEqual(7, assignedCopy.Value);
            Assert.AreEqual(7, equalValue.Value);
            Assert.IsTrue(target == assignedCopy);
            Assert.IsTrue(target == equalValue);
            CollectionAssert.AreEqual(
                new[] { 7, 7, 7 },
                entryValues);
            Assert.AreEqual(
                0,
                ProfiledStructOriginalCounters.RecordRead);

            Mock.Struct<ProfiledRecordStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledRecordStructReadCaller.Selected(
                            ref value,
                            4))
                .Once();
            Mock.Struct<ProfiledRecordStructTarget>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        ProfiledRecordStructReadCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .Exactly(3);

            Assert.AreEqual(
                0,
                ProfiledStructOriginalCounters.RecordRead);
            Assert.IsTrue(
                lifecycle.RecordWrapperEntriesAreExact,
                "Type-wide setup, three configured calls, exact and " +
                "whole-operation verification must enter only the " +
                "production record wrapper: " +
                lifecycle.RecordWrapperEntryCount);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRecordRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }
}
