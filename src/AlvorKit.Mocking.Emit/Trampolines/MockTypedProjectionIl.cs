namespace AlvorKit.Mocking;

/// <summary>Emits shared exact typed entry and exit history projection calls.</summary>
internal static class MockTypedProjectionIl
{
    private static readonly MethodInfo ProjectMethod =
        typeof(MockTypedMatcherEvaluation).GetMethod(
            nameof(MockTypedMatcherEvaluation.Project),
            BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo CompleteReturnedMethod =
        typeof(MockTypedMatcherEvaluation).GetMethod(
            nameof(MockTypedMatcherEvaluation.CompleteReturned),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>Emits selected entry projectors before behavior execution.</summary>
    internal static void EmitEntry(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        int parameterOffset,
        LocalBuilder evaluation) =>
        Emit(
            il,
            parameters,
            parameterOffset,
            evaluation,
            MockSnapshotPhase.Entry);

    /// <summary>Emits selected exit projectors after successful writeback.</summary>
    internal static void EmitExit(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        int parameterOffset,
        LocalBuilder evaluation) =>
        Emit(
            il,
            parameters,
            parameterOffset,
            evaluation,
            MockSnapshotPhase.Exit);

    /// <summary>Completes a configured behavior whose exit projection deferred capture.</summary>
    internal static void EmitCompleteReturned(
        ILGenerator il,
        LocalBuilder evaluation,
        LocalBuilder arguments,
        LocalBuilder result)
    {
        Label complete = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, evaluation);
        il.Emit(OpCodes.Brfalse, complete);
        il.Emit(OpCodes.Ldloc, evaluation);
        il.Emit(OpCodes.Ldloc, arguments);
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Callvirt, CompleteReturnedMethod);
        il.Emit(OpCodes.Stloc, result);
        il.MarkLabel(complete);
    }

    private static void Emit(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        int parameterOffset,
        LocalBuilder evaluation,
        MockSnapshotPhase phase)
    {
        for (var index = 0; index < parameters.Count; index++)
        {
            MockIlParameter parameter = parameters[index];
            if (phase == MockSnapshotPhase.Entry && parameter.IsOut)
                continue;
            if (phase == MockSnapshotPhase.Exit &&
                (!parameter.Type.IsByRef || parameter.IsIn))
            {
                continue;
            }

            Type valueType = parameter.Type.IsByRef
                ? parameter.Type.GetElementType()!
                : parameter.Type;
            if (valueType.IsPointer ||
                valueType.IsFunctionPointer)
            {
                continue;
            }

            Label complete = il.DefineLabel();
            il.Emit(OpCodes.Ldloc, evaluation);
            il.Emit(OpCodes.Brfalse, complete);
            il.Emit(OpCodes.Ldloc, evaluation);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldc_I4, (int)phase);
            if (parameter.Type.IsByRef)
                il.Emit(OpCodes.Ldarg, index + parameterOffset);
            else
                il.Emit(OpCodes.Ldarga, index + parameterOffset);
            il.Emit(
                OpCodes.Callvirt,
                ProjectMethod.MakeGenericMethod(valueType));
            il.MarkLabel(complete);
        }
    }
}
