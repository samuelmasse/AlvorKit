namespace AlvorKit;

/// <summary>Exercises live value receivers through rewritten caller sites.</summary>
[TestClass]
public sealed partial class StructInterceptionTest
{
    /// <summary>
    /// No session preserves mutable, readonly, record, and constrained originals.
    /// </summary>
    [TestMethod]
    public void NoSession_StructMethodsRunOriginalWithoutBoxing()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledStructRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes = ProfiledStructRouteLifecycle.CreateRoutes();
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

            ProfiledStructOriginalCounters.Reset();
            var mutable = new ProfiledMutableStructTarget(3);
            var readOnly = new ProfiledReadonlyStructTarget(5);
            var record = new ProfiledRecordStructTarget(7);

            Assert.AreEqual(
                5,
                ProfiledStructAddCaller.Selected(
                    ref mutable,
                    2));
            Assert.AreEqual(5, mutable.Value);
            Assert.AreEqual(
                8,
                ProfiledStructReadCaller.Selected(
                    in readOnly,
                    3));
            Assert.AreEqual(
                11,
                ProfiledRecordStructReadCaller.Selected(
                    ref record,
                    4));
            Assert.IsFalse(
                RuntimeHelpers.IsReferenceOrContainsReferences<
                    ProfiledMutableStructTarget>());

            var originalWindow =
                ProfiledStructWindowCaller.Selected(
                    ref mutable,
                    [29, 31]);
            Assert.IsTrue(
                originalWindow.Values.SequenceEqual([29, 31]));

            var storage = new ProfiledStructStorage(11);
            Assert.AreEqual(
                16,
                ProfiledStructFieldAddCaller.Selected(
                    storage,
                    5));
            Assert.AreEqual(16, storage.Target.Value);
            ProfiledStructStorage.StaticTarget =
                new ProfiledMutableStructTarget(13);
            Assert.AreEqual(
                19,
                ProfiledStructStaticFieldAddCaller.Selected(6));
            Assert.AreEqual(
                19,
                ProfiledStructStorage.StaticTarget.Value);

            var targets =
                new[] { new ProfiledMutableStructTarget(23) };
            Assert.AreEqual(
                31,
                ProfiledStructArrayAddCaller.Selected(
                    targets,
                    0,
                    8));
            Assert.AreEqual(31, targets[0].Value);
            var constrained =
                new ProfiledMutableStructTarget(17);
            Assert.AreEqual(
                24,
                ProfiledStructConstrainedCaller.Selected(
                    ref constrained,
                    7));
            Assert.AreEqual(24, constrained.Value);

            Assert.AreEqual(4, ProfiledStructOriginalCounters.Add);
            Assert.AreEqual(1, ProfiledStructOriginalCounters.Read);
            Assert.AreEqual(
                1,
                ProfiledStructOriginalCounters.RecordRead);
            Assert.AreEqual(1, ProfiledStructOriginalCounters.Window);
            Assert.AreEqual(
                1,
                ProfiledStructOriginalCounters.Constrained);
            Assert.IsTrue(
                lifecycle.AllWrappersEnteredExactlyOnce,
                "Every rewritten value-receiver call site must enter " +
                "its exact production wrapper once.");
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

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
