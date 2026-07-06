namespace AlvorKit.Engine.Loop;

/// <summary>Configures whether unhandled exceptions request root shutdown before process exit.</summary>
public sealed class FailSafeConfig
{
    /// <summary>Gets whether unhandled exceptions request root shutdown before process exit.</summary>
    public bool Enabled { get; init; } = true;
}
