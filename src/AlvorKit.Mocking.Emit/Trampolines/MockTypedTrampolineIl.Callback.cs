using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Emits shared exact typed callback paths.</summary>
internal static partial class MockTypedTrampolineIl
{
    private static readonly MethodInfo IsTypedCallbackGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.IsTypedCallback),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo TypedCallbackGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.TypedCallback),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo CompleteTypedCallbackThrownMethod =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.CompleteTypedCallbackThrown),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>Invokes one normalized exact callback directly over the live typed frame.</summary>
    private static void EmitTypedCallback(
        ILGenerator il,
        Type callbackType,
        Type returnType,
        IReadOnlyList<MockIlParameter> parameters,
        ImmutableArray<int> carrierIndices,
        int parameterOffset,
        int stateArgumentIndex,
        LocalBuilder arguments,
        LocalBuilder matcherEvaluation)
    {
        MethodInfo invoke = callbackType.ContainsGenericParameters
            ? TypeBuilder.GetMethod(
                callbackType,
                callbackType.GetGenericTypeDefinition().GetMethod(
                    nameof(Action.Invoke))!)
            : callbackType.GetMethod(nameof(Action.Invoke))!;
        LocalBuilder exception = il.DeclareLocal(typeof(Exception));
        LocalBuilder continuation = il.DeclareLocal(
            typeof(MockDispatchContinuation));
        Label completed = il.DefineLabel();

        EmitLoadState(il, stateArgumentIndex);
        il.Emit(OpCodes.Stloc, continuation);
        il.BeginExceptionBlock();
        if (returnType != typeof(void))
            il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Callvirt, TypedCallbackGetter);
        il.Emit(OpCodes.Castclass, callbackType);
        for (var index = 0; index < parameters.Count; index++)
            il.Emit(OpCodes.Ldarg, index + parameterOffset);
        il.Emit(OpCodes.Callvirt, invoke);
        if (returnType != typeof(void))
            EmitStoreResult(il, returnType);
        il.Emit(OpCodes.Leave, completed);

        il.BeginCatchBlock(typeof(Exception));
        il.Emit(OpCodes.Stloc, exception);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Ldloc, exception);
        il.Emit(OpCodes.Callvirt, CompleteTypedCallbackThrownMethod);
        EmitClearState(il, stateArgumentIndex);
        il.Emit(OpCodes.Rethrow);
        il.EndExceptionBlock();

        il.MarkLabel(completed);
        MockTypedArgumentIl.EmitRefreshReferenceArguments(
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
}
