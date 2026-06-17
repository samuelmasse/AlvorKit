namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>Creates a mock object for a class or interface type.</summary>
    public static object Create(Type type)
    {
        ValidateMockableType(type);

        if (type.IsSealed)
        {
            Patcher.Patch(type);
            var mock = RuntimeHelpers.GetUninitializedObject(type);
            Sealed.Add(mock, new(false, Types.Get(type)));

            return mock;
        }
        else
        {
            var proxyType = Proxies.Get(type);
            Patcher.Patch(proxyType);

            var mock = (IMock)RuntimeHelpers.GetUninitializedObject(proxyType);
            mock.__Mocked_cc6d2cf7 = new(false, Types.Get(type));

            return mock;
        }
    }

    /// <summary>Creates a mock object for the requested class or interface type.</summary>
    public static T Create<T>() where T : class => (T)Create(typeof(T));

    /// <summary>Initializes mocking for one constructed generic method.</summary>
    public static void Generic(MethodInfo method)
    {
        if (!method.IsConstructedGenericMethod)
            throw new MockException("Can only intialize generic mocking on constructed generic methods.");

        Patcher.PatchMethod(method);
    }

    /// <summary>Initializes mocking for the constructed generic method referenced by a delegate.</summary>
    public static void Generic(Delegate del) => Generic(del.Method);

    /// <summary>Partially mocks an existing object instance while keeping unmatched calls on the original implementation.</summary>
    public static object Instance(object instance)
    {
        if (instance is IMock)
            throw new MockException($"Cannot mock instance {instance} because it is already created as a mock.");

        var type = instance.GetType();
        ValidateMockableType(type);

        Patcher.Patch(type);

        lock (instance)
        {
            if (Sealed.TryGetValue(instance, out _))
                throw new MockException($"Cannot mock instance {instance} because it is already mocked.");

            Sealed.Add(instance, new(true, Types.Get(type)));
        }

        return instance;
    }

    /// <summary>Partially mocks an existing object instance while preserving its concrete type.</summary>
    public static T Instance<T>(T instance) where T : class => (T)Instance((object)instance);

    /// <summary>Throws when a type cannot be mocked by this runtime patching model.</summary>
    private static void ValidateMockableType(Type type)
    {
        if (!type.IsClass && !type.IsInterface)
            throw new MockException($"Cannot mock type '{type.FullName}'. Only classes and interfaces are supported.");

        if (type.IsArray || type.IsPointer || type.IsEnum || type.IsValueType || typeof(Delegate).IsAssignableFrom(type))
            throw new MockException($"Cannot mock unsupported type '{type.FullName}'.");
    }
}
