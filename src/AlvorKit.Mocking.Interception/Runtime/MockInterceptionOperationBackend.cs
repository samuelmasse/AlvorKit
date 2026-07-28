namespace AlvorKit.Mocking;

/// <summary>
/// Binds Mocking operations to the Interception-backed runtime.
/// </summary>
internal sealed class MockInterceptionOperationBackend :
    IMockOperationBackend
{
    /// <summary>Gets the single operation backend instance.</summary>
    internal static MockInterceptionOperationBackend Instance { get; } =
        new();

    private MockInterceptionOperationBackend()
    {
    }

    /// <inheritdoc />
    public string Name => "interception";

    /// <inheritdoc />
    public TDelegate BindInterception<TDelegate>(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        TDelegate original)
        where TDelegate : Delegate =>
        MockInterceptionRuntime.Bind(
            site,
            operation,
            original);
}
