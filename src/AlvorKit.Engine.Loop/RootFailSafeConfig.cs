namespace AlvorKit;

/// <summary>Configures whether unhandled exceptions request root shutdown before process exit.</summary>
[Root]
public class RootFailSafeConfig
{
    private readonly bool enabled = true;

    /// <summary>Gets whether unhandled exceptions request root shutdown before process exit.</summary>
    public bool Enabled
    {
        get => enabled;
        init => enabled = value;
    }
}
