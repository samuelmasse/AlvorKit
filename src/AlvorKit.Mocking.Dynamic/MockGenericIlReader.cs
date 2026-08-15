namespace AlvorKit;

/// <summary>
/// Reads constructed generic method references from one capture delegate body.
/// </summary>
internal static class MockGenericIlReader
{
    private static readonly Dictionary<ushort, OpCode> opcodes = CreateOpcodes();

    /// <summary>Returns constructed generic methods referenced directly by a delegate body.</summary>
    internal static List<MethodInfo> Read(Delegate capture)
    {
        var methods = new List<MethodInfo>();
        AddIfConstructed(capture.Method, methods);
        MethodBody? body = capture.Method.GetMethodBody();
        byte[]? il = body?.GetILAsByteArray();
        if (il is null)
            return methods;

        Type[] typeArguments = capture.Method.DeclaringType?.GetGenericArguments()
            ?? Type.EmptyTypes;
        Type[] methodArguments = capture.Method.IsGenericMethod
            ? capture.Method.GetGenericArguments()
            : Type.EmptyTypes;

        for (int offset = 0; offset < il.Length;)
        {
            OpCode opcode = ReadOpcode(il, ref offset);
            if (opcode.OperandType is OperandType.InlineMethod or OperandType.InlineTok)
            {
                int token = BitConverter.ToInt32(il, offset);
                TryAddResolvedMethod(
                    capture.Method.Module,
                    token,
                    typeArguments,
                    methodArguments,
                    methods);
            }

            offset += OperandSize(opcode.OperandType, il, offset);
        }

        return methods;
    }

    private static OpCode ReadOpcode(byte[] il, ref int offset)
    {
        ushort value = il[offset++];
        if (value == 0xfe)
            value = (ushort)(0xfe00 | il[offset++]);
        return opcodes[value];
    }

    private static int OperandSize(
        OperandType operandType,
        byte[] il,
        int offset)
    {
        return operandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget
                or OperandType.ShortInlineI
                or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI
                or OperandType.InlineBrTarget
                or OperandType.InlineField
                or OperandType.InlineMethod
                or OperandType.InlineSig
                or OperandType.InlineString
                or OperandType.InlineTok
                or OperandType.InlineType
                or OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch =>
                4 + (BitConverter.ToInt32(il, offset) * 4),
            _ => throw new InvalidOperationException(
                $"Unknown IL operand type '{operandType}'.")
        };
    }

    private static void TryAddResolvedMethod(
        Module module,
        int token,
        Type[] typeArguments,
        Type[] methodArguments,
        List<MethodInfo> methods)
    {
        try
        {
            if (module.ResolveMethod(token, typeArguments, methodArguments)
                is MethodInfo method)
            {
                AddIfConstructed(method, methods);
            }
        }
        catch (ArgumentException)
        {
        }
    }

    private static void AddIfConstructed(
        MethodInfo method,
        List<MethodInfo> methods)
    {
        if (method.IsConstructedGenericMethod && !methods.Contains(method))
            methods.Add(method);
    }

    private static Dictionary<ushort, OpCode> CreateOpcodes()
    {
        var result = new Dictionary<ushort, OpCode>();
        foreach (FieldInfo field in typeof(OpCodes).GetFields(
            BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is OpCode opcode)
                result[(ushort)opcode.Value] = opcode;
        }

        return result;
    }
}
