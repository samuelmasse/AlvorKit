namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Emits the retained constructor prefix followed by one exact route call.</summary>
internal static class LoadedConstructorRemainderMethodBodyEmitter
{
    internal static InterceptionMethodBody Emit(
        LoadedMethodBodySnapshot body,
        LoadedConstructorRemainderPlan remainder,
        MethodInfo route,
        int argumentCount)
    {
        ReadOnlySpan<byte> baseline = body.Bytes.AsSpan();
        ReadOnlySpan<byte> code = baseline.Slice(
            body.HeaderSize,
            remainder.PreservedPrefix.Length);
        var routed = new List<byte>(
            code.Length + argumentCount * 3 + 6);
        routed.AddRange(code);
        for (var index = 0; index < argumentCount; ++index)
            EmitLoadArgument(routed, index);
        routed.Add(0x28);
        AddInt32(routed, route.MetadataToken);
        routed.Add(0x2A);

        ushort maxStack = checked((ushort)Math.Max(
            body.MaxStack,
            argumentCount));
        return InterceptionMethodBody.FromRaw(
            LoadedConstructorMethodBodyEncoding.Encode(
                routed,
                maxStack,
                body.InitLocals,
                body.LocalSignatureToken,
                remainder.PreservedExceptionRegions,
                0));
    }

    private static void EmitLoadArgument(List<byte> code, int index)
    {
        if (index <= 3)
        {
            code.Add(checked((byte)(0x02 + index)));
            return;
        }
        if (index <= byte.MaxValue)
        {
            code.Add(0x0E);
            code.Add(checked((byte)index));
            return;
        }

        code.Add(0xFE);
        code.Add(0x09);
        code.Add(checked((byte)index));
        code.Add(checked((byte)(index >> 8)));
    }

    private static void AddInt32(List<byte> bytes, int value)
    {
        bytes.Add(unchecked((byte)value));
        bytes.Add(unchecked((byte)(value >> 8)));
        bytes.Add(unchecked((byte)(value >> 16)));
        bytes.Add(unchecked((byte)(value >> 24)));
    }
}
