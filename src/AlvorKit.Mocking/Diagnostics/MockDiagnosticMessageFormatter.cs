namespace AlvorKit;

/// <summary>Applies deterministic collection and total-message bounds.</summary>
internal static class MockDiagnosticMessageFormatter
{
    private const int MaximumCandidateCount = 12;
    private const int MaximumMessageLength = 4096;

    /// <summary>Appends bounded invocation candidates in logical order.</summary>
    internal static void AppendCandidates(
        StringBuilder message,
        string heading,
        ReadOnlySpan<MockInvocation> candidates)
    {
        if (candidates.Length == 0)
        {
            message.Append(
                " No same-target invocation was recorded.");
            return;
        }

        message.Append(heading);
        var count = Math.Min(
            candidates.Length,
            MaximumCandidateCount);
        for (var i = 0; i < count; i++)
        {
            message.Append("\n  ");
            MockDiagnosticFormatter.AppendInvocation(
                message,
                candidates[i]);
        }

        if (candidates.Length > count)
        {
            message.Append("\n  ... (+")
                .Append(candidates.Length - count)
                .Append(" more)");
        }
    }

    /// <summary>Appends the current session and timeline when one is active.</summary>
    internal static void AppendCurrentSession(
        StringBuilder message)
    {
        var session = MockSession.Current;
        if (session is null)
            return;

        message
            .Append(" in session #")
            .Append(session.Id)
            .Append(" on timeline #")
            .Append(session.Timeline.Id);
    }

    /// <summary>Formats one count constraint without user values.</summary>
    internal static string DescribeConstraint(
        MockVerificationCountKind kind,
        int expected) =>
        kind switch
        {
            MockVerificationCountKind.Exactly =>
                $"exactly {expected} time(s)",
            MockVerificationCountKind.AtLeast =>
                $"at least {expected} time(s)",
            MockVerificationCountKind.AtMost =>
                $"at most {expected} time(s)",
            _ => throw new UnreachableException(
                $"Unknown count constraint '{kind}'.")
        };

    /// <summary>Appends a bounded deterministic sequence list.</summary>
    internal static void AppendSequences(
        StringBuilder message,
        ReadOnlySpan<long> sequences)
    {
        var count = Math.Min(
            sequences.Length,
            MaximumCandidateCount);
        for (var i = 0; i < count; i++)
            message.Append(i == 0 ? " " : ", ").Append(sequences[i]);

        if (sequences.Length > count)
        {
            message.Append(", … (+")
                .Append(sequences.Length - count)
                .Append(" more)");
        }
    }

    /// <summary>Returns a string capped at the diagnostics-wide message limit.</summary>
    internal static string Bound(StringBuilder message)
    {
        if (message.Length <= MaximumMessageLength)
            return message.ToString();

        message.Length = MaximumMessageLength - 16;
        return message.Append("... <truncated>").ToString();
    }
}
