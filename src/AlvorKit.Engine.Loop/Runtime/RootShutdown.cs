namespace AlvorKit.Engine.Loop;

/// <summary>Tracks whether the root loop is shutting down after a requested close or runtime failure.</summary>
[Root]
public sealed class RootShutdown(RootScreen screen)
{
    /// <summary>Gets whether shutdown has started.</summary>
    public bool Started => screen.IsExiting;

    /// <summary>Requests shutdown from the host window.</summary>
    public void Start() => screen.Close();
}
