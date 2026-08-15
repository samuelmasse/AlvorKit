namespace AlvorKit;

/// <summary>
/// Holds immutable per-site metadata and the generated original-operation
/// delegate captured by one interception wrapper.
/// </summary>
internal sealed class MockInterceptionBindingState(
    MockInterceptionSiteDescriptor site,
    MemberInfo operation,
    MethodInfo logicalMethod,
    Delegate original)
{
    /// <summary>Gets the stable original call-site descriptor.</summary>
    internal MockInterceptionSiteDescriptor Site { get; } = site;

    /// <summary>Gets the exact constructed operation invoked at this site.</summary>
    internal MemberInfo Operation { get; } = operation;

    /// <summary>Gets the exact method-shaped dispatch signature.</summary>
    internal MethodInfo LogicalMethod { get; } = logicalMethod;

    /// <summary>Gets the infrastructure delegate that preserves the original opcode.</summary>
    internal Delegate Original { get; } = original;

    /// <summary>
    /// Gets the current session's synthetic receiver, or null for a no-session
    /// receiver-free bypass.
    /// </summary>
    internal object? Receiver =>
        MockSession.Current?.GetReceiverFreeTarget(
            Site,
            Operation,
            LogicalMethod);
}
