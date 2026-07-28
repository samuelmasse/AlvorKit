namespace AlvorKit.Mocking;

/// <summary>
/// Emits shared direct matcher evaluation while exact parameter values remain live.
/// </summary>
internal static class MockTypedMatcherIl
{
    private static readonly MethodInfo OpenMethod =
        typeof(MockTypedMatcherEvaluation).GetMethod(
            nameof(MockTypedMatcherEvaluation.Open),
            BindingFlags.Static | BindingFlags.NonPublic,
            [
                typeof(Mocked),
                typeof(MethodInfo),
                typeof(object[]),
                typeof(string)
            ])!;
    private static readonly MethodInfo MatchMethod =
        typeof(MockTypedMatcherEvaluation).GetMethod(
            nameof(MockTypedMatcherEvaluation.Match),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Opens a single invocation evaluation and applies every live input
    /// position directly without boxing.
    /// </summary>
    internal static void EmitEvaluation(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        int parameterOffset,
        LocalBuilder mocked,
        LocalBuilder arguments,
        LocalBuilder evaluation,
        string backend)
    {
        il.Emit(OpCodes.Ldloc, mocked);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldloc, arguments);
        il.Emit(OpCodes.Ldstr, backend);
        il.Emit(OpCodes.Call, OpenMethod);
        il.Emit(OpCodes.Stloc, evaluation);

        for (var index = 0; index < parameters.Count; index++)
        {
            MockIlParameter parameter = parameters[index];
            Type valueType = parameter.Type.IsByRef
                ? parameter.Type.GetElementType()!
                : parameter.Type;
            if (parameter.IsOut ||
                valueType.IsPointer ||
                valueType.IsFunctionPointer)
            {
                continue;
            }

            Label complete = il.DefineLabel();
            il.Emit(OpCodes.Ldloc, evaluation);
            il.Emit(OpCodes.Brfalse, complete);
            il.Emit(OpCodes.Ldloc, evaluation);
            il.Emit(OpCodes.Ldc_I4, index);
            if (parameter.Type.IsByRef)
                il.Emit(OpCodes.Ldarg, index + parameterOffset);
            else
                il.Emit(OpCodes.Ldarga, index + parameterOffset);
            il.Emit(OpCodes.Callvirt, MatchMethod.MakeGenericMethod(valueType));
            il.MarkLabel(complete);
        }
    }
}
