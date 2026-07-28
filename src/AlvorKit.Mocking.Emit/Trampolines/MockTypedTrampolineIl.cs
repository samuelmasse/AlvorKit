using System.Collections.Immutable;

namespace AlvorKit.Mocking;

/// <summary>
/// Emits the shared exact stack-to-control-plane bridge for one typed prefix.
/// </summary>
internal static partial class MockTypedTrampolineIl
{
    private static readonly MethodInfo GetMockedMethod = typeof(Mock).GetMethod(
        "GetMocked",
        BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo DispatchMethod = typeof(Rewire).GetMethod(
        nameof(Rewire.Method),
        BindingFlags.Static | BindingFlags.NonPublic,
        [
            typeof(MethodInfo),
            typeof(object),
            typeof(Mocked),
            typeof(object[]),
            typeof(MockTypedMatcherEvaluation),
            typeof(object).MakeByRefType(),
            typeof(MockDispatchContinuation).MakeByRefType(),
            typeof(string)
        ])!;

    /// <summary>
    /// Emits one exact dispatch body that boxes only ordinary retained values.
    /// </summary>
    internal static void Emit(
        ILGenerator il,
        MethodInfo target,
        ParameterInfo[] parameters,
        ImmutableArray<int> carrierIndices,
        string backend) =>
        Emit(
            il,
            target.ReturnType,
            MockIlParameter.Create(parameters),
            carrierIndices,
            CanUseTypedCallbackReturn(target.ReturnType)
                ? MockTypedCallbackDelegateCache.GetOrCreate(target)
                : null,
            backend);

    /// <summary>
    /// Emits one typed prefix from already substituted return and parameter types.
    /// </summary>
    internal static void Emit(
        ILGenerator il,
        Type returnType,
        IReadOnlyList<MockIlParameter> parameters,
        ImmutableArray<int> carrierIndices,
        Type? typedCallbackType,
        string backend)
    {
        bool hasResult = returnType != typeof(void);
        int parameterOffset = hasResult ? 3 : 2;
        int stateArgumentIndex = parameterOffset + parameters.Count;
        LocalBuilder mocked = il.DeclareLocal(typeof(Mocked));
        LocalBuilder arguments = il.DeclareLocal(typeof(object[]));
        LocalBuilder result = il.DeclareLocal(typeof(object));
        LocalBuilder matcherEvaluation = il.DeclareLocal(
            typeof(MockTypedMatcherEvaluation));
        Label hasMock = il.DefineLabel();
        Label handled = il.DefineLabel();

        il.Emit(OpCodes.Ldarg, stateArgumentIndex);
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Stind_Ref);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, GetMockedMethod);
        il.Emit(OpCodes.Stloc, mocked);
        il.Emit(OpCodes.Ldloc, mocked);
        il.Emit(OpCodes.Brtrue, hasMock);
        EmitBooleanReturn(il, true);

        il.MarkLabel(hasMock);
        MockTypedArgumentIl.EmitArguments(
            il,
            parameters,
            carrierIndices,
            parameterOffset,
            arguments);
        MockTypedMatcherIl.EmitEvaluation(
            il,
            parameters,
            parameterOffset,
            mocked,
            arguments,
            matcherEvaluation,
            backend);
        MockTypedProjectionIl.EmitEntry(
            il,
            parameters,
            parameterOffset,
            matcherEvaluation);
        MockTypedMutationIl.Emit(
            il,
            parameters,
            carrierIndices,
            parameterOffset,
            arguments,
            matcherEvaluation,
            MockSnapshotPhase.Entry);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldloc, mocked);
        il.Emit(OpCodes.Ldloc, arguments);
        il.Emit(OpCodes.Ldloc, matcherEvaluation);
        il.Emit(OpCodes.Ldloca, result);
        il.Emit(OpCodes.Ldarg, stateArgumentIndex);
        il.Emit(OpCodes.Ldstr, backend);
        il.Emit(OpCodes.Call, DispatchMethod);
        il.Emit(OpCodes.Brtrue, handled);
        EmitBooleanReturn(il, true);

        il.MarkLabel(handled);
        if (typedCallbackType is not null)
        {
            Label notTypedCallback = il.DefineLabel();
            EmitLoadState(il, stateArgumentIndex);
            il.Emit(OpCodes.Brfalse, notTypedCallback);
            EmitLoadState(il, stateArgumentIndex);
            il.Emit(OpCodes.Callvirt, IsTypedCallbackGetter);
            il.Emit(OpCodes.Brfalse, notTypedCallback);
            EmitTypedCallback(
                il,
                typedCallbackType,
                returnType,
                parameters,
                carrierIndices,
                parameterOffset,
                stateArgumentIndex,
                arguments,
                matcherEvaluation);
            il.MarkLabel(notTypedCallback);
        }

        if (MockManagedReferenceAbi.IsSupported(returnType))
        {
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
            LocalBuilder continuation = il.DeclareLocal(
                typeof(MockDispatchContinuation));
            EmitLoadState(il, stateArgumentIndex);
            il.Emit(OpCodes.Stloc, continuation);
            EmitClearState(il, stateArgumentIndex);
            MockTypedProjectionIl.EmitExit(
                il,
                parameters,
                parameterOffset,
                matcherEvaluation);
            il.Emit(OpCodes.Ldarg, stateArgumentIndex);
            il.Emit(OpCodes.Ldloc, continuation);
            il.Emit(OpCodes.Stind_Ref);
            EmitManagedReferenceResult(
                il,
                returnType,
                stateArgumentIndex,
                result);
            return;
        }

        Label ordinaryHandled = il.DefineLabel();
        if (!CanUseTypedReturnFactory(returnType))
        {
            il.Emit(OpCodes.Br, ordinaryHandled);
        }
        else
        {
            il.Emit(OpCodes.Ldarg, stateArgumentIndex);
            il.Emit(OpCodes.Ldind_Ref);
            il.Emit(OpCodes.Brfalse, ordinaryHandled);
            il.Emit(OpCodes.Ldarg, stateArgumentIndex);
            il.Emit(OpCodes.Ldind_Ref);
            il.Emit(OpCodes.Callvirt, IsTypedReturnFactoryGetter);
            il.Emit(OpCodes.Brfalse, ordinaryHandled);
            EmitTypedReturnFactory(
                il,
                returnType,
                stateArgumentIndex,
                arguments,
                parameters,
                carrierIndices,
                parameterOffset,
                matcherEvaluation);
        }

        il.MarkLabel(ordinaryHandled);
        EmitWritebacks(il, parameters, carrierIndices, parameterOffset, arguments);
        MockTypedMutationIl.Emit(
            il,
            parameters,
            carrierIndices,
            parameterOffset,
            arguments,
            matcherEvaluation,
            MockSnapshotPhase.Exit);
        MockTypedProjectionIl.EmitExit(
            il,
            parameters,
            parameterOffset,
            matcherEvaluation);
        MockTypedProjectionIl.EmitCompleteReturned(
            il,
            matcherEvaluation,
            arguments,
            result);
        if (hasResult)
            EmitResult(il, returnType, result);
        EmitBooleanReturn(il, false);
    }

}
