using System.Collections.Immutable;

namespace AlvorKit.Mocking;

/// <summary>Emits shared synchronous live-struct receiver mutation hooks.</summary>
internal static class MockTypedMutationIl
{
    private static readonly MethodInfo MutateMethod =
        typeof(MockTypedMatcherEvaluation).GetMethod(
            nameof(MockTypedMatcherEvaluation.MutateStructThis),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    internal static void Emit(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        ImmutableArray<int> carrierIndices,
        int parameterOffset,
        LocalBuilder arguments,
        LocalBuilder evaluation,
        MockSnapshotPhase phase)
    {
        if (parameters.Count == 0 ||
            !parameters[0].Type.IsByRef ||
            parameters[0].Type.GetElementType() is not
            { IsValueType: true, IsByRefLike: false } receiverType)
        {
            return;
        }

        Label complete = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, evaluation);
        il.Emit(OpCodes.Brfalse, complete);
        il.Emit(OpCodes.Ldloc, evaluation);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4, (int)phase);
        il.Emit(OpCodes.Ldarg, parameterOffset);
        il.Emit(
            OpCodes.Callvirt,
            MutateMethod.MakeGenericMethod(receiverType));
        il.Emit(OpCodes.Brfalse, complete);
        MockTypedArgumentIl.EmitRefreshReferenceArguments(
            il,
            parameters,
            carrierIndices,
            parameterOffset,
            arguments);
        il.MarkLabel(complete);
    }
}
