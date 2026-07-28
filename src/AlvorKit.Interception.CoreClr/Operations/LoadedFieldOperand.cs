namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Provides exact resolved metadata needed to recognize one loaded field operand.</summary>
public sealed class LoadedFieldOperand
{
    /// <summary>The canonical exact constructed field signature.</summary>
    private readonly string canonicalSignature;

    /// <summary>Whether the field is declared static.</summary>
    private readonly bool isStatic;

    /// <summary>Whether any executable signature component remains open.</summary>
    private readonly bool containsOpenGenericParameters;

    /// <summary>The resolved declaring-type stack shape.</summary>
    private readonly LoadedTypeShape declaringTypeShape;

    /// <summary>Whether a value-type receiver is by-ref-like.</summary>
    private readonly bool isByRefLikeReceiver;

    /// <summary>Creates exact field metadata decoded by a loaded-module resolver.</summary>
    public LoadedFieldOperand(
        string canonicalSignature,
        bool isStatic,
        bool containsOpenGenericParameters,
        LoadedTypeShape declaringTypeShape,
        bool isByRefLikeReceiver)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalSignature);
        this.canonicalSignature = canonicalSignature;
        this.isStatic = isStatic;
        this.containsOpenGenericParameters = containsOpenGenericParameters;
        this.declaringTypeShape = declaringTypeShape;
        this.isByRefLikeReceiver = isByRefLikeReceiver;
    }

    /// <summary>Gets the canonical exact constructed field signature.</summary>
    public string CanonicalSignature => canonicalSignature;

    /// <summary>Gets whether the field is declared static.</summary>
    public bool IsStatic => isStatic;

    /// <summary>Gets whether any executable signature component remains open.</summary>
    public bool ContainsOpenGenericParameters =>
        containsOpenGenericParameters;

    /// <summary>Gets the resolved declaring-type stack shape.</summary>
    public LoadedTypeShape DeclaringTypeShape => declaringTypeShape;

    /// <summary>Gets whether a value-type receiver is by-ref-like.</summary>
    public bool IsByRefLikeReceiver => isByRefLikeReceiver;
}
