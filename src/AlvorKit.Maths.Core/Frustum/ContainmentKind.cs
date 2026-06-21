namespace AlvorKit.Maths;

/// <summary>Describes how one spatial shape relates to another.</summary>
public enum ContainmentKind
{
    /// <summary>The shapes do not overlap.</summary>
    Disjoint,

    /// <summary>The shapes overlap, but neither fully contains the other.</summary>
    Intersects,

    /// <summary>The tested shape is fully contained.</summary>
    Contains,
}
