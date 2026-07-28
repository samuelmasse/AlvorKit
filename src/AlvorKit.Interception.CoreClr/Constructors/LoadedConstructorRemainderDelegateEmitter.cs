using System.Buffers.Binary;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Extracts one relocated post-initializer constructor suffix as a dynamic delegate.</summary>
internal static class LoadedConstructorRemainderDelegateEmitter
{
    internal static Delegate Emit(
        ConstructorInfo constructor,
        LoadedMethodBodySnapshot body,
        LoadedConstructorRemainderPlan remainder,
        Type delegateType,
        Type[] signature)
    {
        var method = new DynamicMethod(
            $"{constructor.DeclaringType!.Name}_OriginalConstructorRemainder",
            typeof(void),
            signature,
            constructor.Module,
            true);
        DynamicILInfo info = method.GetDynamicILInfo();
        method.InitLocals = body.InitLocals;
        byte[] code = body.Bytes
            .AsSpan(
                body.HeaderSize + remainder.MovedRemainder.StartOffset,
                remainder.MovedRemainder.Length)
            .ToArray();
        RelocateMetadata(
            constructor,
            remainder,
            info,
            code);
        if (body.LocalSignatureToken != 0)
        {
            byte[] localSignature = constructor.Module.ResolveSignature(
                body.LocalSignatureToken);
            LoadedDynamicLocalSignatureValidator.Validate(localSignature);
            info.SetLocalSignature(localSignature);
        }
        else
        {
            info.SetLocalSignature([0x07, 0x00]);
        }
        if (!remainder.MovedExceptionRegions.IsEmpty)
        {
            info.SetExceptions(
                LoadedConstructorMethodBodyEncoding.EncodeExceptionSection(
                    remainder.MovedExceptionRegions,
                    remainder.MovedRemainder.StartOffset,
                    token => ResolveTypeToken(
                        constructor,
                        info,
                        token)));
        }
        info.SetCode(code, body.MaxStack);
        return method.CreateDelegate(delegateType);
    }

    private static void RelocateMetadata(
        ConstructorInfo constructor,
        LoadedConstructorRemainderPlan remainder,
        DynamicILInfo info,
        Span<byte> code)
    {
        foreach (LoadedIlInstruction instruction in
            remainder.MovedRemainder.Instructions)
        {
            if (instruction.Operand.Kind !=
                LoadedIlOperandKind.MetadataToken)
            {
                continue;
            }

            var sourceToken = checked(
                (int)instruction.Operand.IntegerValue);
            var dynamicToken = ResolveToken(
                constructor,
                info,
                instruction.OpCode.OperandType,
                sourceToken);
            var operandOffset = checked(
                instruction.BaselineOffset -
                remainder.MovedRemainder.StartOffset +
                instruction.OpCode.Size);
            BinaryPrimitives.WriteInt32LittleEndian(
                code[operandOffset..],
                dynamicToken);
        }
    }

    private static int ResolveToken(
        ConstructorInfo constructor,
        DynamicILInfo info,
        OperandType operandType,
        int token)
    {
        Module module = constructor.Module;
        Type[]? typeArguments =
            constructor.DeclaringType!.IsGenericType
                ? constructor.DeclaringType.GetGenericArguments()
                : null;
        if (operandType == OperandType.InlineString)
            return info.GetTokenFor(module.ResolveString(token));
        if (operandType == OperandType.InlineSig)
        {
            throw new NotSupportedException(
                "InlineSig operands in constructor remainders are unsupported " +
                "until exact dynamic-scope signature relocation is implemented.");
        }

        MemberInfo member = module.ResolveMember(
            token,
            typeArguments,
            null) ??
            throw new InvalidOperationException(
                $"Metadata token 0x{token:X8} did not resolve.");
        return member switch
        {
            Type type => info.GetTokenFor(type.TypeHandle),
            MethodInfo method => MethodToken(info, method),
            ConstructorInfo initializer =>
                ConstructorToken(info, initializer),
            FieldInfo field => FieldToken(info, field),
            _ => throw new NotSupportedException(
                $"Metadata operand '{member}' is not supported in a " +
                "constructor remainder.")
        };
    }

    private static int ResolveTypeToken(
        ConstructorInfo constructor,
        DynamicILInfo info,
        int token)
    {
        Type type = constructor.Module.ResolveType(
            token,
            constructor.DeclaringType!.IsGenericType
                ? constructor.DeclaringType.GetGenericArguments()
                : null,
            null);
        return info.GetTokenFor(type.TypeHandle);
    }

    private static int MethodToken(
        DynamicILInfo info,
        MethodInfo method) =>
        method.DeclaringType?.IsGenericType == true
            ? info.GetTokenFor(
                method.MethodHandle,
                method.DeclaringType.TypeHandle)
            : info.GetTokenFor(method.MethodHandle);

    private static int ConstructorToken(
        DynamicILInfo info,
        ConstructorInfo constructor) =>
        constructor.DeclaringType?.IsGenericType == true
            ? info.GetTokenFor(
                constructor.MethodHandle,
                constructor.DeclaringType.TypeHandle)
            : info.GetTokenFor(constructor.MethodHandle);

    private static int FieldToken(
        DynamicILInfo info,
        FieldInfo field) =>
        field.DeclaringType?.IsGenericType == true
            ? info.GetTokenFor(
                field.FieldHandle,
                field.DeclaringType.TypeHandle)
            : info.GetTokenFor(field.FieldHandle);
}
