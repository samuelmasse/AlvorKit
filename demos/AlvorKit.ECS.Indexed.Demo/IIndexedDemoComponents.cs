namespace AlvorKit.ECS.Indexed.Demo;

/// <summary>Declares the generated component accessors used by the indexed ECS demo.</summary>
[Components]
public interface IIndexedDemoComponents
{
    /// <summary>The display name written into diagnostic output.</summary>
    [ComponentToString] string Name { get; set; }

    /// <summary>A stable id maintained by a pre-set hook.</summary>
    Guid Id { get; set; }

    /// <summary>A health value observed by pre and post hooks.</summary>
    [ComponentToString] int Health { get; set; }

    /// <summary>The readiness gate used by the demo's gated projectile bag.</summary>
    bool IsReady { get; set; }

    /// <summary>A marker for projectile entities.</summary>
    bool IsProjectile { get; set; }

    /// <summary>A plain marker used by an ungated bag.</summary>
    bool IsScratched { get; set; }
}
