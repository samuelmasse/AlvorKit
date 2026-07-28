namespace AlvorKit.Mocking;

/// <summary>Creates per-mock stable loose and capture backing for managed references.</summary>
internal static class MockManagedReferenceDefault
{
    private static readonly MethodInfo CreateCoreMethod =
        typeof(MockManagedReferenceDefault).GetMethod(
            nameof(CreateCore),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>Creates one exact alias factory for a supported return type.</summary>
    internal static Delegate Create(Type returnType)
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new MockException(
                $"Managed-reference default creation for '{returnType}' " +
                "requires runtime code generation.");
        }

        Type elementType = returnType.GetElementType()!;
        return (Delegate)CreateCoreMethod
            .MakeGenericMethod(elementType)
            .Invoke(null, null)!;
    }

    private static Delegate CreateCore<T>()
    {
        var storage = new MockRefStorage<T>(default!);
        var adapter = new MockRefReturnAdapter<T>(storage.Mutable);
        return new MockManagedReferenceFactory<T>(adapter.Invoke);
    }
}
