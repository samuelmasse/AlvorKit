namespace AlvorKit;

/// <summary>Exercises the wide concrete operation through a real profiled caller.</summary>
[TestClass]
public sealed class ConcreteAdvancedWideInterceptionTest
{
    /// <summary>
    /// Forty-eight declared value, ref, and span parameters preserve indices and ref writeback.
    /// </summary>
    [TestMethod]
    public void WideCall_RefAndSpanParametersPreserveDeclaredPositions()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledWideRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var route = ProfiledWideRouteLifecycle.CreateRoute();
        MockInterceptionActivation? activation = null;
        try
        {
            var result = coordinator.PrepareAndActivate([route]);
            activation = result.Activation;
            Assert.IsTrue(
                result.IsSuccessful,
                string.Join(
                    Environment.NewLine,
                    result.Diagnostics.Select(
                        diagnostic => diagnostic.Message)));
            Assert.IsNotNull(activation);
            Assert.IsTrue(activation.IsActive);
            Assert.IsTrue(route.IsActivated);
            Assert.IsTrue(lifecycle.IsPrepared);

            var target = Mock.Create<ProfiledWideTarget>();
            int[] values = Sequence(100);
            int[] setupReferences = Sequence(200);
            Mock.When(() => ProfiledWideCaller.Selected(
                    target,
                    values,
                    setupReferences,
                    null))
                .Answer(call =>
                {
                    for (var index = 0; index < 16; index++)
                    {
                        Assert.AreEqual(
                            100 + index,
                            call.Argument<int>(index * 3));
                        Assert.AreEqual(
                            200 + index,
                            call.Argument<int>((index * 3) + 1));
                        call.SetReference(
                            (index * 3) + 1,
                            700 + index);
                    }

                    return 4800;
                });
            int[] references = Sequence(200);

            int resultValue = ProfiledWideCaller.Selected(
                target,
                values,
                references,
                Sequence(300));

            Assert.AreEqual(4800, resultValue);
            CollectionAssert.AreEqual(
                Sequence(700),
                references);
            Assert.AreEqual(0, target.OriginalCalls);
            int[] verificationReferences = Sequence(200);
            Mock.Verify(() => ProfiledWideCaller.Selected(
                    target,
                    values,
                    verificationReferences,
                    null))
                .Once();
            Mock.VerifyNoOtherCalls(target);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.IsRemoved);
        Assert.IsFalse(route.IsActivated);
    }

    private static int[] Sequence(int start) =>
        [.. Enumerable.Range(start, 16)];

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
