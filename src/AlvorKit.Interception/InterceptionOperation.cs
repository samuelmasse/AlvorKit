namespace AlvorKit;

/// <summary>Identifies a native method-version lifecycle request.</summary>
public enum InterceptionOperation
{
    /// <summary>No request is represented.</summary>
    None = 0,

    /// <summary>A new patch is being installed.</summary>
    Install = 1,

    /// <summary>An active patch is being replaced atomically.</summary>
    Replace = 2,

    /// <summary>An active patch is being removed.</summary>
    Remove = 3
}
