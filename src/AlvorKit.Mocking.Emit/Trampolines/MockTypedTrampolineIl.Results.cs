using System.Collections.Immutable;

namespace AlvorKit.Mocking;

/// <summary>Emits shared exact typed result and writeback paths.</summary>
internal static partial class MockTypedTrampolineIl
{
    /// <summary>Returns whether the public zero-argument factory can represent this return type.</summary>
    private static bool CanUseTypedReturnFactory(Type returnType) =>
        returnType != typeof(void)
        && !returnType.IsByRef
        && !returnType.IsPointer
        && !returnType.IsFunctionPointer;

    /// <summary>Returns whether an exact callback may write this result through the ordinary result slot.</summary>
    private static bool CanUseTypedCallbackReturn(Type returnType) =>
        !returnType.IsByRef
        && !returnType.IsPointer
        && !returnType.IsFunctionPointer;

    private static void EmitWritebacks(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        ImmutableArray<int> carrierIndices,
        int parameterOffset,
        LocalBuilder arguments)
    {
        for (int index = 0; index < parameters.Count; index++)
        {
            MockIlParameter parameter = parameters[index];
            if (!parameter.Type.IsByRef || (parameter.IsIn && !parameter.IsOut))
                continue;

            Type valueType = parameter.Type.GetElementType()!;
            int argumentIndex = index + parameterOffset;
            Label writeback = il.DefineLabel();
            Label skipWriteback = il.DefineLabel();
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Call, MockTypedArgumentIl.ShouldSkipArgumentMethod);
            il.Emit(OpCodes.Brfalse, writeback);
            il.Emit(OpCodes.Br, skipWriteback);
            il.MarkLabel(writeback);
            if (MockTypeShape.MayBeByRefLike(valueType))
            {
                if (parameter.IsOut)
                {
                    il.Emit(OpCodes.Ldarg, argumentIndex);
                    il.Emit(OpCodes.Initobj, valueType);
                }

                il.MarkLabel(skipWriteback);
                continue;
            }

            EmitObjectWriteback(
                il,
                valueType,
                argumentIndex,
                arguments,
                carrierIndices[index]);
            il.MarkLabel(skipWriteback);
        }
    }

    private static void EmitObjectWriteback(
        ILGenerator il,
        Type valueType,
        int argumentIndex,
        LocalBuilder arguments,
        int carrierIndex)
    {
        Label hasValue = il.DefineLabel();
        Label complete = il.DefineLabel();

        il.Emit(OpCodes.Ldloc, arguments);
        il.Emit(OpCodes.Ldc_I4, carrierIndex);
        il.Emit(OpCodes.Ldelem_Ref);
        il.Emit(OpCodes.Brtrue, hasValue);
        il.Emit(OpCodes.Ldarg, argumentIndex);
        il.Emit(OpCodes.Initobj, valueType);
        il.Emit(OpCodes.Br, complete);

        il.MarkLabel(hasValue);
        il.Emit(OpCodes.Ldarg, argumentIndex);
        il.Emit(OpCodes.Ldloc, arguments);
        il.Emit(OpCodes.Ldc_I4, carrierIndex);
        il.Emit(OpCodes.Ldelem_Ref);
        EmitStoreObjectValue(il, valueType);
        il.MarkLabel(complete);
    }

    private static void EmitResult(
        ILGenerator il,
        Type returnType,
        LocalBuilder result)
    {
        Type valueType = GetValueType(returnType);
        if (MockTypeShape.MayBeByRefLike(valueType))
        {
            EmitInitializeResult(il, returnType);
            return;
        }

        Label hasValue = il.DefineLabel();
        Label complete = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Brtrue, hasValue);
        EmitInitializeResult(il, returnType);
        il.Emit(OpCodes.Br, complete);

        il.MarkLabel(hasValue);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldloc, result);
        EmitStoreObjectValue(il, valueType);
        il.MarkLabel(complete);
    }

    private static void EmitStoreObjectValue(ILGenerator il, Type valueType)
    {
        if (valueType.IsPointer || valueType.IsFunctionPointer)
        {
            il.Emit(OpCodes.Unbox_Any, typeof(nint));
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Stind_I);
        }
        else if (valueType.IsValueType || valueType.IsGenericParameter)
        {
            il.Emit(OpCodes.Unbox_Any, valueType);
            il.Emit(OpCodes.Stobj, valueType);
        }
        else
        {
            il.Emit(OpCodes.Castclass, valueType);
            il.Emit(OpCodes.Stind_Ref);
        }
    }
}
