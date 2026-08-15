using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Emits shared exact typed return-factory paths.</summary>
internal static partial class MockTypedTrampolineIl
{
    private static readonly MethodInfo IsTypedReturnFactoryGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.IsTypedReturnFactory),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo TypedReturnFactoryGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.TypedReturnFactory),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo CompleteTypedReturnedMethod =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.CompleteTypedReturned),
            BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo CompleteTypedUnretainedReturnedMethod =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.CompleteTypedUnretainedReturned),
            BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo CompleteTypedThrownMethod =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.CompleteTypedThrown),
            BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo FactoryInvokeDefinition =
        typeof(Func<>).GetMethod(nameof(Func<>.Invoke))!;

    /// <summary>Emits direct exact factory invocation and one-token completion.</summary>
    private static void EmitTypedReturnFactory(
        ILGenerator il,
        Type returnType,
        int stateArgumentIndex,
        LocalBuilder arguments,
        IReadOnlyList<MockIlParameter> parameters,
        ImmutableArray<int> carrierIndices,
        int parameterOffset,
        LocalBuilder matcherEvaluation)
    {
        Type factoryType = typeof(Func<>).MakeGenericType(returnType);
        MethodInfo invoke = returnType.ContainsGenericParameters
            ? TypeBuilder.GetMethod(factoryType, FactoryInvokeDefinition)
            : factoryType.GetMethod(nameof(Func<>.Invoke))!;
        LocalBuilder exception = il.DeclareLocal(typeof(Exception));
        LocalBuilder continuation = il.DeclareLocal(
            typeof(MockDispatchContinuation));
        Label completed = il.DefineLabel();

        EmitLoadState(il, stateArgumentIndex);
        il.Emit(OpCodes.Stloc, continuation);
        il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Callvirt, TypedReturnFactoryGetter);
        il.Emit(OpCodes.Castclass, factoryType);
        il.Emit(OpCodes.Callvirt, invoke);
        EmitStoreResult(il, returnType);
        il.Emit(OpCodes.Leave, completed);

        il.BeginCatchBlock(typeof(Exception));
        il.Emit(OpCodes.Stloc, exception);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Ldloc, exception);
        il.Emit(OpCodes.Callvirt, CompleteTypedThrownMethod);
        EmitClearState(il, stateArgumentIndex);
        il.Emit(OpCodes.Rethrow);
        il.EndExceptionBlock();

        il.MarkLabel(completed);
        EmitWritebacks(
            il,
            parameters,
            carrierIndices,
            parameterOffset,
            arguments);
        MockTypedMutationIl.Emit(
            il,
            parameters,
            carrierIndices,
            parameterOffset,
            arguments,
            matcherEvaluation,
            MockSnapshotPhase.Exit);
        EmitClearState(il, stateArgumentIndex);
        MockTypedProjectionIl.EmitExit(
            il,
            parameters,
            parameterOffset,
            matcherEvaluation);
        EmitCompleteReturned(
            il,
            returnType,
            continuation,
            arguments);
        EmitBooleanReturn(il, false);
    }

    private static void EmitCompleteReturned(
        ILGenerator il,
        Type returnType,
        LocalBuilder continuation,
        LocalBuilder arguments)
    {
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Ldloc, arguments);
        if (returnType == typeof(void))
        {
            il.Emit(OpCodes.Ldnull);
            il.Emit(
                OpCodes.Callvirt,
                CompleteTypedReturnedMethod);
            il.Emit(OpCodes.Pop);
            return;
        }

        if (MockTypeShape.MayBeByRefLike(returnType))
        {
            il.Emit(
                OpCodes.Callvirt,
                CompleteTypedUnretainedReturnedMethod);
            return;
        }

        MockTypedArgumentIl.EmitLoadBoxedArgument(
            il,
            returnType.MakeByRefType(),
            2);
        il.Emit(OpCodes.Callvirt, CompleteTypedReturnedMethod);
        LocalBuilder normalizedResult =
            il.DeclareLocal(typeof(object));
        il.Emit(OpCodes.Stloc, normalizedResult);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldloc, normalizedResult);
        EmitStoreObjectValue(il, returnType);
    }

    private static void EmitStoreResult(
        ILGenerator il,
        Type returnType)
    {
        if (returnType.IsValueType || returnType.IsGenericParameter)
            il.Emit(OpCodes.Stobj, returnType);
        else
            il.Emit(OpCodes.Stind_Ref);
    }

    private static void EmitLoadState(
        ILGenerator il,
        int stateArgumentIndex)
    {
        il.Emit(OpCodes.Ldarg, stateArgumentIndex);
        il.Emit(OpCodes.Ldind_Ref);
    }

    private static void EmitClearState(
        ILGenerator il,
        int stateArgumentIndex)
    {
        il.Emit(OpCodes.Ldarg, stateArgumentIndex);
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Stind_Ref);
    }
}
