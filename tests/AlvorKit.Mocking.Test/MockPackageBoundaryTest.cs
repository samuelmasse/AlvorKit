using System.Reflection.Metadata;

namespace AlvorKit;

/// <summary>Freezes the instrumentation-neutral core assembly boundary.</summary>
[TestClass]
public sealed class MockPackageBoundaryTest
{
    /// <summary>Core metadata contains neither runtime-emission types nor a Dynamic assembly edge.</summary>
    [TestMethod]
    public void CoreAssembly_HasNoRuntimeEmissionOrDynamicReferences()
    {
        using var assembly =
            File.OpenRead(typeof(Mock).Assembly.Location);
        using var peReader =
            new System.Reflection.PortableExecutable.PEReader(assembly);
        System.Reflection.Metadata.MetadataReader metadata =
            peReader.GetMetadataReader();

        string[] assemblyReferences =
        [
            .. metadata.AssemblyReferences
                .Select(handle => metadata.GetAssemblyReference(handle))
                .Select(reference => metadata.GetString(reference.Name))
        ];
        CollectionAssert.DoesNotContain(
            assemblyReferences,
            "AlvorKit.Mocking.Dynamic");
        CollectionAssert.DoesNotContain(
            assemblyReferences,
            "AlvorKit.Mocking.Emit");
        CollectionAssert.DoesNotContain(
            assemblyReferences,
            "AlvorKit.Mocking.Interception");

        string[] forbiddenTypeReferences =
        [
            .. metadata.TypeReferences
                .Select(handle => metadata.GetTypeReference(handle))
                .Select(reference =>
                    (
                        Namespace: metadata.GetString(reference.Namespace),
                        Name: metadata.GetString(reference.Name)))
                .Where(static reference =>
                    reference.Namespace == "System.Reflection.Emit" ||
                    reference.Name is
                        "DynamicMethod" or
                        "AssemblyBuilder" or
                        "ModuleBuilder" or
                        "TypeBuilder" or
                        "MethodBuilder" or
                        "ILGenerator")
                .Select(static reference =>
                    $"{reference.Namespace}.{reference.Name}")
        ];

        Assert.HasCount(
            0,
            forbiddenTypeReferences,
            $"Core runtime-emission references: " +
            $"{string.Join(", ", forbiddenTypeReferences)}.");
    }

    /// <summary>Dynamic and shared emit stay independent of operation interception and profiler assets.</summary>
    [TestMethod]
    public void DynamicAndSharedEmit_AreProfilerFreeAndOperationIndependent()
    {
        string[] dynamicReferences =
            ReadAssemblyReferences(typeof(MockDynamic).Assembly);
        CollectionAssert.Contains(
            dynamicReferences,
            "AlvorKit.Mocking.Emit");
        AssertNoInterceptionOrProfilerReferences(
            dynamicReferences,
            "Dynamic");

        string[] emitReferences =
            ReadAssemblyReferences(typeof(MockTypedTrampolineIl).Assembly);
        CollectionAssert.Contains(
            emitReferences,
            "AlvorKit.Mocking");
        CollectionAssert.DoesNotContain(
            emitReferences,
            "AlvorKit.Mocking.Dynamic");
        AssertNoInterceptionOrProfilerReferences(
            emitReferences,
            "shared emit");
    }

    /// <summary>Operation runtime lives in the Interception adapter without a Dynamic or CoreCLR edge.</summary>
    [TestMethod]
    public void InterceptionAdapter_OwnsOperationRuntimeWithoutDynamicOrCoreClr()
    {
        Assembly adapter = typeof(MockInterception).Assembly;

        Assert.AreSame(
            adapter,
            typeof(MockInterceptionBindingState).Assembly);
        Assert.AreSame(
            adapter,
            typeof(MockInterceptionWrapperCache).Assembly);
        Assert.AreSame(
            adapter,
            typeof(MockInterceptionDelegateContract).Assembly);
        Assert.AreSame(
            adapter,
            typeof(MockReceiverFreeMethodCache).Assembly);
        Assert.AreSame(
            adapter,
            typeof(MockTypedTrampolineCache).Assembly);
        Assert.IsFalse(
            typeof(IMockOperationBackend).IsAssignableFrom(
                typeof(DynamicMockRuntimeBackend)));
        Assert.IsTrue(
            typeof(IMockOperationBackend).IsAssignableFrom(
                typeof(MockInterceptionOperationBackend)));

        string[] references = ReadAssemblyReferences(adapter);
        CollectionAssert.Contains(
            references,
            "AlvorKit.Mocking");
        CollectionAssert.Contains(
            references,
            "AlvorKit.Mocking.Emit");
        CollectionAssert.DoesNotContain(
            references,
            "AlvorKit.Mocking.Dynamic");
        CollectionAssert.DoesNotContain(
            references,
            "AlvorKit.Interception.CoreClr");
        Assert.IsFalse(
            references.Any(static reference =>
                reference.StartsWith(
                    "AlvorKit.Interception.Profiler",
                    StringComparison.Ordinal)),
            $"Interception adapter references: {string.Join(", ", references)}.");
    }

    private static string[] ReadAssemblyReferences(Assembly assembly)
    {
        using var stream = File.OpenRead(assembly.Location);
        using var peReader =
            new System.Reflection.PortableExecutable.PEReader(stream);
        System.Reflection.Metadata.MetadataReader metadata =
            peReader.GetMetadataReader();
        return
        [
            .. metadata.AssemblyReferences
                .Select(handle => metadata.GetAssemblyReference(handle))
                .Select(reference => metadata.GetString(reference.Name))
        ];
    }

    private static void AssertNoInterceptionOrProfilerReferences(
        string[] references,
        string boundary)
    {
        string[] forbidden =
        [
            .. references.Where(static reference =>
                reference == "AlvorKit.Mocking.Interception" ||
                reference == "AlvorKit.Interception" ||
                reference == "AlvorKit.Interception.CoreClr" ||
                reference.StartsWith(
                    "AlvorKit.Interception.Profiler",
                    StringComparison.Ordinal))
        ];
        Assert.HasCount(
            0,
            forbidden,
            $"{boundary} forbidden references: {string.Join(", ", forbidden)}.");
    }
}
