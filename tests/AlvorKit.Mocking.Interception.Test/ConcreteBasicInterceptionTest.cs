namespace AlvorKit;

/// <summary>Exercises the concrete basic matrix through real profiled callers.</summary>
[TestClass]
public sealed class ConcreteBasicInterceptionTest
{
    /// <summary>
    /// Strict, loose, configured, sequence, argument, ref/out, event, property, and method paths remain exact.
    /// </summary>
    [TestMethod]
    public void SealedNonvirtual_BehaviorMatrix_UsesInterception()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledBasicRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes = ProfiledBasicRouteLifecycle.CreateRoutes();
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

            var strict = Mock.Create<ProfiledBasicTarget>();
            var loose = Mock.CreateLoose<ProfiledBasicTarget>();
            Assert.ThrowsExactly<MockException>(
                () => ProfiledBasicAddCaller.Selected(strict, 1, 2));
            Assert.AreEqual(
                0,
                ProfiledBasicAddCaller.Selected(loose, 1, 2));

            var configured = Mock.Create<ProfiledBasicTarget>();
            Mock.When(() => ProfiledBasicAddCaller.Selected(
                    configured,
                    Arg.Any<int>(),
                    2))
                .Answer(call => call.Argument<int>(0) * 10);
            Assert.AreEqual(
                70,
                ProfiledBasicAddCaller.Selected(configured, 7, 2));
            Assert.ThrowsExactly<MockException>(
                () => ProfiledBasicAddCaller.Selected(
                    configured,
                    7,
                    3));

            var sequenced = Mock.Create<ProfiledBasicTarget>();
            Mock.When(() =>
                    ProfiledBasicGetNumberCaller.Selected(sequenced))
                .ReturnSequence(11, 13);
            Assert.AreEqual(
                11,
                ProfiledBasicGetNumberCaller.Selected(sequenced));
            Assert.AreEqual(
                13,
                ProfiledBasicGetNumberCaller.Selected(sequenced));
            Assert.AreEqual(
                13,
                ProfiledBasicGetNumberCaller.Selected(sequenced));

            var assigned = 0;
            Mock.When(() =>
                    ProfiledBasicSetNumberCaller.Selected(
                        configured,
                        19))
                .Do(call => assigned = call.Argument<int>(0));
            ProfiledBasicSetNumberCaller.Selected(configured, 19);
            Assert.AreEqual(19, assigned);

            int setup = 5;
            Mock.When(() => ProfiledBasicMutateCaller.Selected(
                    configured,
                    ref setup,
                    out _))
                .Answer(call =>
                {
                    call.SetReference(0, 17);
                    call.SetReference(1, 34);
                    return 51;
                });
            int value = 5;
            Assert.AreEqual(
                51,
                ProfiledBasicMutateCaller.Selected(
                    configured,
                    ref value,
                    out int doubled));
            Assert.AreEqual(17, value);
            Assert.AreEqual(34, doubled);

            var raised = 0;
            void handler(object? _1, EventArgs _2) => raised++;
            EventHandler? handlers = null;
            Mock.When(() => ProfiledBasicAddChangedCaller.Selected(
                    configured,
handler))
                .Do((EventHandler added) => handlers += added);
            Mock.When(() => ProfiledBasicRemoveChangedCaller.Selected(
                    configured,
handler))
                .Do((EventHandler removed) => handlers -= removed);
            ProfiledBasicAddChangedCaller.Selected(configured, handler);
            handlers?.Invoke(configured, EventArgs.Empty);
            Assert.AreEqual(1, raised);
            ProfiledBasicRemoveChangedCaller.Selected(
                configured,
handler);
            handlers?.Invoke(configured, EventArgs.Empty);
            Assert.AreEqual(1, raised);

            Mock.Verify(() =>
                    ProfiledBasicAddCaller.Selected(
                        configured,
                        7,
                        2))
                .Once();
            Mock.Verify(() =>
                    ProfiledBasicGetNumberCaller.Selected(sequenced))
                .Exactly(3);
            Mock.Verify(() =>
                    ProfiledBasicSetNumberCaller.Selected(
                        configured,
                        19))
                .Once();
            Mock.Verify(() =>
                    ProfiledBasicAddChangedCaller.Selected(
                        configured,
handler))
                .Once();
            Mock.Verify(() =>
                    ProfiledBasicRemoveChangedCaller.Selected(
                        configured,
handler))
                .Once();
            Assert.AreEqual(0, configured.OriginalCalls);
            Assert.AreEqual(0, configured.EventAccessorCalls);
        }
        finally
        {
            activation?.Dispose();
        }

        Assert.IsTrue(lifecycle.AllRemoved);
        Assert.IsTrue(routes.All(route => !route.IsActivated));
    }

    /// <summary>
    /// Closed generic types and constructed generic methods retain independent value and reference setups.
    /// </summary>
    [TestMethod]
    public void ConcreteGenerics_ConstructionsRemainIndependent()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle = new ProfiledGenericsRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes = ProfiledGenericsRouteLifecycle.CreateRoutes();
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

            var integers =
                Mock.Create<ProfiledGenericTarget<int>>();
            var strings =
                Mock.Create<ProfiledGenericTarget<string>>();
            Mock.When(() =>
                    ProfiledClosedGenericEchoCaller.Selected(
                        integers,
                        7))
                .Return(70);
            Mock.When(() =>
                    ProfiledClosedGenericValueCaller.Selected(integers))
                .Return(71);
            Mock.When(() =>
                    ProfiledClosedGenericEchoCaller.Selected(
                        strings,
                        "seven"))
                .Return("seventy");
            Mock.When(() =>
                    ProfiledClosedGenericValueCaller.Selected(strings))
                .Return("seventy-one");

            Assert.AreEqual(
                70,
                ProfiledClosedGenericEchoCaller.Selected(integers, 7));
            Assert.AreEqual(
                71,
                ProfiledClosedGenericValueCaller.Selected(integers));
            Assert.AreEqual(
                "seventy",
                ProfiledClosedGenericEchoCaller.Selected(
                    strings,
                    "seven"));
            Assert.AreEqual(
                "seventy-one",
                ProfiledClosedGenericValueCaller.Selected(strings));

            var constructed =
                Mock.Create<ProfiledConstructedGenericTarget>();
            Mock.When(() =>
                    ProfiledConstructedGenericEchoCaller.Selected(
                        constructed,
                        11))
                .Return(110);
            Mock.When(() =>
                    ProfiledConstructedGenericEchoCaller.Selected(
                        constructed,
                        "eleven"))
                .Return("one hundred ten");

            Assert.AreEqual(
                110,
                ProfiledConstructedGenericEchoCaller.Selected(
                    constructed,
                    11));
            Assert.AreEqual(
                "one hundred ten",
                ProfiledConstructedGenericEchoCaller.Selected(
                    constructed,
                    "eleven"));
            Assert.ThrowsExactly<MockException>(
                () => ProfiledConstructedGenericEchoCaller.Selected(
                    constructed,
                    12));
            Assert.AreEqual(0, constructed.OriginalCalls);
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
