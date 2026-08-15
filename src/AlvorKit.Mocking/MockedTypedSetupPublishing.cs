namespace AlvorKit;

/// <summary>Publishes typed value and managed-reference return factories.</summary>
internal static class MockedTypedSetupPublishing
{
    /// <summary>Adds an exact typed return factory without retaining its result.</summary>
    internal static void AddTypedReturnFactory<T>(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        Func<T> factory)
        where T : allows ref struct =>
        mocked.AddTypedReturnFactory(
            method,
            arguments,
            factory,
            []);

    /// <summary>Adds an exact typed return factory with history projectors.</summary>
    internal static void AddTypedReturnFactory<T>(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        Func<T> factory,
        ReadOnlySpan<MockSnapshotProjector> projectors)
        where T : allows ref struct
    {
        if (method.ContainsGenericParameters)
        {
            throw new MockException(
                $"Cannot configure a return factory for open method '{method.Name}'.");
        }

        if (method.ReturnType.IsByRef)
        {
            throw new MockException(
                $"ReturnFactory does not support managed-reference return " +
                $"'{method.DeclaringType?.FullName}.{method.Name}'; use " +
                "Mock.WhenRef or Mock.WhenRefReadonly.");
        }

        if (method.ReturnType != typeof(T))
        {
            throw new MockException(
                $"ReturnFactory type '{typeof(T).FullName}' does not match " +
                $"'{method.DeclaringType?.FullName}.{method.Name}' return type " +
                $"'{method.ReturnType.FullName}'.");
        }

        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockTypedReturnFactoryBehavior(factory),
            projectors);
    }

    /// <summary>Adds a mutable managed-reference factory after validation.</summary>
    internal static void AddRefReturnFactory<T>(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        MockRefCall<T> factory) =>
        mocked.AddRefReturnFactory(
            method,
            arguments,
            factory,
            []);

    /// <summary>Adds a mutable managed-reference factory with history projectors.</summary>
    internal static void AddRefReturnFactory<T>(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        MockRefCall<T> factory,
        ReadOnlySpan<MockSnapshotProjector> projectors)
    {
        ValidateRefReturn<T>(
            method,
            MockReturnKind.ManagedReference);
        var adapter = new MockRefReturnAdapter<T>(factory);
        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockTypedRefReturnFactoryBehavior(
                new MockManagedReferenceFactory<T>(adapter.Invoke)),
            projectors);
    }

    /// <summary>Adds a readonly managed-reference factory after validation.</summary>
    internal static void AddRefReadonlyReturnFactory<T>(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        MockRefReadonlyCall<T> factory) =>
        mocked.AddRefReadonlyReturnFactory(
            method,
            arguments,
            factory,
            []);

    /// <summary>Adds a readonly managed-reference factory with history projectors.</summary>
    internal static void AddRefReadonlyReturnFactory<T>(
        this Mocked mocked,
        MethodInfo method,
        object?[] arguments,
        MockRefReadonlyCall<T> factory,
        ReadOnlySpan<MockSnapshotProjector> projectors)
    {
        ValidateRefReturn<T>(
            method,
            MockReturnKind.ReadOnlyManagedReference);
        var adapter = new MockRefReadonlyReturnAdapter<T>(factory);
        MockedSetupPublication.Publish(
            mocked,
            method,
            arguments,
            new MockTypedRefReturnFactoryBehavior(
                new MockManagedReferenceFactory<T>(adapter.Invoke)),
            projectors);
    }

    /// <summary>Validates the exact element and mutability of a managed-reference return.</summary>
    private static void ValidateRefReturn<T>(
        MethodInfo method,
        MockReturnKind expectedKind)
    {
        if (method.ContainsGenericParameters)
        {
            throw new MockException(
                $"Cannot configure a managed-reference return for open method " +
                $"'{method.Name}'.");
        }

        if (!MockManagedReferenceAbi.IsSupported(method.ReturnType))
        {
            throw new MockException(
                $"Managed-reference return '{method.DeclaringType?.FullName}." +
                $"{method.Name}' cannot use stable generic storage because its " +
                "element type is open, pointer-shaped, or a ref struct.");
        }

        Type elementType = method.ReturnType.GetElementType()!;
        if (elementType != typeof(T))
        {
            throw new MockException(
                $"Managed-reference factory type '{typeof(T).FullName}' does not " +
                $"match '{method.DeclaringType?.FullName}.{method.Name}' element " +
                $"type '{elementType.FullName}'.");
        }

        MockReturnKind actualKind =
            MockCanonicalSignature.Create(method).Return.Kind;
        if (actualKind != expectedKind)
        {
            string expected = expectedKind ==
                MockReturnKind.ManagedReference
                ? "mutable"
                : "read-only";
            string actual = actualKind ==
                MockReturnKind.ManagedReference
                ? "mutable"
                : "read-only";
            throw new MockException(
                $"Cannot configure a {expected} reference factory for " +
                $"'{method.DeclaringType?.FullName}.{method.Name}' because its " +
                $"return is {actual}.");
        }
    }
}
