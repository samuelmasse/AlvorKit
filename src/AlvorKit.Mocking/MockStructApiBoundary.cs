namespace AlvorKit;

/// <summary>
/// Captures interception live-struct operations and publishes session-owned setup
/// and verification contracts without retaining receiver storage.
/// </summary>
internal static class MockStructApiBoundary
{
    internal static MockStructSetupPublisher Setup<T>(
        MockStructScopeDescriptor scope,
        MockStructCall<T> operation)
        where T : struct
    {
        MockSession owner = RequireCurrent("Struct setup");
        var descriptor =
            new MockStructSetupDescriptor(
                scope,
                operation,
                typeof(void));
        return new(
            descriptor,
            (published, behavior) =>
                Publish(
                    owner,
                    published,
                    behavior,
                    operation));
    }

    internal static MockStructSetupPublisher Setup<T, TResult>(
        MockStructScopeDescriptor scope,
        MockStructCall<T, TResult> operation)
        where T : struct
        where TResult : allows ref struct
    {
        MockSession owner = RequireCurrent("Struct setup");
        var descriptor =
            new MockStructSetupDescriptor(
                scope,
                operation,
                typeof(TResult));
        return new(
            descriptor,
            (published, behavior) =>
                Publish(
                    owner,
                    published,
                    behavior,
                    operation));
    }

    internal static MockStructVerificationContract Verification<T>(
        MockStructScopeDescriptor scope,
        MockStructCall<T> operation)
        where T : struct
    {
        MockSession owner = RequireCurrent("Struct verification");
        var descriptor =
            new MockStructSetupDescriptor(
                scope,
                operation,
                typeof(void));
        return new(
            descriptor,
            (verified, kind, expected, session, after, through) =>
                Verify(
                    owner,
                    verified,
                    kind,
                    expected,
                    session,
                    after,
                    through,
                    operation));
    }

    internal static MockStructVerificationContract Verification<T, TResult>(
        MockStructScopeDescriptor scope,
        MockStructCall<T, TResult> operation)
        where T : struct
        where TResult : allows ref struct
    {
        MockSession owner = RequireCurrent("Struct verification");
        var descriptor =
            new MockStructSetupDescriptor(
                scope,
                operation,
                typeof(TResult));
        return new(
            descriptor,
            (verified, kind, expected, session, after, through) =>
                Verify(
                    owner,
                    verified,
                    kind,
                    expected,
                    session,
                    after,
                    through,
                    operation));
    }

    private static void Publish<T>(
        MockSession owner,
        MockStructSetupDescriptor descriptor,
        MockStructBehavior behavior,
        MockStructCall<T> operation)
        where T : struct
    {
        EnsureOwner(owner, "Struct setup");
        T receiver = default;
        MockCapturedInvocation captured = Capture.Run(
            CaptureOperation.Setup,
            MockInvocationOperationKind.StructMethod,
            () => operation(ref receiver));
        Publish<T>(descriptor, behavior, captured);
    }

    private static void Publish<T, TResult>(
        MockSession owner,
        MockStructSetupDescriptor descriptor,
        MockStructBehavior behavior,
        MockStructCall<T, TResult> operation)
        where T : struct
        where TResult : allows ref struct
    {
        EnsureOwner(owner, "Struct setup");
        T receiver = default;
        MockCapturedInvocation captured = Capture.Run(
            CaptureOperation.Setup,
            MockInvocationOperationKind.StructMethod,
            () => _ = operation(ref receiver));
        Publish<T>(descriptor, behavior, captured);
    }

    private static void Publish<T>(
        MockStructSetupDescriptor descriptor,
        MockStructBehavior behavior,
        MockCapturedInvocation captured)
        where T : struct
    {
        MockReceiverFreeIdentity identity =
            MockStructSetupContract.ValidateCapture<T>(
                descriptor,
                captured);
        MethodInfo logicalMethod = captured.Method;
        MockStructSetupContract.ValidateMutableThis(
            descriptor,
            logicalMethod);
        MockArgumentPattern[] patterns =
            captured.DeclaredPatterns.ToArray();
        patterns[0] =
            MockStructSetupContract.ReceiverPattern<T>(
                descriptor.Scope);
        MockSnapshotProjector[] projectors =
            MockStructSetupContract.Projectors(descriptor);
        MockConfiguredBehavior configured =
            MockStructSetupContract.ConfigureBehavior(
                behavior,
                logicalMethod,
                descriptor.ResultType);
        descriptor.Scope.Site?.Validate(
            identity.Operation,
            MockInvocationOperationKind.StructMethod);
        captured.Mocked.AddSetup(
            new MockSetup(
                logicalMethod,
                patterns,
                configured,
                projectors,
                descriptor.Mutations,
                descriptor.Scope.Site));
    }

    private static void Verify<T>(
        MockSession owner,
        MockStructSetupDescriptor descriptor,
        MockVerificationCountKind kind,
        int expected,
        MockSession? windowSession,
        MockCheckpoint after,
        MockCheckpoint through,
        MockStructCall<T> operation)
        where T : struct
    {
        EnsureOwner(owner, "Struct verification");
        T receiver = default;
        MockCapturedInvocation captured = Capture.Run(
            CaptureOperation.Verification,
            MockInvocationOperationKind.StructMethod,
            () => operation(ref receiver));
        MockStructVerificationRuntime.Verify<T>(
            owner,
            descriptor,
            captured,
            kind,
            expected,
            windowSession,
            after,
            through);
    }

    private static void Verify<T, TResult>(
        MockSession owner,
        MockStructSetupDescriptor descriptor,
        MockVerificationCountKind kind,
        int expected,
        MockSession? windowSession,
        MockCheckpoint after,
        MockCheckpoint through,
        MockStructCall<T, TResult> operation)
        where T : struct
        where TResult : allows ref struct
    {
        EnsureOwner(owner, "Struct verification");
        T receiver = default;
        MockCapturedInvocation captured = Capture.Run(
            CaptureOperation.Verification,
            MockInvocationOperationKind.StructMethod,
            () => _ = operation(ref receiver));
        MockStructVerificationRuntime.Verify<T>(
            owner,
            descriptor,
            captured,
            kind,
            expected,
            windowSession,
            after,
            through);
    }

    private static MockSession RequireCurrent(string operation) =>
        MockSession.Current ??
        throw new MockException(
            MockDiagnostics.SessionMustBeCurrent(operation));

    private static void EnsureOwner(
        MockSession owner,
        string operation)
    {
        if (!ReferenceEquals(MockSession.Current, owner))
        {
            throw new MockException(
                MockDiagnostics.SessionMustBeCurrent(operation));
        }
    }
}
