namespace AlvorKit;

/// <summary>Describes one instruction at its immutable loaded-baseline coordinate.</summary>
public sealed class LoadedIlInstruction
{
    /// <summary>The encoded value of the accepted <c>volatile.</c> prefix.</summary>
    private const ushort VolatilePrefix = 0xFE13;

    /// <summary>The encoded value of the accepted <c>constrained.</c> prefix.</summary>
    private const ushort ConstrainedPrefix = 0xFE16;

    /// <summary>The immutable original offset relative to the loaded IL stream.</summary>
    private readonly int baselineOffset;

    /// <summary>The complete encoded instruction size.</summary>
    private readonly int size;

    /// <summary>The runtime opcode metadata for the encoded value.</summary>
    private readonly OpCode opCode;

    /// <summary>The decoded operand retained without metadata resolution.</summary>
    private readonly LoadedIlOperand operand;

    /// <summary>Creates one decoded baseline instruction.</summary>
    internal LoadedIlInstruction(
        int baselineOffset,
        int size,
        OpCode opCode,
        LoadedIlOperand operand)
    {
        this.baselineOffset = baselineOffset;
        this.size = size;
        this.opCode = opCode;
        this.operand = operand;
    }

    /// <summary>Gets the instruction offset relative to the first code byte of the loaded body.</summary>
    public int BaselineOffset => baselineOffset;

    /// <summary>Gets the complete encoded instruction size, including its opcode and operand.</summary>
    public int Size => size;

    /// <summary>Gets the offset immediately following this baseline instruction.</summary>
    public int NextBaselineOffset => (baselineOffset + size);

    /// <summary>Gets the ECMA-335 opcode metadata.</summary>
    public OpCode OpCode => opCode;

    /// <summary>Gets the opcode's stable unsigned encoded value.</summary>
    public ushort OpCodeValue => unchecked((ushort)opCode.Value);

    /// <summary>Gets the decoded, unresolved operand.</summary>
    public LoadedIlOperand Operand => operand;

    /// <summary>Gets whether the opcode is an IL prefix.</summary>
    public bool IsPrefix => opCode.OpCodeType == OpCodeType.Prefix;

    /// <summary>
    /// Gets whether the prefix is currently accepted for exact original-operation replay.
    /// </summary>
    public bool IsAcceptedPrefix =>
        OpCodeValue is VolatilePrefix or ConstrainedPrefix;
}
