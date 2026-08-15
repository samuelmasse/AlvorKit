namespace AlvorKit;

/// <summary>Owns the vertical offset of one reusable Blend scroll view.</summary>
public sealed class BlendScrollHandle
{
    /// <summary>Gets the current vertical content offset.</summary>
    public float Offset { get; internal set; }

    /// <summary>Returns the view to its beginning.</summary>
    public void Reset() =>
        Offset = 0f;

    /// <summary>Moves only as far as needed to reveal a vertical interval inside the viewport.</summary>
    public void EnsureVisible(
        float minimum,
        float maximum,
        float viewportHeight)
    {
        if (minimum < Offset)
            Offset = minimum;
        else if (maximum
                 > Offset + viewportHeight)
            Offset = maximum - viewportHeight;
    }
}
