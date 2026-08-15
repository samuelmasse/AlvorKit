using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Emits shared partial-original completion through exact dispatch state.
/// </summary>
internal static class MockTypedFinalizerIl
{
    private static readonly MethodInfo CompleteReturnedMethod = typeof(MockDispatchContinuation).GetMethod(
        nameof(MockDispatchContinuation.CompleteReturned),
        BindingFlags.Instance | BindingFlags.NonPublic,
        [typeof(object[]), typeof(object)])!;
    private static readonly MethodInfo CompleteThrownMethod = typeof(MockDispatchContinuation).GetMethod(
        nameof(MockDispatchContinuation.CompleteThrown),
        BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo OriginalArgumentsGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.OriginalArguments),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo ProjectMethod =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.Project),
            BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo MutateStructThisMethod =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.MutateStructThis),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Emits completion of the exact token opened by a partial prefix.
    /// </summary>
    internal static void Emit(
        ILGenerator il,
        MethodInfo target,
        ParameterInfo[] parameters,
        ImmutableArray<int> carrierIndices)
    {
        bool hasResult = target.ReturnType != typeof(void);
        int parameterOffset = hasResult ? 3 : 2;
        LocalBuilder arguments = il.DeclareLocal(typeof(object[]));
        LocalBuilder result = il.DeclareLocal(typeof(object));
        Label hasState = il.DefineLabel();
        Label returned = il.DefineLabel();

        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Brtrue, hasState);
        EmitReturnException(il);

        il.MarkLabel(hasState);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Brfalse, returned);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, CompleteThrownMethod);
        EmitReturnException(il);

        il.MarkLabel(returned);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Callvirt, OriginalArgumentsGetter);
        il.Emit(OpCodes.Stloc, arguments);
        EmitExitStructMutation(
            il,
            parameters,
            parameterOffset);
        MockTypedArgumentIl.EmitRefreshReferenceArguments(
            il,
            MockIlParameter.Create(parameters),
            carrierIndices,
            parameterOffset,
            arguments);
        EmitExitProjections(
            il,
            parameters,
            parameterOffset);
        EmitBoxedResult(il, target.ReturnType, hasResult, result);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldloc, arguments);
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Callvirt, CompleteReturnedMethod);
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitExitStructMutation(
        ILGenerator il,
        ParameterInfo[] parameters,
        int parameterOffset)
    {
        if (parameters.Length == 0 ||
            !parameters[0].ParameterType.IsByRef ||
            parameters[0].ParameterType.GetElementType() is not
            { IsValueType: true, IsByRefLike: false } receiverType)
        {
            return;
        }

        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(
            OpCodes.Ldc_I4,
            (int)MockSnapshotPhase.Exit);
        il.Emit(OpCodes.Ldarg, parameterOffset);
        il.Emit(
            OpCodes.Callvirt,
            MutateStructThisMethod.MakeGenericMethod(receiverType));
        il.Emit(OpCodes.Pop);
    }

    private static void EmitExitProjections(
        ILGenerator il,
        ParameterInfo[] parameters,
        int parameterOffset)
    {
        for (int index = 0;
            index < parameters.Length;
            index++)
        {
            ParameterInfo parameter = parameters[index];
            if (!parameter.ParameterType.IsByRef ||
                parameter.IsIn)
            {
                continue;
            }

            Type valueType =
                parameter.ParameterType.GetElementType()!;
            if (valueType.IsPointer ||
                valueType.IsFunctionPointer)
            {
                continue;
            }

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(
                OpCodes.Ldc_I4,
                (int)MockSnapshotPhase.Exit);
            il.Emit(
                OpCodes.Ldarg,
                index + parameterOffset);
            il.Emit(
                OpCodes.Callvirt,
                ProjectMethod.MakeGenericMethod(valueType));
        }
    }

    private static void EmitBoxedResult(
        ILGenerator il,
        Type returnType,
        bool hasResult,
        LocalBuilder result)
    {
        if (!hasResult || returnType.IsByRef || returnType.IsByRefLike)
        {
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc, result);
            return;
        }

        Type resultParameterType = returnType.IsByRef
            ? returnType
            : returnType.MakeByRefType();
        MockTypedArgumentIl.EmitLoadBoxedArgument(
            il,
            resultParameterType,
            2);
        il.Emit(OpCodes.Stloc, result);
    }

    private static void EmitReturnException(ILGenerator il)
    {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ret);
    }
}
