namespace AlvorKit;

internal static partial class Capture
{
    private static readonly ThreadLocal<List<MockIndexedMatcher>>
        firstIndexedMatchers = new(() => []);
    private static readonly ThreadLocal<List<MockIndexedMatcher>>
        secondIndexedMatchers = new(() => []);

    /// <summary>Gets indexed matchers captured during the first pass.</summary>
    internal static List<MockIndexedMatcher> FirstIndexedMatchers =>
        firstIndexedMatchers.Value!;

    /// <summary>Gets indexed matchers captured during disambiguation.</summary>
    internal static List<MockIndexedMatcher> SecondIndexedMatchers =>
        secondIndexedMatchers.Value!;

    /// <summary>Binds a matcher directly to one declared parameter index.</summary>
    internal static void WriteIndexedMatcher<T>(
        int declaredIndex,
        MockIndexedMatcherPassingKind passingKind,
        Matcher matcher)
        where T : allows ref struct
    {
        if (!Context.IsActive)
        {
            throw new MockException(
                "Declared-index argument matchers require an active capture.");
        }

        List<MockIndexedMatcher> matchers = Context.IsDisambiguating
            ? SecondIndexedMatchers
            : FirstIndexedMatchers;
        if (matchers.Any(
            candidate => candidate.DeclaredIndex == declaredIndex))
        {
            throw new MockException(
                $"Declared argument {declaredIndex} already has a matcher.");
        }

        matchers.Add(
            new(
                declaredIndex,
                typeof(T),
                passingKind,
                matcher));
    }

    /// <summary>Gets whether the active capture pass owns one indexed matcher.</summary>
    internal static bool HasIndexedMatcher(int declaredIndex)
    {
        List<MockIndexedMatcher> matchers = Context.IsDisambiguating
            ? SecondIndexedMatchers
            : FirstIndexedMatchers;
        return matchers.Any(
            candidate => candidate.DeclaredIndex == declaredIndex);
    }

    private static void ClearFirstIndexedMatchers() =>
        FirstIndexedMatchers.Clear();

    private static void ClearSecondIndexedMatchers() =>
        SecondIndexedMatchers.Clear();

    private static void ValidateIndexedDisambiguation()
    {
        if (FirstIndexedMatchers.Count != SecondIndexedMatchers.Count)
        {
            throw new MockException(
                "Declared-index matcher capture changed between passes.");
        }

        for (var index = 0; index < FirstIndexedMatchers.Count; index++)
        {
            MockIndexedMatcher first = FirstIndexedMatchers[index];
            MockIndexedMatcher second = SecondIndexedMatchers[index];
            if (first.DeclaredIndex != second.DeclaredIndex ||
                first.ValueType != second.ValueType ||
                first.PassingKind != second.PassingKind ||
                first.Matcher.Type != second.Matcher.Type)
            {
                throw new MockException(
                    "Declared-index matcher capture changed between passes.");
            }
        }
    }

    private static void ProcessIndexedMatchers(
        MethodInfo method,
        object?[] arguments)
    {
        ParameterInfo[] parameters = method.GetParameters();
        int parameterOffset = CaptureParameterOffset();
        for (var declaredIndex = parameterOffset;
             declaredIndex < parameters.Length;
             declaredIndex++)
        {
            ParameterInfo parameter = parameters[declaredIndex];
            Type declaredType = parameter.ParameterType;
            Type valueType = declaredType.IsByRef
                ? declaredType.GetElementType()!
                : declaredType;
            if (valueType.IsByRefLike &&
                !parameter.IsOut &&
                !FirstIndexedMatchers.Any(
                    matcher =>
                        matcher.DeclaredIndex ==
                            declaredIndex - parameterOffset))
            {
                throw new MockException(
                    $"Ref-struct input parameter " +
                    $"{declaredIndex - parameterOffset} on " +
                    $"'{method.Name}' requires a declared-index matcher.");
            }
        }

        if (FirstIndexedMatchers.Count == 0)
            return;

        Mocked mocked = Mock.GetMocked(Context.Instance!)!;
        int[] carrierIndices = Indices.ParameterIndices(
            mocked.Type,
            method);

        foreach (MockIndexedMatcher indexed in FirstIndexedMatchers)
        {
            int actualIndex =
                indexed.DeclaredIndex + parameterOffset;
            if ((uint)actualIndex >= (uint)parameters.Length)
            {
                throw new MockException(
                    $"Declared matcher index {indexed.DeclaredIndex} is outside " +
                    $"method '{method.Name}' with " +
                    $"{parameters.Length - parameterOffset} parameters.");
            }

            ParameterInfo parameter = parameters[actualIndex];
            Type declaredType = parameter.ParameterType;
            Type valueType = declaredType.IsByRef
                ? declaredType.GetElementType()!
                : declaredType;
            if (valueType != indexed.ValueType)
            {
                throw new MockException(
                    $"Declared matcher index {indexed.DeclaredIndex} expects " +
                    $"'{valueType}' but received '{indexed.ValueType}'.");
            }

            ValidatePassingKind(indexed, parameter);
            arguments[carrierIndices[actualIndex]] =
                indexed.Matcher;
        }
    }

    private static void ValidatePassingKind(
        MockIndexedMatcher indexed,
        ParameterInfo parameter)
    {
        if (parameter.IsOut)
        {
            throw new MockException(
                $"Output parameter {indexed.DeclaredIndex} has no entry value to match.");
        }

        bool mutableReference =
            parameter.ParameterType.IsByRef &&
            !parameter.IsIn;
        if (indexed.PassingKind == MockIndexedMatcherPassingKind.Reference &&
            !mutableReference)
        {
            throw new MockException(
                $"Declared matcher index {indexed.DeclaredIndex} is not a mutable ref parameter.");
        }

        if (indexed.PassingKind == MockIndexedMatcherPassingKind.Value &&
            mutableReference)
        {
            throw new MockException(
                $"Declared matcher index {indexed.DeclaredIndex} requires Arg.AnyRef or a ref predicate.");
        }
    }
}
