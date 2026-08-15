namespace AlvorKit;

public sealed partial class StructInterceptionTest
{
    /// <summary>
    /// Ref-struct arguments and results stay typed over caller-owned storage.
    /// </summary>
    [TestMethod]
    public void Session_StructMethodPreservesRefStructArgument()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledStructRefStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledStructRefStructRouteLifecycle.CreateRoutes();
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

            RunConfiguredRefStructBehavior();
            RunPassthroughRefStructBehavior();

            Assert.IsTrue(
                lifecycle.WrapperEntriesAreExact,
                "Configured and passthrough span/window calls, setup " +
                "captures, and whole-operation verification must enter " +
                "only their production wrappers: " +
                lifecycle.WrapperEntryCounts);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    private static void RunConfiguredRefStructBehavior()
    {
        ProfiledStructRefStructOriginalCounters.Reset();
        var observeEntries = new List<int>();
        var windowEntries = new List<int>();
        int observedLength = -1;
        int observedFirst = -1;
        using var session = Mock.Session();
        Mock.Struct<ProfiledStructRefStructTarget>()
            .When<int>(
                static (
                    scoped ref value) =>
                    ProfiledStructSpanCaller.Selected(
                        ref value,
                        Arg.Any<Span<int>>(0)))
            .SnapshotThisOnEntry(
                (scoped in value) =>
                {
                    observeEntries.Add(value.Value);
                    return value.Value;
                })
            .Answer(
                (ProfiledStructSpanOperation)(
                    (
                        ref value,
                        values) =>
                    {
                        _ = value;
                        observedLength = values.Length;
                        observedFirst = values[0];
                        return 97;
                    }));
        var owner =
            new ProfiledStructBehaviorWindowOwner([3, 5, 8]);
        Mock.Struct<ProfiledStructRefStructTarget>()
            .When<ProfiledStructWindow>(
                static (
                    scoped ref value) =>
                    ProfiledStructBorrowedWindowCaller.Selected(
                        ref value,
                        Arg.Any<int[]>()))
            .SnapshotThisOnEntry(
                (scoped in value) =>
                {
                    windowEntries.Add(value.Value);
                    return value.Value;
                })
            .ReturnFactory(owner.Create);
        var target = new ProfiledStructRefStructTarget(7);
        Span<int> values = [11, 13];

        Assert.AreEqual(
            97,
            ProfiledStructSpanCaller.Selected(
                ref target,
                values));
        Assert.AreEqual(2, observedLength);
        Assert.AreEqual(11, observedFirst);
        Assert.IsTrue(values.SequenceEqual([11, 13]));
        Assert.AreEqual(7, target.Value);

        ProfiledStructWindow window =
            ProfiledStructBorrowedWindowCaller.Selected(
                ref target,
                [13, 21]);
        Assert.IsTrue(window.Values.SequenceEqual([3, 5, 8]));
        owner.Set(1, 34);
        Assert.IsTrue(window.Values.SequenceEqual([3, 34, 8]));
        Assert.AreEqual(1, owner.Calls);
        Assert.AreEqual(7, target.Value);
        CollectionAssert.AreEqual(
            new[] { 7 },
            observeEntries);
        CollectionAssert.AreEqual(
            new[] { 7 },
            windowEntries);
        Assert.AreEqual(
            0,
            ProfiledStructRefStructOriginalCounters.Observe);
        Assert.AreEqual(
            0,
            ProfiledStructRefStructOriginalCounters.Window);

        VerifyOneSpanCall();
        VerifyOneWindowCall();
    }

    private static void RunPassthroughRefStructBehavior()
    {
        int observeEntry = -1;
        int observeExit = -1;
        int windowEntry = -1;
        int windowExit = -1;
        using var session = Mock.Session();
        Mock.Struct<ProfiledStructRefStructTarget>()
            .When<int>(
                static (
                    scoped ref value) =>
                    ProfiledStructSpanCaller.Selected(
                        ref value,
                        Arg.Any<Span<int>>(0)))
            .SnapshotThisOnEntry(
                (scoped in value) =>
                {
                    observeEntry = value.Value;
                    return value.Value;
                })
            .SnapshotThisOnExit(
                (scoped in value) =>
                {
                    observeExit = value.Value;
                    return value.Value;
                })
            .Passthrough();
        Mock.Struct<ProfiledStructRefStructTarget>()
            .When<ProfiledStructWindow>(
                static (
                    scoped ref value) =>
                    ProfiledStructBorrowedWindowCaller.Selected(
                        ref value,
                        Arg.Any<int[]>()))
            .SnapshotThisOnEntry(
                (scoped in value) =>
                {
                    windowEntry = value.Value;
                    return value.Value;
                })
            .SnapshotThisOnExit(
                (scoped in value) =>
                {
                    windowExit = value.Value;
                    return value.Value;
                })
            .Passthrough();
        var target = new ProfiledStructRefStructTarget(7);
        Span<int> values = [11, 17];

        Assert.AreEqual(
            18,
            ProfiledStructSpanCaller.Selected(
                ref target,
                values));
        Assert.IsTrue(values.SequenceEqual([11, 17]));
        Assert.AreEqual(18, target.Value);
        Assert.AreEqual(7, observeEntry);
        Assert.AreEqual(18, observeExit);

        int[] owner = [13, 21];
        ProfiledStructWindow window =
            ProfiledStructBorrowedWindowCaller.Selected(
                ref target,
                owner);
        Assert.IsTrue(window.Values.SequenceEqual([13, 21]));
        owner[0] = 34;
        Assert.IsTrue(window.Values.SequenceEqual([34, 21]));
        Assert.AreEqual(18, target.Value);
        Assert.AreEqual(18, windowEntry);
        Assert.AreEqual(18, windowExit);
        Assert.AreEqual(
            1,
            ProfiledStructRefStructOriginalCounters.Observe);
        Assert.AreEqual(
            1,
            ProfiledStructRefStructOriginalCounters.Window);

        VerifyOneSpanCall();
        VerifyOneWindowCall();
    }

    private static void VerifyOneSpanCall() =>
        Mock.Struct<ProfiledStructRefStructTarget>()
            .Verify<int>(
                static (
                    scoped ref value) =>
                    ProfiledStructSpanCaller.Selected(
                        ref value,
                        Arg.Any<Span<int>>(0)))
            .Once();

    private static void VerifyOneWindowCall() =>
        Mock.Struct<ProfiledStructRefStructTarget>()
            .Verify<ProfiledStructWindow>(
                static (
                    scoped ref value) =>
                    ProfiledStructBorrowedWindowCaller.Selected(
                        ref value,
                        Arg.Any<int[]>()))
            .Once();
}
