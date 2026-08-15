using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Stores one immutable decoded IL operand without resolving loaded metadata.</summary>
public sealed class LoadedIlOperand
{
    /// <summary>The shared operand for instructions with no encoded operand.</summary>
    public static readonly LoadedIlOperand None = new(
        LoadedIlOperandKind.None,
        0,
        0,
        []);

    /// <summary>The operand's immutable representation tag.</summary>
    private readonly LoadedIlOperandKind kind;

    /// <summary>The integer-like value retained without boxing.</summary>
    private readonly long integerValue;

    /// <summary>The floating-point value retained without boxing.</summary>
    private readonly double floatingPointValue;

    /// <summary>The absolute baseline targets for control-flow operands.</summary>
    private readonly ImmutableArray<int> branchTargets;

    /// <summary>Creates one internally consistent operand representation.</summary>
    private LoadedIlOperand(
        LoadedIlOperandKind kind,
        long integerValue,
        double floatingPointValue,
        ImmutableArray<int> branchTargets)
    {
        this.kind = kind;
        this.integerValue = integerValue;
        this.floatingPointValue = floatingPointValue;
        this.branchTargets = branchTargets;
    }

    /// <summary>Gets the operand representation.</summary>
    public LoadedIlOperandKind Kind => kind;

    /// <summary>Gets an integer, metadata-token, or variable-index value.</summary>
    public long IntegerValue => integerValue;

    /// <summary>Gets a single- or double-precision operand represented as a <see cref="double"/>.</summary>
    public double FloatingPointValue => floatingPointValue;

    /// <summary>Gets absolute baseline targets for branch and switch operands.</summary>
    public ImmutableArray<int> BranchTargets => branchTargets;

    /// <summary>Creates a signed integer operand.</summary>
    internal static LoadedIlOperand Integer(long value) =>
        new(LoadedIlOperandKind.Integer, value, 0, []);

    /// <summary>Creates a floating-point operand.</summary>
    internal static LoadedIlOperand FloatingPoint(double value) =>
        new(LoadedIlOperandKind.FloatingPoint, 0, value, []);

    /// <summary>Creates an unresolved metadata-token operand.</summary>
    internal static LoadedIlOperand MetadataToken(int value) =>
        new(LoadedIlOperandKind.MetadataToken, value, 0, []);

    /// <summary>Creates an argument or local-variable index operand.</summary>
    internal static LoadedIlOperand VariableIndex(ushort value) =>
        new(LoadedIlOperandKind.VariableIndex, value, 0, []);

    /// <summary>Creates one absolute baseline branch target.</summary>
    internal static LoadedIlOperand BranchTarget(int value) =>
        new(
            LoadedIlOperandKind.BranchTarget,
            value,
            0,
            [value]);

    /// <summary>Creates immutable absolute baseline switch targets.</summary>
    internal static LoadedIlOperand SwitchTargets(
        ImmutableArray<int> values) =>
        new(LoadedIlOperandKind.SwitchTargets, 0, 0, values);
}
