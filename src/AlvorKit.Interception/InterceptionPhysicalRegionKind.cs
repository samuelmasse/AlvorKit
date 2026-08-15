namespace AlvorKit;

/// <summary>Identifies whether a physical claim covers a whole method or one IL range.</summary>
public enum InterceptionPhysicalRegionKind
{
    /// <summary>The claim covers every executable region in the loaded method.</summary>
    MethodWide,

    /// <summary>The claim covers one exact range in the loaded method's baseline IL.</summary>
    IlRange
}
