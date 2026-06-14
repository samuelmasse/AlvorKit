namespace AlvorKit.Script.Bindgen;

/// <summary>Describes one callback typedef that should be surfaced as a typed managed delegate.</summary>
public sealed class CallbackConfig
{
    /// <summary>Managed delegate type name, for example GLDEBUGPROC to GlDebugProc.</summary>
    public required string ManagedName { get; set; }

    /// <summary>
    /// Maps callback parameter names to registry enum groups. Parameters not listed here use the
    /// catch-all enum because callback typedefs do not include group attributes.
    /// </summary>
    public Dictionary<string, string> ParamGroups { get; set; } = [];
}
