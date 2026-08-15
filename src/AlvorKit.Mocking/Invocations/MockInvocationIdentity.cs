namespace AlvorKit;

/// <summary>Identifies the target, member, and backend of one intercepted operation.</summary>
internal sealed record MockInvocationIdentity
{
    /// <summary>Creates an invocation identity without formatting the target.</summary>
    internal MockInvocationIdentity(
        MockInvocationTarget target,
        MemberInfo operation,
        string backend)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(backend);

        Target = target;
        Operation = operation;
        Backend = backend;
    }

    /// <summary>Gets the mock or call-site target.</summary>
    internal MockInvocationTarget Target { get; }

    /// <summary>Gets the intercepted method, constructor, or field.</summary>
    internal MemberInfo Operation { get; }

    /// <summary>Gets the instrumentation backend identity.</summary>
    internal string Backend { get; }
}
