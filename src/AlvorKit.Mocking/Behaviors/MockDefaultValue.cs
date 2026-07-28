namespace AlvorKit.Mocking;

/// <summary>Creates stable values for explicit loose-mock fallback.</summary>
internal static class MockDefaultValue
{
    private static readonly ConditionalWeakTable<Type, MockTaskDefault>
        TaskDefaults = [];
    private static readonly Lock TaskDefaultGate = new();

    /// <summary>Creates a loose default for one declared return type.</summary>
    internal static object? Create(Type returnType)
    {
        if (returnType == typeof(void) ||
            returnType.IsFunctionPointer ||
            returnType.IsPointer)
        {
            return null;
        }

        if (typeof(Delegate).IsAssignableFrom(returnType) ||
            returnType.IsByRefLike)
        {
            return null;
        }

        if (returnType.IsByRef)
            return Create(returnType.GetElementType()!);

        if (returnType == typeof(string))
            return string.Empty;

        if (returnType == typeof(Task))
            return Task.CompletedTask;

        if (returnType.IsGenericType &&
            returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                throw new MockException(
                    $"Loose default creation for '{returnType}' requires " +
                    "runtime code generation.");
            }

            MockTaskDefault adapter;
            lock (TaskDefaultGate)
            {
                if (!TaskDefaults.TryGetValue(
                        returnType,
                        out adapter!))
                {
                    adapter = CreateDynamicTaskDefault(
                        returnType);
                    TaskDefaults.Add(
                        returnType,
                        adapter);
                }
            }
            return adapter.Create();
        }

        if (typeof(Task).IsAssignableFrom(returnType))
            return null;

        if (returnType.IsValueType)
            return Array.CreateInstance(
                    returnType,
                    1)
                .GetValue(0);

        if (returnType.IsArray)
            return Array.CreateInstance(returnType.GetElementType()!, 0);

        if (typeof(ICollection).IsAssignableFrom(returnType))
        {
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                throw new MockException(
                    $"Loose collection default creation for '{returnType}' " +
                    "requires runtime code generation.");
            }
            if (!returnType.IsAbstract &&
                returnType.GetConstructor(Type.EmptyTypes) is not null)
            {
                return Activator.CreateInstance(returnType);
            }
        }

        if (returnType.IsClass || returnType.IsInterface)
        {
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                throw new MockException(
                    $"Loose nested-mock default creation for '{returnType}' " +
                    "requires runtime code generation.");
            }
            return Mock.Create(returnType, MockBehavior.Loose);
        }

        return null;
    }

    [UnconditionalSuppressMessage(
        "Aot",
        "IL3050",
        Justification =
            "The caller checks dynamic-code support before entering this " +
            "closed adapter factory.")]
    private static MockTaskDefault CreateDynamicTaskDefault(
        Type returnType)
    {
        Type adapterType = typeof(MockTaskDefault<>)
            .MakeGenericType(
                returnType.GetGenericArguments()[0]);
        return (MockTaskDefault)Activator.CreateInstance(
            adapterType,
            nonPublic: true)!;
    }
}

/// <summary>Creates the completed loose default for one closed generic task type.</summary>
internal abstract class MockTaskDefault
{
    /// <summary>Creates a completed task containing the element default.</summary>
    internal abstract object Create();
}

/// <summary>Creates completed loose defaults for <see cref="Task{TResult}"/>.</summary>
/// <typeparam name="T">The task result type.</typeparam>
internal sealed class MockTaskDefault<T> : MockTaskDefault
{
    /// <inheritdoc />
    internal override object Create() =>
        Task.FromResult(default(T));
}
