namespace AlvorKit.Script.LiveWorkspace;

/// <summary>Runtime mechanism responsible for one tracked live intervention.</summary>
public enum LiveWorkspaceInterventionKind
{
    /// <summary>A submitted LiveCode command left a persistent effect.</summary>
    LiveCode,

    /// <summary>A LivePatch handler replaced an existing method.</summary>
    LivePatch,

    /// <summary>A predefined bridge left a persistent effect.</summary>
    Bridge
}
