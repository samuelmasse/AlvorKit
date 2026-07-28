namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Provides exact resolved metadata needed to recognize one loaded method operand.</summary>
public sealed class LoadedMethodOperand
{
    /// <summary>The canonical exact constructed method signature.</summary>
    private readonly string canonicalSignature;

    /// <summary>Whether the signature consumes an instance receiver.</summary>
    private readonly bool hasThis;

    /// <summary>Whether the operand resolves to an instance constructor.</summary>
    private readonly bool isConstructor;

    /// <summary>Whether the calling convention accepts variable arguments.</summary>
    private readonly bool isVariableArguments;

    /// <summary>Whether any executable signature component remains open.</summary>
    private readonly bool containsOpenGenericParameters;

    /// <summary>The resolved declaring-type stack shape.</summary>
    private readonly LoadedTypeShape declaringTypeShape;

    /// <summary>Whether a value-type receiver is by-ref-like.</summary>
    private readonly bool isByRefLikeReceiver;

    /// <summary>The number of declared parameters consumed by a direct call.</summary>
    private readonly int parameterCount;

    /// <summary>Whether an ordinary call pushes one result.</summary>
    private readonly bool returnsValue;

    /// <summary>Creates exact method metadata decoded by a loaded-module resolver.</summary>
    public LoadedMethodOperand(
        string canonicalSignature,
        bool hasThis,
        bool isConstructor,
        bool isVariableArguments,
        bool containsOpenGenericParameters,
        LoadedTypeShape declaringTypeShape,
        bool isByRefLikeReceiver,
        int parameterCount = 0,
        bool returnsValue = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterCount);
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalSignature);
        this.canonicalSignature = canonicalSignature;
        this.hasThis = hasThis;
        this.isConstructor = isConstructor;
        this.isVariableArguments = isVariableArguments;
        this.containsOpenGenericParameters = containsOpenGenericParameters;
        this.declaringTypeShape = declaringTypeShape;
        this.isByRefLikeReceiver = isByRefLikeReceiver;
        this.parameterCount = parameterCount;
        this.returnsValue = returnsValue;
    }

    /// <summary>Gets the canonical exact constructed method signature.</summary>
    public string CanonicalSignature => canonicalSignature;

    /// <summary>Gets whether the signature consumes an instance receiver.</summary>
    public bool HasThis => hasThis;

    /// <summary>Gets whether the operand resolves to an instance constructor.</summary>
    public bool IsConstructor => isConstructor;

    /// <summary>Gets whether the calling convention accepts variable arguments.</summary>
    public bool IsVariableArguments => isVariableArguments;

    /// <summary>Gets whether any executable signature component remains open.</summary>
    public bool ContainsOpenGenericParameters =>
        containsOpenGenericParameters;

    /// <summary>Gets the resolved declaring-type stack shape.</summary>
    public LoadedTypeShape DeclaringTypeShape => declaringTypeShape;

    /// <summary>Gets whether a value-type receiver is by-ref-like.</summary>
    public bool IsByRefLikeReceiver => isByRefLikeReceiver;

    /// <summary>Gets the number of declared parameters consumed by a direct call.</summary>
    public int ParameterCount => parameterCount;

    /// <summary>Gets whether an ordinary call pushes one result.</summary>
    public bool ReturnsValue => returnsValue;
}
