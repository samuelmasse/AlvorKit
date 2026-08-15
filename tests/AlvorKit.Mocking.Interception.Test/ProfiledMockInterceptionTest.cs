namespace AlvorKit;

/// <summary>Proves one public Mocking behavior through a real ReJITted owned caller.</summary>
[TestClass]
public sealed class ProfiledMockInterceptionTest
{
    /// <summary>
    /// Configures, invokes, and verifies a sealed nonvirtual operation while other receivers and callers fall back.
    /// </summary>
    [TestMethod]
    public void SelectedCallerUsesRealMockingWrapperAndPreservesFallbacks()
    {
        RequireProfiledHost();
        var operation = ProfiledMockProfiler.Operation;
        Assert.IsTrue(operation.DeclaringType!.IsSealed);
        Assert.IsFalse(operation.IsVirtual);

        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledMockRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var route = new MockInterceptionRoute(
            "ProfiledMockCaller.Selected::ProfiledMockTarget.Calculate");
        MockInterceptionActivation? activation = null;
        try
        {
            var result =
                coordinator.PrepareAndActivate([route]);
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
            Assert.IsTrue(
                lifecycle.PreparationCompletion.HasValue);
            var preparation =
                lifecycle.PreparationCompletion.Value;
            Assert.AreEqual(
                InterceptionState.Active,
                preparation.State);
            Assert.IsTrue(preparation.ParameterCallbacks >= 1);
            Assert.AreEqual(15, lifecycle.ActivationProbeResult);
            Assert.AreEqual(
                1,
                lifecycle.ActivationProbeOriginalCalls);
            Assert.AreEqual(
                0,
                lifecycle.ActivationProbeHandlerCalls);

            var mocked = Mock.Create<ProfiledMockTarget>();
            Mock.When(() =>
                    ProfiledMockCaller.Selected(mocked, 7))
                .Return(70);

            var ordinary = new ProfiledMockTarget();
            Assert.AreEqual(
                13,
                ProfiledMockCaller.Selected(ordinary, 3));
            Assert.AreEqual(1, ordinary.OriginalCalls);

            Assert.AreEqual(
                14,
                ProfiledMockCaller.Unselected(mocked, 4));
            Assert.AreEqual(1, mocked.OriginalCalls);

            Assert.AreEqual(
                70,
                ProfiledMockCaller.Selected(mocked, 7));
            Assert.AreEqual(1, mocked.OriginalCalls);
            Mock.Verify(() =>
                    ProfiledMockCaller.Selected(mocked, 7))
                .Once();
            Mock.VerifyNoOtherCalls(mocked);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsFalse(route.IsActivated);
        Assert.AreEqual(
            InterceptionState.Removed,
            lifecycle.RemovalCompletion?.State);
        Assert.IsTrue(lifecycle.TrampolineRetired);
        var afterRemoval = new ProfiledMockTarget();
        Assert.AreEqual(
            12,
            ProfiledMockCaller.Selected(afterRemoval, 2));
        Assert.AreEqual(1, afterRemoval.OriginalCalls);
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
