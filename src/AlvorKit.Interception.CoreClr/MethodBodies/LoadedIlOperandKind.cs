namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies the decoded representation of an ECMA-335 instruction operand.</summary>
public enum LoadedIlOperandKind
{
    /// <summary>The instruction has no operand.</summary>
    None,

    /// <summary>The operand is a signed integer.</summary>
    Integer,

    /// <summary>The operand is an IEEE floating-point value.</summary>
    FloatingPoint,

    /// <summary>The operand is a metadata token retained exactly from the loaded body.</summary>
    MetadataToken,

    /// <summary>The operand is an argument or local-variable index.</summary>
    VariableIndex,

    /// <summary>The operand is one absolute baseline instruction offset.</summary>
    BranchTarget,

    /// <summary>The operand is an immutable set of absolute baseline switch targets.</summary>
    SwitchTargets
}
