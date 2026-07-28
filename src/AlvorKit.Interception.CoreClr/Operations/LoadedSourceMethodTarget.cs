namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>
/// Retains selected source identity and the exact executable loaded body used for planning.
/// </summary>
public sealed class LoadedSourceMethodTarget
{
    /// <summary>The exact selected source MethodDef identity.</summary>
    private readonly InterceptionTarget sourceMethod;

    /// <summary>The exact source or generated executable MethodDef identity.</summary>
    private readonly InterceptionTarget bodyMethod;

    /// <summary>The authoritative executable loaded body.</summary>
    private readonly LoadedMethodBodySnapshot body;

    /// <summary>The synchronous or state-machine source relationship.</summary>
    private readonly LoadedSourceMethodKind kind;

    /// <summary>The deterministic source-facing diagnostic name.</summary>
    private readonly string sourceAttribution;

    /// <summary>The deterministic executable-body diagnostic name.</summary>
    private readonly string bodyAttribution;

    /// <summary>Creates one fully resolved source-to-loaded-body target.</summary>
    internal LoadedSourceMethodTarget(
        InterceptionTarget sourceMethod,
        InterceptionTarget bodyMethod,
        LoadedMethodBodySnapshot body,
        LoadedSourceMethodKind kind,
        string sourceAttribution,
        string bodyAttribution)
    {
        this.sourceMethod = sourceMethod;
        this.bodyMethod = bodyMethod;
        this.body = body;
        this.kind = kind;
        this.sourceAttribution = sourceAttribution;
        this.bodyAttribution = bodyAttribution;
    }

    /// <summary>Gets the exact selected source MethodDef identity.</summary>
    public InterceptionTarget SourceMethod => sourceMethod;

    /// <summary>Gets the exact source or generated executable MethodDef identity.</summary>
    public InterceptionTarget BodyMethod => bodyMethod;

    /// <summary>Gets the authoritative executable loaded body.</summary>
    public LoadedMethodBodySnapshot Body => body;

    /// <summary>Gets the authoritative identity of the executable loaded body.</summary>
    public LoadedMethodBodyIdentity BodyIdentity => body.Identity;

    /// <summary>Gets the synchronous or state-machine source relationship.</summary>
    public LoadedSourceMethodKind Kind => kind;

    /// <summary>Gets whether the executable body belongs to a generated state machine.</summary>
    public bool UsesGeneratedBody => sourceMethod != bodyMethod;

    /// <summary>Gets the deterministic selected-source diagnostic attribution.</summary>
    public string SourceAttribution => sourceAttribution;

    /// <summary>Gets the deterministic executable-body diagnostic attribution.</summary>
    public string BodyAttribution => bodyAttribution;
}
