namespace AlvorKit.Engine;

/// <summary>Root UI scale derived from monitor scale and adjustable by integer numerator and denominator.</summary>
[Root]
public class RootScale(RootScreen screen)
{
    private int numerator = (int)Math.Round(screen.MonitorScale * 4);
    private int denominator = 4;

    /// <summary>Gets a mutable numerator used to compute <see cref="Scale"/>.</summary>
    public ref int Numerator => ref numerator;

    /// <summary>Gets a mutable denominator used to compute <see cref="Scale"/>.</summary>
    public ref int Denominator => ref denominator;

    /// <summary>Gets the current UI scale multiplier.</summary>
    public float Scale => numerator / (float)denominator;

    /// <summary>Scales an integer value by the current multiplier.</summary>
    public int this[int value] => (int)(value * Scale);
}
