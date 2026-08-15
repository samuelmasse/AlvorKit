namespace AlvorKit;

/// <summary>Describes one exception clause using symbolic rewritten labels.</summary>
public sealed class LoadedSymbolicExceptionRegion
{
    /// <summary>The semantic clause kind.</summary>
    private readonly LoadedExceptionRegionKind kind;

    /// <summary>The exact loaded clause flags.</summary>
    private readonly uint rawFlags;

    /// <summary>The symbolic try-region start.</summary>
    private readonly string tryStartLabel;

    /// <summary>The symbolic exclusive try-region end.</summary>
    private readonly string tryEndLabel;

    /// <summary>The symbolic handler start.</summary>
    private readonly string handlerStartLabel;

    /// <summary>The symbolic exclusive handler end.</summary>
    private readonly string handlerEndLabel;

    /// <summary>The symbolic filter start, or an empty string.</summary>
    private readonly string filterStartLabel;

    /// <summary>The existing loaded catch type token, or zero.</summary>
    private readonly int catchTypeToken;

    /// <summary>Creates one symbolically remapped exception clause.</summary>
    internal LoadedSymbolicExceptionRegion(
        LoadedExceptionRegionKind kind,
        uint rawFlags,
        string tryStartLabel,
        string tryEndLabel,
        string handlerStartLabel,
        string handlerEndLabel,
        string filterStartLabel,
        int catchTypeToken)
    {
        this.kind = kind;
        this.rawFlags = rawFlags;
        this.tryStartLabel = tryStartLabel;
        this.tryEndLabel = tryEndLabel;
        this.handlerStartLabel = handlerStartLabel;
        this.handlerEndLabel = handlerEndLabel;
        this.filterStartLabel = filterStartLabel;
        this.catchTypeToken = catchTypeToken;
    }

    /// <summary>Gets the semantic clause kind.</summary>
    public LoadedExceptionRegionKind Kind => kind;

    /// <summary>Gets the exact loaded clause flags.</summary>
    public uint RawFlags => rawFlags;

    /// <summary>Gets the symbolic try-region start.</summary>
    public string TryStartLabel => tryStartLabel;

    /// <summary>Gets the symbolic exclusive try-region end.</summary>
    public string TryEndLabel => tryEndLabel;

    /// <summary>Gets the symbolic handler start.</summary>
    public string HandlerStartLabel => handlerStartLabel;

    /// <summary>Gets the symbolic exclusive handler end.</summary>
    public string HandlerEndLabel => handlerEndLabel;

    /// <summary>Gets the symbolic filter start, or an empty string.</summary>
    public string FilterStartLabel => filterStartLabel;

    /// <summary>Gets the existing loaded catch type token, or zero.</summary>
    public int CatchTypeToken => catchTypeToken;
}
