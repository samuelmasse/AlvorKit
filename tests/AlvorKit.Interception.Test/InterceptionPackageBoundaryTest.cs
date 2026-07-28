namespace AlvorKit.Interception.Test;

[TestClass]
public sealed class InterceptionPackageBoundaryTest
{
    /// <summary>Verifies neutral contracts do not reference the CoreCLR adapter or its generated profiler binding.</summary>
    [TestMethod]
    public void NeutralAssembly_DoesNotReferenceCoreClrBindings()
    {
        var neutral = typeof(InterceptionTarget).Assembly;
        var references = neutral
            .GetReferencedAssemblies()
            .Select(static reference => reference.Name)
            .ToArray();

        CollectionAssert.DoesNotContain(
            references,
            "AlvorKit.Interception.CoreClr");
        CollectionAssert.DoesNotContain(
            references,
            "AlvorKit.Interception.Profiler.Backend");
        Assert.AreEqual(
            "AlvorKit.Interception.CoreClr",
            typeof(InterceptionProfiler).Assembly.GetName().Name);
    }

    /// <summary>Verifies CoreCLR implementations satisfy each neutral runtime ownership contract.</summary>
    [TestMethod]
    public void CoreClrTypes_ImplementNeutralRuntimeContracts()
    {
        Assert.IsTrue(
            typeof(IInterceptionBackend).IsAssignableFrom(
                typeof(InterceptionProfiler)));
        Assert.IsTrue(
            typeof(IInterceptionPatchHandle).IsAssignableFrom(
                typeof(InterceptionPatchHandle)));
        Assert.IsTrue(
            typeof(IInterceptionHandlerTrampoline).IsAssignableFrom(
                typeof(InterceptionHandlerTrampoline)));
    }

    /// <summary>The ordinary CoreCLR namespace exports only its connection facade.</summary>
    [TestMethod]
    public void CoreClrAssembly_OrdinaryNamespaceHasMinimalFacade()
    {
        var facade = typeof(InterceptionProfiler).Assembly
            .ExportedTypes
            .Where(static type =>
                type.Namespace == "AlvorKit.Interception")
            .Select(static type => type.Name)
            .Order()
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { nameof(InterceptionProfiler) },
            facade);
    }

    /// <summary>Loaded-body and generation models remain available only through the advanced namespace.</summary>
    [TestMethod]
    public void CoreClrAssembly_AdvancedPlanningIsExplicit()
    {
        var advanced = typeof(InterceptionProfiler).Assembly
            .ExportedTypes
            .Where(static type =>
                type.Namespace ==
                "AlvorKit.Interception.CoreClr.Advanced")
            .ToArray();

        Assert.IsTrue(advanced.Length > 0);
        Assert.IsTrue(advanced.Any(static type =>
            type.Name == "LoadedMethodBodySnapshot"));
        Assert.IsTrue(advanced.Any(static type =>
            type.Name == "IInterceptionGenerationBackend"));
        Assert.IsFalse(advanced.Any(static type =>
            type.Name is "InterceptionPatchHandle" or
                "InterceptionHandlerTrampoline" or
                "InterceptionTrampolineState"));
    }

    /// <summary>Proof helpers and direct trampoline entry points are absent from the public contract.</summary>
    [TestMethod]
    public void PublicSurface_DoesNotExposeProofOrOwnershipBypassMembers()
    {
        Assert.IsNull(typeof(InterceptionMethodBody).GetMethod("ReturnInt32"));
        Assert.IsNull(typeof(InterceptionTarget).GetMethod("FromStaticInt32"));
        Assert.IsNull(typeof(InterceptionProfiler).GetMethod("InstallInt32"));
        Assert.IsNull(typeof(IInterceptionHandlerTrampoline).GetProperty(
            "EntryPoint"));
        Assert.IsFalse(typeof(InterceptionPatchHandle).IsPublic);
        Assert.IsFalse(typeof(InterceptionHandlerTrampoline).IsPublic);
        Assert.IsFalse(typeof(InterceptionTrampolineState).IsPublic);
    }
}
