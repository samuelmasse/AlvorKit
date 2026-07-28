namespace AlvorKit.Mocking;

/// <summary>
/// API-to-runtime seam implemented by the receiver-free session control plane.
/// </summary>
internal static class MockReceiverFreeApiBoundary
{
    internal static MockCallSite CaptureSite(Action operation) =>
        WithCaptureSession(
            () => CreateSite(
                Capture.Run(
                    CaptureOperation.Verification,
                    operation)));

    internal static MockCallSite CaptureSite<T>(Func<T> operation)
        where T : allows ref struct =>
        WithCaptureSession(
            () => CreateSite(
                Capture.Run(
                    CaptureOperation.Verification,
                    operation)));

    internal static MockReceiverFreeSetupPublisher CaptureSetup<T>(
        Func<T> operation,
        MockInvocationOperationKind operationKind)
        where T : allows ref struct
    {
        EnsureSession("Receiver-free setup");
        return Setup(
            Capture.Run(
                CaptureOperation.Setup,
                operationKind,
                operation),
            operationKind);
    }

    internal static MockReceiverFreeVerificationContract CaptureVerification<T>(
        Func<T> operation,
        MockInvocationOperationKind operationKind)
        where T : allows ref struct
    {
        EnsureSession("Receiver-free verification");
        return Verification(
            Capture.Run(
                CaptureOperation.Verification,
                operationKind,
                operation),
            operationKind);
    }

    internal static MockReceiverFreeSetupPublisher Setup(
        MockCapturedInvocation captured)
    {
        MockReceiverFreeIdentity identity = RequireIdentity(captured);
        return Setup(captured, identity.Site.OperationKind);
    }

    internal static MockReceiverFreeVerificationContract Verification(
        MockCapturedInvocation captured)
    {
        MockReceiverFreeIdentity identity = RequireIdentity(captured);
        return Verification(captured, identity.Site.OperationKind);
    }

    internal static MockReceiverFreeSetupPublisher FieldSetup<T>(
        FieldInfo field,
        MockInvocationOperationKind operationKind,
        object? receiver,
        Func<T>? value)
        where T : allows ref struct
    {
        MockSession owner = RequireSession(
            "Receiver-free field setup");
        var descriptor = new MockReceiverFreeSetupDescriptor(
            field,
            operationKind,
            receiver,
            MockFieldPatternCapture.Capture(operationKind, value));
        return new(
            descriptor,
            owner.AddReceiverFreeSetup);
    }

    internal static MockReceiverFreeVerificationContract FieldVerification<T>(
        FieldInfo field,
        MockInvocationOperationKind operationKind,
        object? receiver,
        Func<T>? value)
        where T : allows ref struct
    {
        MockSession owner = RequireSession(
            "Receiver-free field verification");
        var descriptor = new MockReceiverFreeSetupDescriptor(
            field,
            operationKind,
            receiver,
            MockFieldPatternCapture.Capture(operationKind, value));
        return new(
            descriptor,
            (scope, kind, expected, session, after, through) =>
                MockReceiverFreeVerification.Verify(
                    owner,
                    scope,
                    kind,
                    expected,
                    session,
                    after,
                    through));
    }

    private static MockReceiverFreeSetupPublisher Setup(
        MockCapturedInvocation captured,
        MockInvocationOperationKind operationKind)
    {
        MockReceiverFreeIdentity identity =
            ValidateIdentity(captured, operationKind);
        ReadOnlySpan<MockArgumentPattern> patterns =
            CapturedPatterns(captured, identity);
        var descriptor = new MockReceiverFreeSetupDescriptor(
            identity.Operation,
            operationKind,
            null,
            patterns);
        MockSession owner = RequireSession(
            "Receiver-free setup");
        return new(
            descriptor,
            owner.AddReceiverFreeSetup);
    }

    private static MockReceiverFreeVerificationContract Verification(
        MockCapturedInvocation captured,
        MockInvocationOperationKind operationKind)
    {
        MockReceiverFreeIdentity identity =
            ValidateIdentity(captured, operationKind);
        ReadOnlySpan<MockArgumentPattern> patterns =
            CapturedPatterns(captured, identity);
        var descriptor = new MockReceiverFreeSetupDescriptor(
            identity.Operation,
            operationKind,
            null,
            patterns);
        MockSession owner = RequireSession(
            "Receiver-free verification");
        return new(
            descriptor,
            (scope, kind, expected, session, after, through) =>
                MockReceiverFreeVerification.Verify(
                    owner,
                    scope,
                    kind,
                    expected,
                    session,
                    after,
                    through));
    }

    private static ReadOnlySpan<MockArgumentPattern> CapturedPatterns(
        MockCapturedInvocation captured,
        MockReceiverFreeIdentity identity) =>
        identity.Site.OperationKind ==
            MockInvocationOperationKind.ConstructorBody
            ? captured.DeclaredPatterns[1..]
            : captured.DeclaredPatterns;

    private static MockCallSite CreateSite(
        MockCapturedInvocation captured)
    {
        MockReceiverFreeIdentity identity = RequireIdentity(captured);
        return new(identity.Site, identity.Operation);
    }

    private static TResult WithCaptureSession<TResult>(
        Func<TResult> capture)
    {
        if (MockSession.Current is not null)
            return capture();

        using var session = new MockSession();
        return capture();
    }

    private static MockReceiverFreeIdentity ValidateIdentity(
        MockCapturedInvocation captured,
        MockInvocationOperationKind operationKind)
    {
        MockReceiverFreeIdentity identity = RequireIdentity(captured);
        if (identity.Site.OperationKind != operationKind)
        {
            throw new MockException(
                $"Captured receiver-free operation '{identity.Operation.Name}' " +
                $"has kind '{identity.Site.OperationKind}', not " +
                $"'{operationKind}'.");
        }

        return identity;
    }

    private static MockReceiverFreeIdentity RequireIdentity(
        MockCapturedInvocation captured) =>
        captured.Mocked.ReceiverFree ??
        throw new MockException(
            "The captured operation has an instance mock receiver instead of " +
            "a interception receiver-free call site.");

    private static void EnsureSession(string operation) =>
        _ = RequireSession(operation);

    private static MockSession RequireSession(string operation)
    {
        if (MockSession.Current is not { } current)
        {
            throw new MockException(
                MockDiagnostics.SessionMustBeCurrent(operation));
        }

        return current;
    }

}
