namespace AlvorKit.ECS.Demo;

/// <summary>Declares the generated component accessors used by the ECS demo.</summary>
[Components]
public interface ICombatComponents
{
    /// <summary>The display name written into <see cref="EntHandle.ToString" /> output.</summary>
    [ComponentToString] string Name { get; set; }

    /// <summary>The current health value written into <see cref="EntHandle.ToString" /> output.</summary>
    [ComponentToString] int Health { get; set; }

    /// <summary>An optional team tag that can be set, queried, and removed.</summary>
    string? Team { get; set; }
}
