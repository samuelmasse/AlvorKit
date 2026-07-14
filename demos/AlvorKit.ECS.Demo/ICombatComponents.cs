namespace AlvorKit.ECS.Demo;

/// <summary>Declares the generated component accessors used by the ECS demo.</summary>
[Components]
public interface ICombatComponents
{
    /// <summary>The display name written into <see cref="EntHandle.ToString" /> output.</summary>
    [ComponentToString] string Name { get; set; }

    /// <summary>The current health value written into <see cref="EntHandle.ToString" /> output.</summary>
    [Archetypal]
    [ComponentToString]
    int Health { get; set; }

    /// <summary>The unit's current world position.</summary>
    [Archetypal]
    Position Position { get; set; }

    /// <summary>The unit's movement applied by a future archetypal query.</summary>
    [Archetypal]
    Velocity Velocity { get; set; }

    /// <summary>An optional team tag that can be set, queried, and removed.</summary>
    string? Team { get; set; }
}

/// <summary>A small demo position component.</summary>
public readonly record struct Position(int X, int Y);

/// <summary>A small demo velocity component.</summary>
public readonly record struct Velocity(int X, int Y);
