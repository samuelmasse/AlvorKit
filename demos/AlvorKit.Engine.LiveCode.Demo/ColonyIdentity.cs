namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Colony-local identity resolved only from its exact injector scope.</summary>
[Colony]
public sealed class ColonyIdentity
{
    /// <summary>Gets or sets the colony name used by graph targeting and the HUD.</summary>
    public string Name { get; set; } = "Unnamed colony";

    /// <summary>Gets or sets the glyph drawn in the colony core.</summary>
    public string Sigil { get; set; } = "*";

    /// <summary>Gets or sets the root tick when this scope was created.</summary>
    public long BornAtTick { get; set; }
}
