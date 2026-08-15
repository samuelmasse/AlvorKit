namespace AlvorKit;

/// <summary>Colony-local atmosphere used by render code and live interventions.</summary>
[Colony]
public sealed class ColonySky
{
    /// <summary>Gets or sets the outer halo color.</summary>
    public Vec4 Halo { get; set; } = (0.2f, 0.5f, 1f, 0.5f);

    /// <summary>Gets or sets the amount of orbital warping.</summary>
    public float Warp { get; set; } = 0.15f;

    /// <summary>Gets or sets the atmosphere description shown in the HUD.</summary>
    public string Weather { get; set; } = "quiet signal";
}
