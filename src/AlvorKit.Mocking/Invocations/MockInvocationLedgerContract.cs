namespace AlvorKit;

/// <summary>Validates invocation-ledger ownership and entry snapshot invariants.</summary>
internal static class MockInvocationLedgerContract
{
    /// <summary>Validates that a token belongs to the expected ledger.</summary>
    internal static void ValidateToken(
        long ledgerId,
        MockInvocationToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (token.LedgerId != ledgerId)
        {
            throw new ArgumentException(
                "The invocation token belongs to another ledger.",
                nameof(token));
        }
    }

    /// <summary>Validates that a snapshot belongs to the expected ledger.</summary>
    internal static void ValidateSnapshot(
        long ledgerId,
        MockInvocationLedgerSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.LedgerId != ledgerId)
        {
            throw new ArgumentException(
                "The snapshot belongs to another ledger.",
                nameof(snapshot));
        }
    }

    /// <summary>Validates declared parameter ordering and unavailable-value metadata.</summary>
    internal static void ValidateEntryArguments(
        MockInvocationIdentity identity,
        MockInvocationArgumentSnapshot[] entries,
        ParameterInfo[]? reflectedParameters)
    {
        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i] ??
                throw new ArgumentException(
                    "Entry argument snapshots cannot contain null.",
                    nameof(entries));
            if (entry.DeclaredIndex != i ||
                entry.Phase != MockSnapshotPhase.Entry)
            {
                throw new ArgumentException(
                    "Entry argument snapshots must be in declared parameter order.",
                    nameof(entries));
            }
        }

        if (identity.Operation is not MethodBase method)
            return;

        ParameterInfo[] parameters =
            reflectedParameters ?? method.GetParameters();
        if (parameters.Length != entries.Length)
        {
            throw new ArgumentException(
                "Entry argument count does not match the intercepted method.",
                nameof(entries));
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType != entries[i].DeclaredType)
            {
                throw new ArgumentException(
                    $"Entry argument {i} does not preserve its declared parameter type.",
                    nameof(entries));
            }

            if (parameters[i].IsOut)
            {
                if (entries[i].Unavailable?.Reason !=
                    MockUnavailableReason.OutHasNoEntryValue)
                {
                    throw new ArgumentException(
                        $"Output argument {i} must be normalized without reading its entry storage.",
                        nameof(entries));
                }

                continue;
            }

            Type parameterType = parameters[i].ParameterType;
            Type valueType = parameterType.IsByRef
                ? parameterType.GetElementType()!
                : parameterType;
            if (valueType.IsByRefLike &&
                entries[i].Unavailable?.Reason !=
                MockUnavailableReason.ByRefLikeProjectionNotConfigured)
            {
                throw new ArgumentException(
                    $"Byref-like argument {i} must enter history as unavailable metadata.",
                    nameof(entries));
            }
        }
    }
}
