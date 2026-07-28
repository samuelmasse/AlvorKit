namespace AlvorKit.Mocking;

/// <summary>Validates and collects typed snapshot projectors for one setup clause.</summary>
internal sealed class MockSnapshotProjectorBuilder(MethodInfo method)
{
    private static readonly MockSnapshotProjector[] Empty = [];
    private List<MockSnapshotProjector>? projectors;

    /// <summary>Registers one exact typed projector for a declared phase.</summary>
    internal void Add<T, TResult>(
        int declaredIndex,
        MockSnapshotPhase phase,
        SnapshotProjector<T, TResult> projector)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(declaredIndex);
        ArgumentNullException.ThrowIfNull(projector);

        ParameterInfo[] parameters = method.GetParameters();
        if ((uint)declaredIndex >= (uint)parameters.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(declaredIndex),
                declaredIndex,
                $"Method '{method.Name}' has {parameters.Length} parameters.");
        }

        ParameterInfo parameter = parameters[declaredIndex];
        Type declaredType = parameter.ParameterType;
        Type valueType = declaredType.IsByRef
            ? declaredType.GetElementType()!
            : declaredType;
        if (valueType != typeof(T))
        {
            throw new ArgumentException(
                $"Declared parameter {declaredIndex} on '{method.Name}' has " +
                $"value type '{valueType}', not '{typeof(T)}'.",
                nameof(projector));
        }

        ValidatePhase(declaredIndex, phase, parameter);
        ValidateResultType<TResult>();

        projectors ??= [];
        if (projectors.Any(
            candidate =>
                candidate.DeclaredIndex == declaredIndex &&
                candidate.Phase == phase))
        {
            throw new MockException(
                $"Declared parameter {declaredIndex} on '{method.Name}' " +
                $"already has a {phase.ToString().ToLowerInvariant()} projector.");
        }

        projectors.Add(
            new MockSnapshotProjector<T, TResult>(
                declaredIndex,
                declaredType,
                phase,
                projector));
    }

    /// <summary>Returns one immutable projector generation for setup publication.</summary>
    internal MockSnapshotProjector[] Snapshot() =>
        projectors is null
            ? Empty
            : [.. projectors];

    private static void ValidatePhase(
        int declaredIndex,
        MockSnapshotPhase phase,
        ParameterInfo parameter)
    {
        if (phase == MockSnapshotPhase.Entry)
        {
            if (parameter.IsOut)
            {
                throw new MockException(
                    $"Output parameter {declaredIndex} has no entry value to project.");
            }

            return;
        }

        bool mutableReference =
            parameter.ParameterType.IsByRef &&
            !parameter.IsIn;
        if (!mutableReference)
        {
            throw new MockException(
                $"Exit projection requires mutable ref or out parameter " +
                $"{declaredIndex}.");
        }
    }

    private static void ValidateResultType<TResult>()
    {
        Type resultType = typeof(TResult);
        if (resultType.IsByRefLike ||
            resultType.IsByRef ||
            resultType.IsPointer ||
            resultType.IsFunctionPointer ||
            resultType.ContainsGenericParameters)
        {
            throw new MockException(
                $"Snapshot projector result type '{resultType}' is not heap-safe.");
        }
    }
}
