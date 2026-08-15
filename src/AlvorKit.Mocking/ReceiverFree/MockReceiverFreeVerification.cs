namespace AlvorKit;

/// <summary>Verifies receiver-free history against member and site scopes.</summary>
internal static class MockReceiverFreeVerification
{
    /// <summary>Applies one count constraint and marks matches atomically.</summary>
    internal static void Verify(
        MockSession owner,
        MockReceiverFreeSetupDescriptor descriptor,
        MockVerificationCountKind kind,
        int expected,
        MockSession? windowSession,
        MockCheckpoint after,
        MockCheckpoint through)
    {
        if (windowSession is not null)
        {
            if (!ReferenceEquals(owner, windowSession))
            {
                throw new MockException(
                    "Receiver-free checkpoint verification belongs to " +
                    "another mock session.");
            }

            owner.ValidateWindow(after, through);
        }

        MockInvocationLedger ledger =
            owner.ReceiverFreeInvocations;
        MockInvocationLedgerSnapshot snapshot = ledger.Snapshot();
        ReadOnlySpan<MockInvocation> invocations =
            snapshot.Invocations;
        var matching = new int[invocations.Length];
        int matchingCount = 0;
        for (int index = 0; index < invocations.Length; index++)
        {
            MockInvocation invocation = invocations[index];
            if (IsInWindow(
                    invocation,
                    windowSession,
                    after,
                    through) &&
                Matches(owner, descriptor, invocation))
            {
                matching[matchingCount++] = index;
            }
        }

        bool succeeded = kind switch
        {
            MockVerificationCountKind.Exactly =>
                matchingCount == expected,
            MockVerificationCountKind.AtLeast =>
                matchingCount >= expected,
            MockVerificationCountKind.AtMost =>
                matchingCount <= expected,
            _ => throw new UnreachableException()
        };
        if (!succeeded)
        {
            throw new MockException(
                $"Receiver-free verification for " +
                $"'{descriptor.Operation.DeclaringType?.FullName}." +
                $"{descriptor.Operation.Name}' expected " +
                $"{Describe(kind, expected)}, but observed " +
                $"{matchingCount} matching invocations.");
        }

        ledger.MarkVerifiedAtomically(
            snapshot,
            matching.AsSpan(0, matchingCount));
    }

    private static bool Matches(
        MockSession owner,
        MockReceiverFreeSetupDescriptor descriptor,
        MockInvocation invocation)
    {
        MockInvocationIdentity identity = invocation.Identity;
        MockInvocationTarget target = identity.Target;
        if (target.Kind != MockInvocationTargetKind.CallSite ||
            target.OwnerId != owner.Id ||
            !Equals(identity.Operation, descriptor.Operation) ||
            target.OperationKind != descriptor.OperationKind)
        {
            return false;
        }
        if (descriptor.Site is not null &&
            (target.ModuleVersionId !=
                descriptor.Site.Descriptor.ModuleVersionId ||
             target.ContainingMethodToken !=
                descriptor.Site.Descriptor.ContainingMethodToken ||
             target.IlOffset !=
                descriptor.Site.Descriptor.OriginalIlOffset))
        {
            return false;
        }

        ReadOnlySpan<MockInvocationArgument> arguments =
            invocation.Arguments;
        int offset = 0;
        if (target.OperationKind ==
            MockInvocationOperationKind.ConstructorBody)
        {
            offset = 1;
        }
        else if (identity.Operation is FieldInfo { IsStatic: false })
        {
            if (arguments.Length == 0 ||
                arguments[0].Entry.Kind ==
                    MockInvocationArgumentSnapshotKind.Unavailable ||
                !ReferenceEquals(
                    descriptor.Receiver,
                    arguments[0].Entry.Value))
            {
                return false;
            }

            offset = 1;
        }

        ReadOnlySpan<MockArgumentPattern> patterns =
            descriptor.Patterns;
        if (patterns.Length != arguments.Length - offset)
            return false;
        for (int index = 0; index < patterns.Length; index++)
        {
            MockInvocationArgument argument =
                arguments[index + offset];
            MockArgumentPattern pattern = patterns[index];
            if (argument.Entry.Unavailable?.Reason ==
                MockUnavailableReason.OutHasNoEntryValue)
            {
                continue;
            }

            Type valueType = argument.DeclaredType.IsByRef
                ? argument.DeclaredType.GetElementType()!
                : argument.DeclaredType;
            if (valueType.IsByRefLike &&
                pattern.Value is not Matcher)
            {
                continue;
            }

            object? actual =
                argument.Entry.Kind ==
                    MockInvocationArgumentSnapshotKind.Unavailable
                    ? null
                    : argument.Entry.Value;
            if (!pattern.Matches(actual))
                return false;
        }

        return true;
    }

    private static bool IsInWindow(
        MockInvocation invocation,
        MockSession? session,
        MockCheckpoint after,
        MockCheckpoint through) =>
        session is null ||
        invocation.Coordinate.TimelineId == after.TimelineId &&
        invocation.Coordinate.Sequence > after.Sequence &&
        invocation.Coordinate.Sequence <= through.Sequence;

    private static string Describe(
        MockVerificationCountKind kind,
        int expected) =>
        kind switch
        {
            MockVerificationCountKind.Exactly =>
                $"exactly {expected}",
            MockVerificationCountKind.AtLeast =>
                $"at least {expected}",
            MockVerificationCountKind.AtMost =>
                $"at most {expected}",
            _ => throw new UnreachableException()
        };
}
