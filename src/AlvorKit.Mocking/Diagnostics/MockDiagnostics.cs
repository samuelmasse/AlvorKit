namespace AlvorKit;

/// <summary>Creates bounded deterministic failures without invoking arbitrary user code.</summary>
internal static class MockDiagnostics
{
    /// <summary>Formats an unexpected strict invocation.</summary>
    internal static string UnexpectedInvocation(
        Mocked mocked,
        MethodInfo method,
        ReadOnlySpan<object?> arguments)
    {
        var message = new StringBuilder()
            .Append("Unexpected invocation of '");
        MockDiagnosticSignatureFormatter.AppendSignature(
            message,
            mocked.Type.Type,
            method);
        message
            .Append("' on strict mock #")
            .Append(mocked.Invocations.Id)
            .Append(" using the dynamic instance backend");
        MockDiagnosticMessageFormatter.AppendCurrentSession(
            message);
        message.Append('.');

        if (arguments.Length != 0)
        {
            message.Append(" Received: ");
            var parameters = method.GetParameters();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (i > 0)
                    message.Append(", ");

                var declaredType = i < parameters.Length
                    ? parameters[i].ParameterType
                    : null;
                MockDiagnosticValueFormatter.AppendValue(
                    message,
                    arguments[i],
                    declaredType);
            }
        }

        return MockDiagnosticMessageFormatter.Bound(message);
    }

    /// <summary>Formats a method using its declared type and exact parameter shapes.</summary>
    internal static string Operation(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        var message = new StringBuilder();
        MockDiagnosticSignatureFormatter.AppendSignature(
            message,
            method.DeclaringType ?? typeof(object),
            method);
        return MockDiagnosticMessageFormatter.Bound(message);
    }

    /// <summary>Formats a retained invocation argument without re-running user code.</summary>
    internal static string ArgumentSnapshot(
        MockInvocationArgumentSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var message = new StringBuilder();
        MockDiagnosticFormatter.AppendSnapshot(message, snapshot);
        return MockDiagnosticMessageFormatter.Bound(message);
    }

    /// <summary>Formats a failed matcher-based count constraint and same-target candidates.</summary>
    internal static string CountFailure(
        MockCapturedInvocation captured,
        MockVerificationCountKind kind,
        int expected,
        int observed,
        ReadOnlySpan<MockInvocation> candidates)
    {
        var sortedCandidates = candidates.ToArray();
        Array.Sort(
            sortedCandidates,
            MockInvocationSequenceComparer.Instance);
        var message = new StringBuilder()
            .Append("Expected '");
        MockDiagnosticFormatter.AppendCaptured(message, captured);
        message
            .Append("' ")
            .Append(
                MockDiagnosticMessageFormatter.DescribeConstraint(
                    kind,
                    expected))
            .Append(", but observed ")
            .Append(observed)
            .Append('.');
        MockDiagnosticMessageFormatter.AppendCandidates(
            message,
            " Same-target candidates:",
            sortedCandidates);
        return MockDiagnosticMessageFormatter.Bound(message);
    }

    /// <summary>Formats all remaining unverified invocations for one mock.</summary>
    internal static string? NoOtherCalls(
        Mocked mocked,
        ReadOnlySpan<MockInvocation> invocations)
    {
        var remaining = new MockInvocation[invocations.Length];
        var remainingCount = 0;
        for (var i = 0; i < invocations.Length; i++)
        {
            if (!invocations[i].IsVerified)
                remaining[remainingCount++] = invocations[i];
        }

        if (remainingCount == 0)
            return null;

        Array.Sort(
            remaining,
            0,
            remainingCount,
            MockInvocationSequenceComparer.Instance);
        var message = new StringBuilder()
            .Append("Expected no other calls for mock type '")
            .Append(mocked.Type.Type.FullName)
            .Append("'. Remaining invocations:");
        MockDiagnosticMessageFormatter.AppendCandidates(
            message,
            string.Empty,
            remaining.AsSpan(0, remainingCount));
        return MockDiagnosticMessageFormatter.Bound(message);
    }

    /// <summary>Formats the first expected-versus-actual logical sequence divergence.</summary>
    internal static string SequenceFailure(
        int index,
        MockCapturedInvocation? expected,
        MockInvocation? actual)
    {
        var message = new StringBuilder()
            .Append("Mock sequence diverged at position ")
            .Append(index)
            .Append(": expected '");
        if (expected is null)
            message.Append("<end of expected sequence>");
        else
            MockDiagnosticFormatter.AppendCaptured(message, expected);

        message.Append("', actual '");
        if (actual is null)
            message.Append("<end of actual sequence>");
        else
            MockDiagnosticFormatter.AppendInvocation(message, actual);

        return MockDiagnosticMessageFormatter.Bound(
            message.Append("'."));
    }

    /// <summary>Formats one deterministic backend-specific signature rejection.</summary>
    internal static string SignatureRejection(
        MockBackendIdentity backend,
        MockOperationKind operation,
        MockCanonicalSignature signature,
        MockUnsupportedSignatureReason reason,
        string detail)
    {
        var message = new StringBuilder()
            .Append(backend.Kind)
            .Append(" ABI ")
            .Append(backend.AbiVersion)
            .Append(" does not support ")
            .Append(operation)
            .Append(" signature '");
        MockDiagnosticSignatureFormatter.AppendCanonicalSignature(
            message,
            signature);
        message
            .Append("': ")
            .Append(detail)
            .Append(" [")
            .Append(reason)
            .Append(']');
        return MockDiagnosticMessageFormatter.Bound(message);
    }

    /// <summary>Formats a public lifecycle operation against a non-mock target.</summary>
    internal static string NonMockTarget(
        string operation,
        object target)
    {
        var message = new StringBuilder()
            .Append("Cannot ")
            .Append(operation)
            .Append(" for non-mock type '");
        MockDiagnosticValueFormatter.AppendType(
            message,
            target.GetType());
        message.Append("'.");
        return MockDiagnosticMessageFormatter.Bound(message);
    }

    /// <summary>Formats a session operation that requires its session to be current.</summary>
    internal static string SessionMustBeCurrent(string operation) =>
        $"{operation} requires this mock session to be current.";

    /// <summary>Gets the out-of-order session-disposal failure.</summary>
    internal static string SessionDisposalOrder() =>
        "Mock sessions must be disposed in reverse creation order on the active execution context.";

    /// <summary>Gets the foreign-checkpoint lifecycle failure.</summary>
    internal static string ForeignCheckpoint() =>
        "The checkpoint belongs to a different mock session.";

    /// <summary>Gets the future-checkpoint lifecycle failure.</summary>
    internal static string FutureCheckpoint() =>
        "The checkpoint is ahead of the session timeline.";

    /// <summary>Gets the reversed-window lifecycle failure.</summary>
    internal static string ReversedCheckpointWindow() =>
        "The beginning checkpoint must not follow the ending checkpoint.";

    /// <summary>Appends a bounded deterministic sequence list for failure diagnostics.</summary>
    internal static void AppendSequences(
        StringBuilder message,
        ReadOnlySpan<long> sequences) =>
        MockDiagnosticMessageFormatter.AppendSequences(
            message,
            sequences);
}
