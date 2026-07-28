namespace AlvorKit.Mocking;

/// <summary>Owns thread-local mocked-call capture and matcher state.</summary>
internal static partial class Capture
{
    /// <summary>The current capture state for this thread.</summary>
    private static readonly ThreadLocal<CaptureContext> context = new();

    /// <summary>Matchers captured during the first setup invocation.</summary>
    private static readonly ThreadLocal<List<Matcher>> firstMatchers = new(() => []);

    /// <summary>Matchers captured during the disambiguation invocation.</summary>
    private static readonly ThreadLocal<List<Matcher>> secondMatchers = new(() => []);

    /// <summary>Nested start attempts whose matching finally block must not end the outer capture.</summary>
    private static readonly ThreadLocal<int> rejectedStarts = new();

    /// <summary>Gets the current capture state for this thread.</summary>
    internal static CaptureContext Context => context.Value;

    /// <summary>Gets the first-pass matchers for this thread.</summary>
    internal static List<Matcher> FirstMatchers => firstMatchers.Value!;

    /// <summary>Gets the second-pass matchers for this thread.</summary>
    internal static List<Matcher> SecondMatchers => secondMatchers.Value!;

    /// <summary>Runs one complete setup or verification capture operation.</summary>
    internal static MockCapturedInvocation Run(
        CaptureOperation operation,
        Action invoke)
        => Run(
            operation,
            null,
            invoke);

    /// <summary>
    /// Runs capture while allowing earlier nonmatching interception operations to
    /// execute their originals.
    /// </summary>
    internal static MockCapturedInvocation Run(
        CaptureOperation operation,
        MockInvocationOperationKind expectedOperationKind,
        Action invoke)
        => Run(
            operation,
            (MockInvocationOperationKind?)expectedOperationKind,
            invoke);

    private static MockCapturedInvocation Run(
        CaptureOperation operation,
        MockInvocationOperationKind? expectedOperationKind,
        Action invoke)
    {
        ArgumentNullException.ThrowIfNull(invoke);
        MockGenericCallsite.Prepare(invoke);

        return RunPrepared(
            operation,
            expectedOperationKind,
            invoke);
    }

    private static MockCapturedInvocation RunPrepared(
        CaptureOperation operation,
        MockInvocationOperationKind? expectedOperationKind,
        Action invoke)
    {
        object?[]? firstArguments = null;

        try
        {
            Start(operation, expectedOperationKind);
            invoke();
            ValidateSingleInvocation();

            var first = Context;
            firstArguments = first.Args;
            if (FirstMatchers.Count > 0)
            {
                Disambiguate();
                invoke();
                ValidateDisambiguation(first);
                CaptureOrdinaryMatcherProcessing.Process(
                    Context,
                    first.Method!,
                    first.Args!,
                    FirstMatchers);
            }

            ProcessIndexedMatchers(first.Method!, first.Args!);
            var mocked = Mock.GetMocked(first.Instance!) ??
                throw new MockException("The captured receiver is not owned by the mocking runtime.");
            return new(first.Instance!, mocked, first.Method!, first.Args!);
        }
        finally
        {
            firstArguments?.AsSpan().Clear();
            End();
        }
    }

    /// <summary>Starts event capture for the existing event-raising bridge.</summary>
    internal static void Start() => Start(CaptureOperation.Event);

    /// <summary>Starts one explicit capture operation for this thread.</summary>
    internal static void Start(CaptureOperation operation) =>
        Start(operation, null);

    private static void Start(
        CaptureOperation operation,
        MockInvocationOperationKind? expectedOperationKind)
    {
        if (Context.IsActive)
        {
            rejectedStarts.Value++;
            throw new MockException(
                $"Cannot start {operation.ToString().ToLowerInvariant()} capture while " +
                $"{Context.Operation.ToString().ToLowerInvariant()} capture is active.");
        }

        FirstMatchers.Clear();
        ClearFirstIndexedMatchers();
        context.Value = new()
        {
            IsActive = true,
            Operation = operation,
            ExpectedOperationKind = expectedOperationKind
        };
    }

    /// <summary>Switches the current setup capture into matcher disambiguation mode.</summary>
    internal static void Disambiguate()
    {
        SecondMatchers.Clear();
        ClearSecondIndexedMatchers();
        context.Value = context.Value with
        {
            IsDisambiguating = true,
            InvocationCount = 0
        };
    }

    /// <summary>Ends capture and clears thread-local matcher lists.</summary>
    internal static void End()
    {
        if (rejectedStarts.Value > 0)
        {
            rejectedStarts.Value--;
            return;
        }

        Context.Args?.AsSpan().Clear();
        context.Value = default;
        FirstMatchers.Clear();
        SecondMatchers.Clear();
        ClearFirstIndexedMatchers();
        ClearSecondIndexedMatchers();
    }

    /// <summary>Records a captured invocation and its argument values.</summary>
    internal static bool TryWrite(
        object cinstance,
        MethodInfo cmethod,
        object?[] cargs)
    {
        MockInvocationOperationKind? expected =
            Context.ExpectedOperationKind;
        if (expected is not null &&
            Mock.GetMocked(cinstance)?.ReceiverFree?.Site.OperationKind !=
                expected)
        {
            return false;
        }

        EnsureFirstInvocation();
        context.Value = context.Value with
        {
            InvocationCount = 1,
            Instance = cinstance,
            Method = cmethod,
            Args = cargs
        };
        return true;
    }

    /// <summary>Stores an argument matcher in the active capture pass.</summary>
    internal static void WriteMatcher(Matcher matcher)
    {
        if (Context.IsDisambiguating)
            SecondMatchers.Add(matcher);
        else FirstMatchers.Add(matcher);
    }

    private static void EnsureFirstInvocation()
    {
        if (!Context.IsActive)
            throw new MockException("Cannot write a captured invocation when capture is inactive.");
        if (Context.InvocationCount != 0)
            throw new MockException("A capture expression must contain exactly one mocked call.");
    }

    private static void ValidateSingleInvocation()
    {
        if (Context.InvocationCount != 1 ||
            Context.Instance is null ||
            Context.Method is null ||
            Context.Args is null)
        {
            throw new MockException(
                $"Failed to capture one mocked call for {Context.Operation.ToString().ToLowerInvariant()}.");
        }
    }

    private static void ValidateDisambiguation(CaptureContext first)
    {
        ValidateSingleInvocation();

        if (first.Method != Context.Method ||
            !ReferenceEquals(first.Instance, Context.Instance))
        {
            throw new MockException(
                "Matcher disambiguation must replay the same mocked receiver and method.");
        }

        if (FirstMatchers.Count != SecondMatchers.Count)
        {
            throw new MockException(
                $"Matcher capture changed from {FirstMatchers.Count} to " +
                $"{SecondMatchers.Count} matchers between passes.");
        }

        ValidateIndexedDisambiguation();
    }

    /// <summary>Gets the synthetic receiver prefix used by captured operation kinds.</summary>
    private static int CaptureParameterOffset() =>
        Context.ExpectedOperationKind is
            MockInvocationOperationKind.ConstructorBody or
            MockInvocationOperationKind.StructMethod
            ? 1
            : 0;

}
