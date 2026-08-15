namespace AlvorKit;

internal delegate void ExactTransformCallback(
    scoped in ReadOnlySpan<int> source,
    scoped ref Span<int> destination,
    scoped out BorrowedWindow written);

internal delegate int ExactTransformAnswer(
    int offset,
    scoped in ReadOnlySpan<int> source,
    scoped ref Span<int> destination,
    scoped out BorrowedWindow written);

internal delegate void WideDirectCallback(
    int v0, int v1, int v2, int v3, int v4, int v5,
    int v6, int v7, int v8, int v9, int v10, int v11,
    int v12, int v13, int v14, int v15, int v16);

internal enum RefSafeCallbackKind
{
    Action,
    Func,
    NaturalDelegate
}

internal static class RefSafeCallbackContract
{
    internal static Delegate Normalize(
        Delegate callback,
        MethodInfo capturedMethod)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(capturedMethod);

        Validate(callback, capturedMethod);
        Type stableDelegateType =
            RefSafeStableDelegateCache.GetOrCreate(
                capturedMethod);
        ValidateInvoke(
            stableDelegateType.GetMethod(nameof(Action.Invoke))!,
            capturedMethod);

        try
        {
            return Delegate.CreateDelegate(
                stableDelegateType,
                callback.Target,
                callback.Method,
                true)!;
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException(
                "The callback could not be normalized to the stable exact delegate shape.",
                nameof(callback),
                exception);
        }
    }

    internal static void Validate(
        Delegate callback,
        MethodInfo capturedMethod)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(capturedMethod);
        if (capturedMethod.ContainsGenericParameters)
        {
            throw new ArgumentException(
                "The captured callback signature must be closed.",
                nameof(capturedMethod));
        }

        ValidateSourceInvoke(
            callback.GetType().GetMethod(nameof(Action.Invoke))!,
            capturedMethod);
        ValidateAsyncVoid(callback);
    }

    private static void ValidateSourceInvoke(
        MethodInfo invoke,
        MethodInfo capturedMethod)
    {
        if (invoke.ReturnType != capturedMethod.ReturnType)
            throw Mismatch("return type");

        ValidateParameter(
            invoke.ReturnParameter,
            capturedMethod.ReturnParameter,
            "return");

        ParameterInfo[] actual = invoke.GetParameters();
        ParameterInfo[] expected = capturedMethod.GetParameters();
        if (actual.Length != expected.Length)
            throw Mismatch("parameter count");

        for (int index = 0; index < actual.Length; index++)
        {
            ValidateSourceParameter(
                actual[index],
                expected[index],
                $"parameter {index}");
        }
    }

    internal static void ValidateInvoke(
        MethodInfo invoke,
        MethodInfo capturedMethod)
    {
        ArgumentNullException.ThrowIfNull(invoke);
        ArgumentNullException.ThrowIfNull(capturedMethod);
        if (capturedMethod.ContainsGenericParameters)
        {
            throw new ArgumentException(
                "The captured callback signature must be closed.",
                nameof(capturedMethod));
        }

        if (invoke.ReturnType != capturedMethod.ReturnType)
            throw Mismatch("return type");

        ValidateParameter(
            invoke.ReturnParameter,
            capturedMethod.ReturnParameter,
            "return");

        ParameterInfo[] actual = invoke.GetParameters();
        ParameterInfo[] expected = capturedMethod.GetParameters();
        if (actual.Length != expected.Length)
            throw Mismatch("parameter count");

        for (int index = 0; index < actual.Length; index++)
        {
            ValidateParameter(
                actual[index],
                expected[index],
                $"parameter {index}");
        }
    }

    internal static void ValidateReturn(
        Delegate callback,
        Type expectedReturnType)
    {
        ArgumentNullException.ThrowIfNull(callback);
        MethodInfo invoke = callback.GetType().GetMethod(
            nameof(Action.Invoke))!;
        if (invoke.ReturnType != expectedReturnType)
            throw Mismatch("return type");

        ValidateAsyncVoid(callback);
    }

    private static void ValidateParameter(
        ParameterInfo actual,
        ParameterInfo expected,
        string location)
    {
        if (actual.ParameterType != expected.ParameterType)
            throw Mismatch($"{location} type");
        if (actual.IsIn != expected.IsIn)
            throw Mismatch($"{location} IsIn metadata");
        if (actual.IsOut != expected.IsOut)
            throw Mismatch($"{location} IsOut metadata");
        if (!actual.GetRequiredCustomModifiers().SequenceEqual(
                expected.GetRequiredCustomModifiers()))
        {
            throw Mismatch(
                $"{location} required custom modifiers " +
                $"({FormatModifiers(actual.GetRequiredCustomModifiers())} != " +
                $"{FormatModifiers(expected.GetRequiredCustomModifiers())})");
        }

        if (!actual.GetOptionalCustomModifiers().SequenceEqual(
                expected.GetOptionalCustomModifiers()))
        {
            throw Mismatch($"{location} optional custom modifiers");
        }

        if (HasScopedRef(actual) != HasScopedRef(expected))
            throw Mismatch($"{location} scoped metadata");
    }

    private static void ValidateSourceParameter(
        ParameterInfo actual,
        ParameterInfo expected,
        string location)
    {
        if (actual.ParameterType != expected.ParameterType)
            throw Mismatch($"{location} type");
        if (actual.IsIn != expected.IsIn)
            throw Mismatch($"{location} IsIn metadata");
        if (actual.IsOut != expected.IsOut)
            throw Mismatch($"{location} IsOut metadata");
        if (!SourceRequiredModifiersMatch(actual, expected))
            throw Mismatch($"{location} required custom modifiers");
        if (!actual.GetOptionalCustomModifiers().SequenceEqual(
                expected.GetOptionalCustomModifiers()))
        {
            throw Mismatch($"{location} optional custom modifiers");
        }

        if (HasScopedRef(actual) != HasScopedRef(expected))
            throw Mismatch($"{location} scoped metadata");
    }

    private static bool SourceRequiredModifiersMatch(
        ParameterInfo actual,
        ParameterInfo expected)
    {
        Type[] actualModifiers =
            actual.GetRequiredCustomModifiers();
        Type[] expectedModifiers =
            expected.GetRequiredCustomModifiers();
        if (actualModifiers.SequenceEqual(expectedModifiers))
            return true;

        // C# natural delegates preserve `in` and `scoped` metadata but omit the
        // method signature's InAttribute modreq. The normalized stable delegate
        // is still validated strictly before it is stored.
        return actual.IsIn &&
            actualModifiers.SequenceEqual(
                expectedModifiers.Where(
                    static modifier =>
                        modifier !=
                        typeof(System.Runtime.InteropServices.InAttribute)));
    }

    private static bool HasScopedRef(ParameterInfo parameter) =>
        parameter.GetCustomAttributesData().Any(
            static attribute =>
                attribute.AttributeType.FullName ==
                "System.Runtime.CompilerServices.ScopedRefAttribute");

    private static string FormatModifiers(Type[] modifiers) =>
        string.Join(
            ",",
            modifiers.Select(static modifier => modifier.FullName));

    private static ArgumentException Mismatch(string facet) =>
        new(
            $"The callback Invoke {facet} does not match the closed captured signature.",
            "callback");

    private static void ValidateAsyncVoid(Delegate callback)
    {
        MethodInfo invoke = callback.GetType().GetMethod(
            nameof(Action.Invoke))!;
        if (invoke.ReturnType == typeof(void) &&
            callback.Method.GetCustomAttribute<
                System.Runtime.CompilerServices
                    .AsyncStateMachineAttribute>() is not null)
        {
            throw new ArgumentException(
                "Async-void callbacks are not supported.",
                nameof(callback));
        }
    }
}

internal interface IRefSafeContractTarget
{
    void Observe(ReadOnlySpan<int> values);

    void TransformByValue(Span<int> values);

    void Transform(ref Span<int> values);

    void TransformExact(
        scoped in ReadOnlySpan<int> source,
        scoped ref Span<int> destination,
        scoped out BorrowedWindow written);

    int TransformAnswer(
        int offset,
        scoped in ReadOnlySpan<int> source,
        scoped ref Span<int> destination,
        scoped out BorrowedWindow written);

    void Wide(
        int v0, int v1, int v2, int v3, int v4, int v5,
        int v6, int v7, int v8, int v9, int v10, int v11,
        int v12, int v13, int v14, int v15, int v16);

    T Echo<T>(T value);
}

internal readonly ref struct BorrowedWindow(
    ReadOnlySpan<int> values)
{
    internal ReadOnlySpan<int> Values { get; } = values;
}
