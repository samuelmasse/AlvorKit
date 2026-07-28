namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Provides exact resolved metadata for a constrained-prefix type operand.</summary>
public sealed class LoadedTypeOperand
{
    /// <summary>The canonical exact constructed type signature.</summary>
    private readonly string canonicalSignature;

    /// <summary>The resolved runtime stack shape.</summary>
    private readonly LoadedTypeShape shape;

    /// <summary>Whether the constrained value type is by-ref-like.</summary>
    private readonly bool isByRefLike;

    /// <summary>Creates exact type metadata decoded by a loaded-module resolver.</summary>
    public LoadedTypeOperand(
        string canonicalSignature,
        LoadedTypeShape shape,
        bool isByRefLike)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalSignature);
        this.canonicalSignature = canonicalSignature;
        this.shape = shape;
        this.isByRefLike = isByRefLike;
    }

    /// <summary>Gets the canonical exact constructed type signature.</summary>
    public string CanonicalSignature => canonicalSignature;

    /// <summary>Gets the resolved runtime stack shape.</summary>
    public LoadedTypeShape Shape => shape;

    /// <summary>Gets whether the constrained value type is by-ref-like.</summary>
    public bool IsByRefLike => isByRefLike;
}
