namespace AlvorKit.Mocking;

/// <summary>
/// Verifies struct calls by operation, explicit mode, retained entry copies,
/// arguments, site, and checkpoint window.
/// </summary>
internal static class MockStructVerificationRuntime
{
    internal static void Verify<T>(
        MockSession owner,
        MockStructSetupDescriptor descriptor,
        MockCapturedInvocation captured,
        MockVerificationCountKind kind,
        int expected,
        MockSession? windowSession,
        MockCheckpoint after,
        MockCheckpoint through)
        where T : struct
    {
        Validate<T>(
            owner,
            descriptor,
            captured,
            windowSession,
            after,
            through);
        var plans = new List<(
            MockInvocationLedger Ledger,
            MockInvocationLedgerSnapshot Snapshot,
            int[] Indices,
            int Count)>();
        int matchingCount = 0;
        foreach (MockInvocationParticipant participant in
            owner.Participants)
        {
            MockInvocationLedger ledger =
                participant.Invocations;
            MockInvocationLedgerSnapshot snapshot =
                ledger.Snapshot();
            ReadOnlySpan<MockInvocation> invocations =
                snapshot.Invocations;
            var indices = new int[invocations.Length];
            int count = 0;
            for (int index = 0;
                index < invocations.Length;
                index++)
            {
                MockInvocation invocation = invocations[index];
                if (IsInWindow(
                        invocation,
                        windowSession,
                        after,
                        through) &&
                    Matches<T>(
                        descriptor,
                        captured,
                        invocation))
                {
                    indices[count++] = index;
                }
            }

            if (count != 0)
            {
                plans.Add(
                    (ledger, snapshot, indices, count));
                matchingCount += count;
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
                $"Struct verification for " +
                $"'{captured.Operation.DeclaringType?.FullName}." +
                $"{captured.Operation.Name}' expected " +
                $"{Describe(kind, expected)}, but observed " +
                $"{matchingCount} matching invocations.");
        }

        foreach (var plan in plans)
        {
            plan.Ledger.MarkVerifiedAtomically(
                plan.Snapshot,
                plan.Indices.AsSpan(0, plan.Count));
        }
    }

    private static void Validate<T>(
        MockSession owner,
        MockStructSetupDescriptor descriptor,
        MockCapturedInvocation captured,
        MockSession? windowSession,
        MockCheckpoint after,
        MockCheckpoint through)
        where T : struct
    {
        MockReceiverFreeIdentity identity =
            captured.Mocked.ReceiverFree ??
            throw new MockException(
                "Struct verification requires a interception live-ref call site.");
        if (identity.Site.OperationKind !=
                MockInvocationOperationKind.StructMethod ||
            identity.Operation is not MethodInfo
            {
                IsStatic: false,
                DeclaringType: { } declaringType
            } ||
            !(declaringType == typeof(T) ||
              declaringType.IsInterface &&
              declaringType.IsAssignableFrom(typeof(T))))
        {
            throw new MockException(
                $"Captured operation is not a struct method on '{typeof(T)}'.");
        }
        descriptor.Scope.Site?.Validate(
            identity.Operation,
            MockInvocationOperationKind.StructMethod);
        if (windowSession is not null)
        {
            if (!ReferenceEquals(owner, windowSession))
            {
                throw new MockException(
                    "Struct checkpoint verification belongs to another " +
                    "mock session.");
            }

            owner.ValidateWindow(after, through);
        }
    }

    private static bool Matches<T>(
        MockStructSetupDescriptor descriptor,
        MockCapturedInvocation captured,
        MockInvocation invocation)
        where T : struct
    {
        MockInvocationIdentity identity = invocation.Identity;
        MockInvocationTarget target = identity.Target;
        if (target.Kind != MockInvocationTargetKind.CallSite ||
            target.OperationKind !=
                MockInvocationOperationKind.StructMethod ||
            !Equals(identity.Operation, captured.Operation))
        {
            return false;
        }
        if (descriptor.Scope.Site is { } site &&
            (target.ModuleVersionId !=
                site.Descriptor.ModuleVersionId ||
             target.ContainingMethodToken !=
                site.Descriptor.ContainingMethodToken ||
             target.IlOffset !=
                site.Descriptor.OriginalIlOffset))
        {
            return false;
        }

        ReadOnlySpan<MockInvocationArgumentSnapshot> entries =
            invocation.SelectionArguments;
        ReadOnlySpan<MockArgumentPattern> patterns =
            captured.DeclaredPatterns;
        if (entries.Length != patterns.Length ||
            entries.Length == 0 ||
            entries[0].Value is not T receiver)
        {
            return false;
        }
        if (descriptor.Scope.Mode ==
                MockStructMode.ValueMatched &&
            !((RefPredicate<T>)descriptor.Scope.Predicate!)(
                in receiver))
        {
            return false;
        }

        for (int index = 1; index < entries.Length; index++)
        {
            MockInvocationArgumentSnapshot entry =
                entries[index];
            MockArgumentPattern pattern = patterns[index];
            if (entry.Unavailable?.Reason ==
                MockUnavailableReason.OutHasNoEntryValue)
            {
                continue;
            }
            if (entry.Kind ==
                MockInvocationArgumentSnapshotKind.Unavailable)
            {
                if (pattern.Value is not Matcher
                    {
                        Type: MatcherType.Any
                    })
                {
                    return false;
                }

                continue;
            }
            if (!pattern.Matches(entry.Value))
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
