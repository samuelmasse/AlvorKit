namespace AlvorKit;

/// <summary>Exercises managed-reference aliases through real profiled callers.</summary>
[TestClass]
public sealed class ConcreteAdvancedManagedReferenceInterceptionTest
{
    /// <summary>
    /// Sealed configured and partial mutable and readonly returns preserve exact alias identity.
    /// </summary>
    [TestMethod]
    public void ManagedReferences_SealedAndPartialPreserveAliases()
    {
        RequireProfiledHost();
        IInterceptionBackend profiler = InterceptionProfiler.Connect();
        var lifecycle =
            new ProfiledManagedReferenceRouteLifecycle(profiler);
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var routes =
            ProfiledManagedReferenceRouteLifecycle.CreateRoutes();
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

            var configured =
                Mock.Create<ProfiledManagedReferenceTarget>();
            var owner = new ProfiledAliasOwner([55, 89]);
            Mock.WhenRef(
                    () => ref ProfiledMutableReferenceCaller.Selected(
                        configured))
                .ReturnRef(owner.Mutable);
            Mock.WhenRefReadonly(
                    () => ref ProfiledReadOnlyReferenceCaller.Selected(
                        configured))
                .ReturnRef(owner.ReadOnly);

            ref int configuredMutable =
                ref ProfiledMutableReferenceCaller.Selected(configured);
            ref readonly int configuredReadOnly =
                ref ProfiledReadOnlyReferenceCaller.Selected(configured);
            ref int ownerMutable = ref owner.Mutable();
            ref readonly int ownerReadOnly = ref owner.ReadOnly();
            Assert.IsTrue(Unsafe.AreSame(
                ref configuredMutable,
                ref ownerMutable));
            Assert.IsTrue(Unsafe.AreSame(
                ref Unsafe.AsRef(in configuredReadOnly),
                ref Unsafe.AsRef(in ownerReadOnly)));

            var partial = Mock.Partial(
                new ProfiledManagedReferenceTarget([144, 233]));
            ref int partialMutable =
                ref ProfiledMutableReferenceCaller.Selected(partial);
            ref readonly int partialReadOnly =
                ref ProfiledReadOnlyReferenceCaller.Selected(partial);
            Assert.IsTrue(Unsafe.AreSame(
                ref partialMutable,
                ref partial.AliasStorage[0]));
            Assert.IsTrue(Unsafe.AreSame(
                ref Unsafe.AsRef(in partialReadOnly),
                ref partial.AliasStorage[1]));
            Assert.AreEqual(2, partial.OriginalCalls);
            Mock.Verify(() =>
                    ProfiledMutableReferenceCaller.Selected(partial))
                .Once();
            Mock.Verify(() =>
                    ProfiledReadOnlyReferenceCaller.Selected(partial))
                .Once();
            Mock.VerifyNoOtherCalls(partial);
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
