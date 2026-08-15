namespace AlvorKit;

/// <summary>Appends captured and retained invocation shapes without running user code.</summary>
internal static class MockDiagnosticFormatter
{
    private const int MaximumArgumentCount = 8;

    /// <summary>Appends a captured target, signature, and matcher descriptions.</summary>
    internal static void AppendCaptured(
        StringBuilder message,
        MockCapturedInvocation captured)
    {
        message
            .Append("mock #")
            .Append(captured.Mocked.Invocations.Id)
            .Append(' ');
        MockDiagnosticSignatureFormatter.AppendSignature(
            message,
            captured.Method.DeclaringType ?? typeof(object),
            captured.Method);
        message.Append(" patterns=[");
        var patterns = captured.DeclaredPatterns;
        var parameters = captured.Method.GetParameters();
        for (var i = 0; i < patterns.Length; i++)
        {
            if (i > 0)
                message.Append(", ");

            AppendPattern(
                message,
                patterns[i],
                parameters[i].ParameterType);
        }

        message.Append(']');
    }

    /// <summary>Appends one invocation candidate and its retained entry state.</summary>
    internal static void AppendInvocation(
        StringBuilder message,
        MockInvocation invocation)
    {
        message
            .Append('#')
            .Append(invocation.Coordinate.Sequence)
            .Append(" mock #")
            .Append(invocation.Identity.Target.OwnerId)
            .Append(' ');
        AppendOperation(
            message,
            invocation.Identity.Operation);
        message
            .Append(" [")
            .Append(invocation.Identity.Backend)
            .Append("; ")
            .Append(invocation.Completion.Kind)
            .Append("; ")
            .Append(invocation.Completion.Source);
        if (invocation.Completion.FailureStage is { } failureStage)
            message.Append("; ").Append(failureStage);
        if (invocation.AsyncCompletion is { } asyncCompletion)
        {
            message
                .Append("; async ")
                .Append(asyncCompletion.Kind);
        }

        message.Append(']');
        AppendEntryArguments(
            message,
            invocation.Arguments);
    }

    /// <summary>Appends a retained shallow, projected, or unavailable snapshot.</summary>
    internal static void AppendSnapshot(
        StringBuilder message,
        MockInvocationArgumentSnapshot snapshot)
    {
        message
            .Append('#')
            .Append(snapshot.DeclaredIndex)
            .Append(' ')
            .Append(snapshot.Phase.ToString().ToLowerInvariant())
            .Append(' ');

        if (snapshot.Kind ==
            MockInvocationArgumentSnapshotKind.Unavailable)
        {
            message
                .Append("<unavailable: ")
                .Append(snapshot.Unavailable!.Reason)
                .Append('>');
            return;
        }

        message.Append(
            snapshot.Kind ==
            MockInvocationArgumentSnapshotKind.Projected
                ? "<projected> "
                : "<shallow> ");
        MockDiagnosticValueFormatter.AppendValue(
            message,
            snapshot.Value,
            snapshot.DeclaredType);
    }

    private static void AppendEntryArguments(
        StringBuilder message,
        ReadOnlySpan<MockInvocationArgument> arguments)
    {
        if (arguments.Length == 0)
            return;

        message.Append(" entry=[");
        var count = Math.Min(
            arguments.Length,
            MaximumArgumentCount);
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
                message.Append(", ");

            AppendSnapshot(
                message,
                arguments[i].Entry);
        }

        if (arguments.Length > count)
        {
            message
                .Append(", ... (+")
                .Append(arguments.Length - count)
                .Append(" more)");
        }

        message.Append(']');
    }

    private static void AppendPattern(
        StringBuilder message,
        MockArgumentPattern pattern,
        Type declaredType)
    {
        if (pattern.Value is Matcher matcher)
        {
            message.Append(
                matcher.Type == MatcherType.Any
                    ? "any"
                    : "predicate");
            return;
        }

        message.Append("exact ");
        MockDiagnosticValueFormatter.AppendValue(
            message,
            pattern.Value,
            declaredType);
    }

    private static void AppendOperation(
        StringBuilder message,
        MemberInfo operation)
    {
        if (operation is MethodInfo method)
        {
            MockDiagnosticSignatureFormatter.AppendSignature(
                message,
                method.DeclaringType ?? typeof(object),
                method);
            return;
        }

        MockDiagnosticValueFormatter.AppendType(
            message,
            operation.DeclaringType ?? typeof(object));
        message.Append('.').Append(operation.Name);
    }
}
