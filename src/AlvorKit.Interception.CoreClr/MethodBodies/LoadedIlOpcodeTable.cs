namespace AlvorKit;

/// <summary>Indexes the runtime's ECMA-335 opcode metadata by encoded opcode value.</summary>
internal static class LoadedIlOpcodeTable
{
    /// <summary>The cold-path index of every runtime-defined IL opcode.</summary>
    private static readonly Dictionary<ushort, OpCode> opCodes = Create();

    /// <summary>Looks up one single-byte or <c>0xFE</c>-prefixed opcode.</summary>
    internal static bool TryGet(ushort value, out OpCode opCode) =>
        opCodes.TryGetValue(value, out opCode);

    /// <summary>Builds the cold-path opcode index once from <see cref="OpCodes"/> constants.</summary>
    private static Dictionary<ushort, OpCode> Create()
    {
        var result = new Dictionary<ushort, OpCode>();
        foreach (var field in typeof(OpCodes).GetFields(
            BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not OpCode opCode)
                continue;

            var value = unchecked((ushort)opCode.Value);
            result[value] = opCode;
        }

        return result;
    }
}
