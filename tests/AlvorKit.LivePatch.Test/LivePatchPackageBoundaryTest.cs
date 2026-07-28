namespace AlvorKit.LivePatch.Test;

[TestClass]
public sealed class LivePatchPackageBoundaryTest
{
    /// <summary>Verifies LivePatch consumes only the runtime-neutral interception assembly.</summary>
    [TestMethod]
    public void LivePatchAssembly_DoesNotReferenceCoreClrBindings()
    {
        var references = typeof(LivePatchSession).Assembly
            .GetReferencedAssemblies()
            .Select(static reference => reference.Name)
            .ToArray();

        CollectionAssert.DoesNotContain(
            references,
            "AlvorKit.Interception.CoreClr");
        CollectionAssert.DoesNotContain(
            references,
            "AlvorKit.Interception.Profiler.Backend");
    }

    /// <summary>Verifies the public session constructor accepts the neutral backend contract.</summary>
    [TestMethod]
    public void LivePatchSession_ConstructorUsesNeutralBackend()
    {
        var constructor = typeof(LivePatchSession).GetConstructors().Single();
        var parameters = constructor.GetParameters();

        Assert.AreEqual(
            typeof(IInterceptionBackend),
            parameters[0].ParameterType);
        Assert.AreEqual(
            typeof(InjectorScopeGraph),
            parameters[1].ParameterType);
    }
}
