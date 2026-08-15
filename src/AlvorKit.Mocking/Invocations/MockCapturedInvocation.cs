namespace AlvorKit;

/// <summary>
/// Stores one captured call in both dispatch-carrier order and canonical
/// declared-parameter order.
/// </summary>
internal sealed class MockCapturedInvocation
{
    private readonly object?[] carrierArguments;
    private readonly MockArgumentPattern[] declaredPatterns;

    /// <summary>Creates an immutable captured-call model.</summary>
    internal MockCapturedInvocation(
        object instance,
        Mocked mocked,
        MethodInfo method,
        ReadOnlySpan<object?> carrierArguments)
    {
        Instance = instance;
        Mocked = mocked;
        Method = method;
        Operation = mocked.ReceiverFree?.Operation ?? method;
        this.carrierArguments = carrierArguments.ToArray();

        var parameters = method.GetParameters();
        var carrierIndices = Indices.ParameterIndices(mocked.Type, method);
        declaredPatterns = new MockArgumentPattern[parameters.Length];

        for (var i = 0; i < declaredPatterns.Length; i++)
        {
            var carrierIndex = carrierIndices[i];
            declaredPatterns[i] = new(
                carrierIndex < this.carrierArguments.Length
                    ? this.carrierArguments[carrierIndex]
                    : null);
        }
    }

    /// <summary>Gets the captured mocked receiver.</summary>
    internal object Instance { get; }

    /// <summary>Gets the receiver's attached mock state.</summary>
    internal Mocked Mocked { get; }

    /// <summary>Gets the captured method or accessor.</summary>
    internal MethodInfo Method { get; }

    /// <summary>Gets the intercepted member represented by history.</summary>
    internal MemberInfo Operation { get; }

    /// <summary>Gets arguments in the current setup carrier order.</summary>
    internal object?[] CarrierArguments => carrierArguments;

    /// <summary>Gets captured argument patterns in declared parameter order.</summary>
    internal ReadOnlySpan<MockArgumentPattern> DeclaredPatterns =>
        declaredPatterns;

    /// <summary>Returns whether one history record matches this captured call.</summary>
    internal bool Matches(MockInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);
        if (invocation.Identity.Target.OwnerId != Mocked.TargetOwnerId ||
            invocation.Identity.Operation != Operation)
            return false;

        var arguments = invocation.Arguments;
        if (arguments.Length != declaredPatterns.Length)
            return false;

        for (var i = 0; i < arguments.Length; i++)
        {
            var declaredType = arguments[i].DeclaredType;
            var valueType = declaredType.IsByRef
                ? declaredType.GetElementType()!
                : declaredType;
            var pattern = declaredPatterns[i];

            if (arguments[i].Entry.Unavailable?.Reason ==
                MockUnavailableReason.OutHasNoEntryValue)
            {
                continue;
            }

            if (valueType.IsByRefLike && pattern.Value is not Matcher)
                continue;

            var actual = arguments[i].Entry.Kind ==
                MockInvocationArgumentSnapshotKind.Unavailable
                    ? null
                    : arguments[i].Entry.Value;
            if (arguments[i].Entry.Kind ==
                    MockInvocationArgumentSnapshotKind.Projected &&
                pattern.Value is Matcher
                {
                    RequiresTypedEvaluation: true
                } projectedMatcher)
            {
                if (!projectedMatcher.MatchesProjected(actual))
                    return false;

                continue;
            }

            if (!pattern.Matches(actual))
                return false;
        }

        return true;
    }

    /// <summary>Returns whether one history record is for the captured member.</summary>
    internal bool IsSameOperation(MockInvocation invocation) =>
        invocation.Identity.Target.OwnerId == Mocked.TargetOwnerId &&
        invocation.Identity.Operation == Operation;

    /// <summary>Formats the captured member without invoking user code.</summary>
    internal string DescribeOperation()
    {
        var typeName = Operation.DeclaringType?.FullName ?? "<unknown type>";
        return $"{typeName}.{Operation.Name}";
    }
}
