namespace AlvorKit.Script.LiveWorkspace;

/// <summary>Cleanup state of one persistent intervention in a live process.</summary>
public enum LiveWorkspaceInterventionState
{
    /// <summary>The intervention still affects the running process.</summary>
    Active,

    /// <summary>The intervention is being removed and requires another status observation.</summary>
    Removing,

    /// <summary>The intervention was reverted or otherwise proved inactive.</summary>
    Resolved,

    /// <summary>The intervention can only be cleared by restarting the target process.</summary>
    RestartRequired
}
