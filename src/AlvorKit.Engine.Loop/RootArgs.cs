namespace AlvorKit.Engine.Loop;

/// <summary>Describes the host, graphics layer, and first state used to start a root loop.</summary>
[Root]
public record class RootArgs
{
    /// <summary>Gets the host window that owns the platform event loop.</summary>
    public required IWindowHost Window { get; init; }

    /// <summary>Gets the strict OpenGL layer owned by the root loop.</summary>
    public required RootGl Gl { get; init; }

    /// <summary>Gets the concrete <see cref="State"/> type created before the first update.</summary>
    public required Type BootState { get; init; }

    /// <summary>Gets whether engine update, frame, and render callbacks convert failures into shutdown.</summary>
    public bool Failsafe { get; init; } = true;
}
