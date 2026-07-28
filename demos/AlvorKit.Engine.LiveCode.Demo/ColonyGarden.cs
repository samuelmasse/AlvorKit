namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Mutable visual and behavioral state intentionally exposed to exact-scope LiveCode.</summary>
[Colony]
public sealed class ColonyGarden(ColonyIdentity identity)
{
    private Vec2 anchor = (0.5f, 0.5f);

    /// <summary>Gets the identity associated with this garden.</summary>
    public ColonyIdentity Identity { get; } = identity;

    /// <summary>Gets or sets the normalized position within the observatory.</summary>
    public Vec2 Anchor
    {
        get => anchor;
        set
        {
            anchor = value;
            var offset = value - (0.5f, 0.5f);
            SolarRadius = MathF.Sqrt(
                offset.X * offset.X +
                offset.Y * offset.Y / (SolarVerticalScale * SolarVerticalScale));
            SolarAngle = Math.Atan2(
                offset.Y / SolarVerticalScale,
                offset.X);
        }
    }

    /// <summary>Gets or sets the current angle around the observatory's central sun.</summary>
    public double SolarAngle { get; set; }

    /// <summary>Gets or sets the normalized radius of this colony's solar orbit.</summary>
    public float SolarRadius { get; set; } = 0.3f;

    /// <summary>Gets the vertical compression used by the visual solar orbit.</summary>
    public const float SolarVerticalScale = 0.72f;

    /// <summary>Gets or sets the colony's primary light color.</summary>
    public Vec4 Primary { get; set; } = (0.3f, 0.8f, 1f, 1f);

    /// <summary>Gets or sets the contrasting orbit color.</summary>
    public Vec4 Secondary { get; set; } = (1f, 0.4f, 0.75f, 1f);

    /// <summary>Gets or sets the core radius.</summary>
    public float Radius { get; set; } = 54f;

    /// <summary>Gets or sets the orbit radius.</summary>
    public float OrbitRadius { get; set; } = 88f;

    /// <summary>Gets or sets how many visible organisms orbit this colony.</summary>
    public int SporeCount { get; set; } = 18;

    /// <summary>Gets or sets orbital angular speed.</summary>
    public float RotationSpeed { get; set; } = 0.7f;

    /// <summary>Gets or sets this colony's phase offset.</summary>
    public double Phase { get; set; }

    /// <summary>Gets the decaying bloom impulse.</summary>
    public float Bloom { get; private set; }

    /// <summary>Gets or sets the visual morphology label.</summary>
    public string Form { get; set; } = "radial";

    /// <summary>Creates a large, immediately visible pulse.</summary>
    public void Burst(float strength) => Bloom = Math.Max(Bloom, strength);

    /// <summary>Advances orbital motion and lets bloom energy decay.</summary>
    public void Update(double delta)
    {
        Phase += delta * RotationSpeed;
        SolarAngle += delta * RotationSpeed * 0.12;
        anchor =
        (
            0.5f + MathF.Cos((float)SolarAngle) * SolarRadius,
            0.5f + MathF.Sin((float)SolarAngle) * SolarRadius * SolarVerticalScale
        );
        Bloom = Math.Max(0f, Bloom - (float)delta * 0.42f);
    }
}
