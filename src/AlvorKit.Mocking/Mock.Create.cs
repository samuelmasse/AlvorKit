namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>Creates a full mock with the requested fallback behavior.</summary>
    public static object Create(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicEvents)]
        Type type,
        MockBehavior behavior = MockBehavior.Strict)
    {
        ValidateMockableType(type);
        var fallback = behavior switch
        {
            MockBehavior.Strict => MockFallbackBehavior.Strict,
            MockBehavior.Loose => MockFallbackBehavior.Loose,
            _ => throw new ArgumentOutOfRangeException(nameof(behavior), behavior, "Unknown mock behavior.")
        };

        Type runtimeType =
            MockRuntimeBackendRegistry.Proxy.ResolveMockType(type);
        object mock = RuntimeHelpers.GetUninitializedObject(runtimeType);
        if (mock is IMock proxy)
        {
            proxy.__Mocked_cc6d2cf7 =
                new(fallback, Types.Get(type));
        }
        else
        {
            Sealed.Add(mock, new(fallback, Types.Get(type)));
        }

        return mock;
    }

    /// <summary>Creates a strict full mock for the requested class or interface type.</summary>
    public static T Create<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicEvents)] T>()
        where T : class =>
        (T)Create(typeof(T));

    /// <summary>Creates an explicit loose full mock for the requested type.</summary>
    public static T CreateLoose<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicEvents)] T>()
        where T : class =>
        (T)Create(typeof(T), MockBehavior.Loose);

    /// <summary>
    /// Partially mocks an existing instance while unmatched calls continue to
    /// the original implementation.
    /// </summary>
    private static object PartialCore(object instance)
    {
        if (instance is IMock)
        {
            throw new MockException(
                $"Cannot partially mock '{instance.GetType().FullName}' because it is already a full mock.");
        }
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new MockException(
                "Partial mocks require runtime code generation.");
        }

        var type = instance.GetType();
        ValidateMockableType(type);

        lock (instance)
        {
            if (Sealed.TryGetValue(instance, out _))
            {
                throw new MockException(
                    $"Cannot partially mock '{type.FullName}' because the instance is already mocked.");
            }

            Sealed.Add(instance, new(MockFallbackBehavior.Partial, Types.Get(type)));
        }

        return instance;
    }

    /// <summary>Partially mocks an existing object instance while preserving its concrete type.</summary>
    public static T Partial<T>(T instance) where T : class =>
        (T)PartialCore(instance);

    /// <summary>Throws when a type cannot carry mock state in this dispatch model.</summary>
    private static void ValidateMockableType(Type type)
    {
        if (!type.IsClass && !type.IsInterface)
            throw new MockException($"Cannot mock type '{type.FullName}'. Only classes and interfaces are supported.");

        if (type.IsArray || type.IsPointer || type.IsEnum || type.IsValueType || typeof(Delegate).IsAssignableFrom(type))
            throw new MockException($"Cannot mock unsupported type '{type.FullName}'.");
    }
}
