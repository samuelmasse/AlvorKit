namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies the semantic kind of one loaded exception-handling clause.</summary>
public enum LoadedExceptionRegionKind
{
    /// <summary>A typed catch handler.</summary>
    Catch,

    /// <summary>A filter followed by its handler.</summary>
    Filter,

    /// <summary>A finally handler.</summary>
    Finally,

    /// <summary>A fault handler.</summary>
    Fault
}

/// <summary>Identifies the small or fat ECMA-335 section encoding of a clause.</summary>
public enum LoadedExceptionRegionFormat
{
    /// <summary>The twelve-byte small clause encoding.</summary>
    Small,

    /// <summary>The twenty-four-byte fat clause encoding.</summary>
    Fat
}

/// <summary>Describes one immutable exception region in loaded-baseline offsets.</summary>
public sealed class LoadedExceptionRegion
{
    /// <summary>The semantic handler kind.</summary>
    private readonly LoadedExceptionRegionKind kind;

    /// <summary>The source clause's small or fat encoding.</summary>
    private readonly LoadedExceptionRegionFormat format;

    /// <summary>The exact loaded clause flags.</summary>
    private readonly uint rawFlags;

    /// <summary>The baseline try-region start.</summary>
    private readonly int tryOffset;

    /// <summary>The baseline try-region length.</summary>
    private readonly int tryLength;

    /// <summary>The baseline handler start.</summary>
    private readonly int handlerOffset;

    /// <summary>The baseline handler length.</summary>
    private readonly int handlerLength;

    /// <summary>The unresolved catch token, or zero.</summary>
    private readonly int catchTypeToken;

    /// <summary>The filter start, or minus one.</summary>
    private readonly int filterOffset;

    /// <summary>Creates a validated loaded exception region.</summary>
    internal LoadedExceptionRegion(
        LoadedExceptionRegionKind kind,
        LoadedExceptionRegionFormat format,
        uint rawFlags,
        int tryOffset,
        int tryLength,
        int handlerOffset,
        int handlerLength,
        int catchTypeToken,
        int filterOffset)
    {
        this.kind = kind;
        this.format = format;
        this.rawFlags = rawFlags;
        this.tryOffset = tryOffset;
        this.tryLength = tryLength;
        this.handlerOffset = handlerOffset;
        this.handlerLength = handlerLength;
        this.catchTypeToken = catchTypeToken;
        this.filterOffset = filterOffset;
    }

    /// <summary>Gets the semantic clause kind.</summary>
    public LoadedExceptionRegionKind Kind => kind;

    /// <summary>Gets the source section's clause encoding.</summary>
    public LoadedExceptionRegionFormat Format => format;

    /// <summary>Gets the exact clause flags, including the duplicated bit.</summary>
    public uint RawFlags => rawFlags;

    /// <summary>Gets whether CoreCLR marked the clause as duplicated.</summary>
    public bool IsDuplicated => (rawFlags & 0x08) != 0;

    /// <summary>Gets the try-region start in baseline code bytes.</summary>
    public int TryOffset => tryOffset;

    /// <summary>Gets the try-region length in code bytes.</summary>
    public int TryLength => tryLength;

    /// <summary>Gets the handler start in baseline code bytes.</summary>
    public int HandlerOffset => handlerOffset;

    /// <summary>Gets the handler length in code bytes.</summary>
    public int HandlerLength => handlerLength;

    /// <summary>Gets the catch type token, or zero for a non-catch clause.</summary>
    public int CatchTypeToken => catchTypeToken;

    /// <summary>Gets the filter start, or minus one for a non-filter clause.</summary>
    public int FilterOffset => filterOffset;
}
