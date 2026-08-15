using System.Collections.Immutable;
using static AlvorKit.LoadedIlPrimitiveReader;

namespace AlvorKit;

/// <summary>Decodes and validates instructions from an authoritative loaded IL stream.</summary>
internal static class LoadedIlInstructionDecoder
{
    /// <summary>Decodes every instruction and validates absolute branch boundaries.</summary>
    internal static ImmutableArray<LoadedIlInstruction> Decode(
        ReadOnlySpan<byte> code)
    {
        var instructions = ImmutableArray.CreateBuilder<LoadedIlInstruction>();
        var offset = 0;
        while (offset < code.Length)
            instructions.Add(DecodeOne(code, ref offset));

        var result = instructions.ToImmutable();
        ValidateBranchTargets(result, code.Length);
        return result;
    }

    /// <summary>Decodes one opcode and its complete operand.</summary>
    private static LoadedIlInstruction DecodeOne(
        ReadOnlySpan<byte> code,
        ref int offset)
    {
        var start = offset;
        var opCode = ReadOpCode(code, ref offset, start);
        var operand = ReadOperand(code, ref offset, start, opCode.OperandType);
        return new(start, offset - start, opCode, operand);
    }

    /// <summary>Reads a single-byte or <c>0xFE</c>-prefixed opcode.</summary>
    private static OpCode ReadOpCode(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset)
    {
        Require(code, offset, 1, instructionOffset, "opcode");
        ushort value = code[offset++];
        if (value == 0xFE)
        {
            Require(code, offset, 1, instructionOffset, "two-byte opcode");
            value = (ushort)(0xFE00 | code[offset++]);
        }

        if (!LoadedIlOpcodeTable.TryGet(value, out var opCode))
        {
            throw Malformed(
                instructionOffset,
                $"opcode 0x{value:X4} is not defined");
        }

        return opCode;
    }

    /// <summary>Reads an operand according to its ECMA-335 opcode metadata.</summary>
    private static LoadedIlOperand ReadOperand(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset,
        OperandType type) =>
        type switch
        {
            OperandType.InlineNone => LoadedIlOperand.None,
            OperandType.ShortInlineI => LoadedIlOperand.Integer(
                ReadSByte(code, ref offset, instructionOffset)),
            OperandType.InlineI => LoadedIlOperand.Integer(
                ReadInt32(code, ref offset, instructionOffset)),
            OperandType.InlineI8 => LoadedIlOperand.Integer(
                ReadInt64(code, ref offset, instructionOffset)),
            OperandType.ShortInlineR => LoadedIlOperand.FloatingPoint(
                ReadSingle(code, ref offset, instructionOffset)),
            OperandType.InlineR => LoadedIlOperand.FloatingPoint(
                ReadDouble(code, ref offset, instructionOffset)),
            OperandType.ShortInlineVar => LoadedIlOperand.VariableIndex(
                ReadByte(code, ref offset, instructionOffset)),
            OperandType.InlineVar => LoadedIlOperand.VariableIndex(
                ReadUInt16(code, ref offset, instructionOffset)),
            OperandType.ShortInlineBrTarget => ReadBranch(
                code,
                ref offset,
                instructionOffset,
                true),
            OperandType.InlineBrTarget => ReadBranch(
                code,
                ref offset,
                instructionOffset,
                false),
            OperandType.InlineSwitch => ReadSwitch(
                code,
                ref offset,
                instructionOffset),
            OperandType.InlineField or
            OperandType.InlineMethod or
            OperandType.InlineSig or
            OperandType.InlineString or
            OperandType.InlineTok or
            OperandType.InlineType => LoadedIlOperand.MetadataToken(
                ReadInt32(code, ref offset, instructionOffset)),
            _ => throw Malformed(
                instructionOffset,
                $"operand type {type} is not supported")
        };

    /// <summary>Reads one relative branch and converts it to a baseline offset.</summary>
    private static LoadedIlOperand ReadBranch(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset,
        bool shortForm)
    {
        var delta = shortForm
            ? ReadSByte(code, ref offset, instructionOffset)
            : ReadInt32(code, ref offset, instructionOffset);
        return LoadedIlOperand.BranchTarget(
            ToTarget(offset, delta, instructionOffset));
    }

    /// <summary>Reads relative switch deltas and converts them to baseline offsets.</summary>
    private static LoadedIlOperand ReadSwitch(
        ReadOnlySpan<byte> code,
        ref int offset,
        int instructionOffset)
    {
        var count = ReadInt32(code, ref offset, instructionOffset);
        if (count < 0 || count > (code.Length - offset) / sizeof(int))
            throw Malformed(instructionOffset, "switch table does not fit");

        var targetBase = (offset + (count * sizeof(int)));
        var targets = ImmutableArray.CreateBuilder<int>(count);
        for (var index = 0; index < count; ++index)
        {
            var delta = ReadInt32(code, ref offset, instructionOffset);
            targets.Add(ToTarget(targetBase, delta, instructionOffset));
        }

        return LoadedIlOperand.SwitchTargets(targets.MoveToImmutable());
    }

    /// <summary>Converts a signed relative displacement without integer wraparound.</summary>
    private static int ToTarget(int origin, long delta, int instructionOffset)
    {
        var target = origin + delta;
        if (target < int.MinValue || target > int.MaxValue)
            throw Malformed(instructionOffset, "branch target overflows");
        return (int)target;
    }

    /// <summary>Validates that every control-flow target begins a decoded instruction.</summary>
    private static void ValidateBranchTargets(
        ImmutableArray<LoadedIlInstruction> instructions,
        int codeSize)
    {
        var boundaries = new bool[codeSize];
        foreach (var instruction in instructions)
            boundaries[instruction.BaselineOffset] = true;

        foreach (var instruction in instructions)
        {
            foreach (var target in instruction.Operand.BranchTargets)
            {
                if ((uint)target >= (uint)boundaries.Length ||
                    !boundaries[target])
                {
                    throw Malformed(
                        instruction.BaselineOffset,
                        $"branch target IL_{target:X4} is not an instruction boundary");
                }
            }
        }
    }

    /// <summary>Requires a complete operand segment within the IL stream.</summary>
    private static void Require(
        ReadOnlySpan<byte> code,
        int offset,
        int size,
        int instructionOffset,
        string description)
    {
        if (offset < 0 || size < 0 || offset > code.Length - size)
            throw Malformed(instructionOffset, $"{description} is truncated");
    }

    /// <summary>Creates a coordinate-rich malformed-IL exception.</summary>
    private static InvalidDataException Malformed(
        int instructionOffset,
        string message) =>
        new($"Malformed loaded IL at IL_{instructionOffset:X4}: {message}.");
}
