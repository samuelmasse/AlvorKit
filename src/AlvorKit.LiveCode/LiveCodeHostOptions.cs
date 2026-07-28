namespace AlvorKit.LiveCode;

/// <summary>Configures one explicitly enabled, loopback-only LiveCode development endpoint.</summary>
public sealed record LiveCodeHostOptions(string Name)
{
    /// <summary>Gets the discovery directory shared with local LiveCode clients.</summary>
    public string DiscoveryDirectory { get; init; } = LiveCodeDiscovery.DefaultDirectory;

    /// <summary>Gets the loopback port, or zero to request an ephemeral port.</summary>
    public int Port { get; init; }

    /// <summary>Gets namespaces imported automatically while client-side Roslyn compiles submissions.</summary>
    public string[] GlobalUsings { get; init; } = [];

    /// <summary>Gets the maximum accepted compiled assembly size.</summary>
    public int MaximumAssemblyBytes { get; init; } = 4 * 1024 * 1024;

    /// <summary>Gets the maximum accepted UTF-8 JSON payload size for one bridge invocation.</summary>
    public int MaximumBridgePayloadBytes { get; init; } = 1024 * 1024;

    /// <summary>Gets whether clients may submit arbitrary compiled C# commands.</summary>
    public bool EnableCodeExecution { get; init; } = true;

    /// <summary>Gets whether clients may discover and invoke predefined structured bridges.</summary>
    public bool EnableBridges { get; init; } = true;

    /// <summary>
    /// Gets the optional frozen-game inspection configuration. A null value keeps the dedicated lane disabled.
    /// </summary>
    public LiveCodeFrozenInspectionOptions? FrozenInspection { get; init; }
}
