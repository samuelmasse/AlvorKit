namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Exercises constructed generic value receivers through a real profiler.</summary>
[TestClass]
public sealed class StructInterceptionConstructedGenericTest
{
    /// <summary>
    /// Exact closed routes preserve configured and passthrough behavior over live storage.
    /// </summary>
    [TestMethod]
    public void Session_ConstructedGenericStructMutatesLiveStorage()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledConstructedGenericStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        MockInterceptionRoute[] routes =
            ProfiledConstructedGenericStructRouteLifecycle.CreateRoutes();
        MockInterceptionActivation? activation = null;
        try
        {
            MockInterceptionPreparationResult preparation =
                coordinator.PrepareAndActivate(routes);
            activation = preparation.Activation;
            Assert.IsTrue(
                preparation.IsSuccessful,
                string.Join(
                    Environment.NewLine,
                    preparation.Diagnostics.Select(
                        diagnostic => diagnostic.Message)));
            Assert.IsNotNull(activation);
            Assert.IsTrue(activation.IsActive);
            Assert.AreEqual(
                InterceptionState.Active,
                lifecycle.PreparationCompletion?.State);
            Assert.IsTrue(lifecycle.HasExactConstructedMetadata);
            Assert.IsTrue(routes.All(route => route.IsActivated));
            Assert.IsFalse(
                RuntimeHelpers.IsReferenceOrContainsReferences<
                    ProfiledConstructedGenericStructTarget<int>>());

            ProfiledConstructedGenericStructOriginalCounter<int>.Reset();
            ProfiledConstructedGenericStructOriginalCounter<string>.Reset();
            using var session = Mock.Session();
            Mock.Struct<ProfiledConstructedGenericStructTarget<int>>()
                .When<int>(
                    static (
                        scoped ref
                                                        value) =>
                        ProfiledConstructedGenericStructCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .Passthrough();
            Mock.Struct<ProfiledConstructedGenericStructTarget<string>>()
                .When<string>(
                    static (
                        scoped ref
                                                        value) =>
                        ProfiledConstructedGenericStructCaller.Selected(
                            ref value,
                            Arg.Any<string>()))
                .Return("configured");
            var integer =
                new ProfiledConstructedGenericStructTarget<int>(3);
            var text =
                new ProfiledConstructedGenericStructTarget<string>("seed");

            Assert.AreEqual(
                11,
                ProfiledConstructedGenericStructCaller.Selected(
                    ref integer,
                    11));
            Assert.AreEqual(11, integer.Value);
            Assert.AreEqual(
                "configured",
                ProfiledConstructedGenericStructCaller.Selected(
                    ref text,
                    "input"));
            Assert.AreEqual("seed", text.Value);
            Assert.AreEqual(
                1,
                ProfiledConstructedGenericStructOriginalCounter<int>.Calls);
            Assert.AreEqual(
                0,
                ProfiledConstructedGenericStructOriginalCounter<string>.Calls);

            Mock.Struct<ProfiledConstructedGenericStructTarget<int>>()
                .Verify<int>(
                    static (
                        scoped ref
                                                        value) =>
                        ProfiledConstructedGenericStructCaller.Selected(
                            ref value,
                            11))
                .Once();
            Mock.Struct<ProfiledConstructedGenericStructTarget<int>>()
                .Verify<int>(
                    static (
                        scoped ref
                                                        value) =>
                        ProfiledConstructedGenericStructCaller.Selected(
                            ref value,
                            Arg.Any<int>()))
                .Exactly(1);
            Mock.Struct<ProfiledConstructedGenericStructTarget<string>>()
                .Verify<string>(
                    static (
                        scoped ref
                                                        value) =>
                        ProfiledConstructedGenericStructCaller.Selected(
                            ref value,
                            "input"))
                .Once();
            Mock.Struct<ProfiledConstructedGenericStructTarget<string>>()
                .Verify<string>(
                    static (
                        scoped ref
                                                        value) =>
                        ProfiledConstructedGenericStructCaller.Selected(
                            ref value,
                            Arg.Any<string>()))
                .Exactly(1);
            session.VerifySequence(
                static () =>
                {
                    var value =
                        new ProfiledConstructedGenericStructTarget<int>(3);
                    _ = ProfiledConstructedGenericStructCaller.Selected(
                        ref value,
                        11);
                },
                static () =>
                {
                    var value =
                        new ProfiledConstructedGenericStructTarget<string>(
                            "seed");
                    _ = ProfiledConstructedGenericStructCaller.Selected(
                        ref value,
                        "input");
                });

            Assert.AreEqual(7, lifecycle.IntegerRouteEntries);
            Assert.AreEqual(7, lifecycle.StringRouteEntries);
            Assert.AreEqual(11, integer.Value);
            Assert.AreEqual("seed", text.Value);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.AreEqual(
            InterceptionState.Removed,
            lifecycle.RemovalCompletion?.State);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
        ProfiledConstructedGenericStructOriginalCounter<int>.Reset();
        ProfiledConstructedGenericStructOriginalCounter<string>.Reset();
        var restoredInteger =
            new ProfiledConstructedGenericStructTarget<int>(17);
        var restoredText =
            new ProfiledConstructedGenericStructTarget<string>("before");
        Assert.AreEqual(
            23,
            ProfiledConstructedGenericStructCaller.Selected(
                ref restoredInteger,
                23));
        Assert.AreEqual(23, restoredInteger.Value);
        Assert.AreEqual(
            "after",
            ProfiledConstructedGenericStructCaller.Selected(
                ref restoredText,
                "after"));
        Assert.AreEqual("after", restoredText.Value);
        Assert.AreEqual(
            1,
            ProfiledConstructedGenericStructOriginalCounter<int>.Calls);
        Assert.AreEqual(
            1,
            ProfiledConstructedGenericStructOriginalCounter<string>.Calls);
    }

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
