using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Emits shared declared-argument normalization into the heap-safe control-plane carrier.
/// </summary>
internal static class MockTypedArgumentIl
{
    internal static readonly MethodInfo ShouldSkipArgumentMethod =
        typeof(MockTypedCaptureRuntime).GetMethod(
            nameof(MockTypedCaptureRuntime.ShouldSkipArgument),
            BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo EmptyArgumentsMethod =
        typeof(Array).GetMethod(nameof(Array.Empty))!
            .MakeGenericMethod(typeof(object));

    /// <summary>
    /// Emits one carrier allocation and boxes only ordinary retained argument forms.
    /// </summary>
    internal static void EmitArguments(
        ILGenerator il,
        ParameterInfo[] parameters,
        ImmutableArray<int> carrierIndices,
        int parameterOffset,
        LocalBuilder arguments,
        bool readOutValues = false) =>
        EmitArguments(
            il,
            MockIlParameter.Create(parameters),
            carrierIndices,
            parameterOffset,
            arguments,
            readOutValues);

    /// <summary>
    /// Emits one carrier allocation from already substituted emitted parameter shapes.
    /// </summary>
    internal static void EmitArguments(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        ImmutableArray<int> carrierIndices,
        int parameterOffset,
        LocalBuilder arguments,
        bool readOutValues = false)
    {
        if (parameters.Count == 0)
        {
            il.Emit(OpCodes.Call, EmptyArgumentsMethod);
            il.Emit(OpCodes.Stloc, arguments);
            return;
        }

        il.Emit(OpCodes.Ldc_I4, parameters.Count);
        il.Emit(OpCodes.Newarr, typeof(object));
        il.Emit(OpCodes.Stloc, arguments);

        for (int index = 0; index < parameters.Count; index++)
        {
            MockIlParameter parameter = parameters[index];
            Type valueType = GetValueType(parameter.Type);
            il.Emit(OpCodes.Ldloc, arguments);
            il.Emit(OpCodes.Ldc_I4, carrierIndices[index]);

            if ((!readOutValues && parameter.IsOut)
                || MockTypeShape.MayBeByRefLike(valueType))
                il.Emit(OpCodes.Ldnull);
            else if (parameter.Type.IsByRef)
                EmitLoadBoxedReferenceArgument(
                    il,
                    parameter.Type,
                    index,
                    index + parameterOffset);
            else
                EmitLoadBoxedArgument(il, parameter.Type, index + parameterOffset);

            il.Emit(OpCodes.Stelem_Ref);
        }
    }

    /// <summary>Refreshes only caller-visible ordinary ref/out slots in an existing carrier.</summary>
    internal static void EmitRefreshReferenceArguments(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        ImmutableArray<int> carrierIndices,
        int parameterOffset,
        LocalBuilder arguments)
    {
        for (var index = 0; index < parameters.Count; index++)
        {
            MockIlParameter parameter = parameters[index];
            if (!parameter.Type.IsByRef
                || (parameter.IsIn && !parameter.IsOut))
            {
                continue;
            }

            Type valueType = parameter.Type.GetElementType()!;
            if (MockTypeShape.MayBeByRefLike(valueType))
                continue;

            il.Emit(OpCodes.Ldloc, arguments);
            il.Emit(OpCodes.Ldc_I4, carrierIndices[index]);
            EmitLoadBoxedArgument(
                il,
                parameter.Type,
                index + parameterOffset);
            il.Emit(OpCodes.Stelem_Ref);
        }
    }

    private static void EmitLoadBoxedReferenceArgument(
        ILGenerator il,
        Type parameterType,
        int declaredIndex,
        int argumentIndex)
    {
        Label load = il.DefineLabel();
        Label complete = il.DefineLabel();
        il.Emit(OpCodes.Ldc_I4, declaredIndex);
        il.Emit(OpCodes.Call, ShouldSkipArgumentMethod);
        il.Emit(OpCodes.Brfalse, load);
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Br, complete);
        il.MarkLabel(load);
        EmitLoadBoxedArgument(il, parameterType, argumentIndex);
        il.MarkLabel(complete);
    }

    internal static void EmitLoadBoxedArgument(
        ILGenerator il,
        Type parameterType,
        int argumentIndex)
    {
        Type valueType = GetValueType(parameterType);
        il.Emit(OpCodes.Ldarg, argumentIndex);

        if (parameterType.IsByRef)
            EmitLoadIndirect(il, valueType);

        if (valueType.IsPointer || valueType.IsFunctionPointer)
        {
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Box, typeof(nint));
        }
        else if (valueType.IsValueType || valueType.IsGenericParameter)
        {
            il.Emit(OpCodes.Box, valueType);
        }
    }

    private static void EmitLoadIndirect(ILGenerator il, Type valueType)
    {
        if (valueType.IsPointer || valueType.IsFunctionPointer)
            il.Emit(OpCodes.Ldind_I);
        else if (valueType.IsValueType)
            il.Emit(OpCodes.Ldobj, valueType);
        else
            il.Emit(OpCodes.Ldind_Ref);
    }

    private static Type GetValueType(Type type) =>
        type.IsByRef ? type.GetElementType()! : type;
}
