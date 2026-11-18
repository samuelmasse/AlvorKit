namespace AlvorKit;

/// <summary>Mutable forward-only target ledger for one exact loaded module.</summary>
internal sealed class SourceUpdateModuleState(
    Assembly assembly,
    SourceUpdateModuleIdentity identity)
{
    internal Assembly Assembly { get; } = assembly;

    internal SourceUpdateModuleIdentity Identity { get; set; } = identity;

    internal Dictionary<string, SourceUpdateApplyResult> Results { get; } =
        new(StringComparer.Ordinal);
}
