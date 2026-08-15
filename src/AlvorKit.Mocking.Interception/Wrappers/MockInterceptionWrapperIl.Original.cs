namespace AlvorKit;

/// <summary>Emits tracked original, constructor, field, and alias-factory paths.</summary>
internal static partial class MockInterceptionWrapperIl
{
    private static void EmitTrackedOriginal(
        ILGenerator il,
        MethodInfo operation,
        Type delegateType,
        MethodInfo invoke,
        MethodInfo finalizer,
        LocalBuilder? result,
        LocalBuilder? alias,
        LocalBuilder continuation,
        int operationArgumentOffset,
        MockOperationKind operationKind)
    {
        if (operationKind == MockOperationKind.ConstructorBody)
        {
            EmitConstructorBehavior(
                il,
                operation,
                continuation,
                operationArgumentOffset);
        }
        if (operationKind == MockOperationKind.FieldWrite)
        {
            EmitApplyFieldWrite(
                il,
                operation,
                continuation,
                operationArgumentOffset);
        }

        Label returned = il.DefineLabel();
        il.BeginExceptionBlock();
        EmitOriginalCall(il, delegateType, invoke);
        if (operation.ReturnType != typeof(void))
            il.Emit(OpCodes.Stloc, alias ?? result!);
        il.Emit(OpCodes.Leave, returned);
        il.BeginCatchBlock(typeof(Exception));
        MockInterceptionWrapperCompletionIl.EmitThrown(
            il,
            operation,
            finalizer,
            result,
            alias,
            continuation,
            operationArgumentOffset);
        il.Emit(OpCodes.Rethrow);
        il.EndExceptionBlock();

        il.MarkLabel(returned);
        if (operationKind == MockOperationKind.FieldRead)
        {
            EmitApplyFieldRead(
                il,
                operation,
                continuation,
                result!);
        }
        MockInterceptionWrapperCompletionIl.EmitReturned(
            il,
            operation,
            finalizer,
            result,
            alias,
            continuation,
            operationArgumentOffset);
        if (operation.ReturnType != typeof(void))
            il.Emit(OpCodes.Ldloc, alias ?? result!);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitConstructorBehavior(
        ILGenerator il,
        MethodInfo operation,
        LocalBuilder continuation,
        int operationArgumentOffset)
    {
        Type callbackType =
            MockTypedCallbackDelegateCache.GetOrCreate(operation);
        MethodInfo invoke = callbackType.GetMethod(
            nameof(Action.Invoke))!;
        LocalBuilder exception = il.DeclareLocal(typeof(Exception));
        Label original = il.DefineLabel();
        Label callbackReturned = il.DefineLabel();

        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Callvirt, IsConstructorBehaviorGetter);
        il.Emit(OpCodes.Brfalse, original);

        il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Callvirt, ConstructorCallbackGetter);
        il.Emit(OpCodes.Castclass, callbackType);
        MockInterceptionWrapperCompletionIl.EmitOperationArguments(
            il,
            operation,
            operationArgumentOffset);
        il.Emit(OpCodes.Callvirt, invoke);
        il.Emit(OpCodes.Leave, callbackReturned);
        il.BeginCatchBlock(typeof(Exception));
        il.Emit(OpCodes.Stloc, exception);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Ldloc, exception);
        il.Emit(OpCodes.Callvirt, CompleteBehaviorThrown);
        il.Emit(OpCodes.Rethrow);
        il.EndExceptionBlock();

        il.MarkLabel(callbackReturned);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Callvirt, ReplacesConstructorBodyGetter);
        il.Emit(OpCodes.Brfalse, original);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Callvirt, CompleteConstructorReplacement);
        il.Emit(OpCodes.Ret);

        il.MarkLabel(original);
    }

    private static void EmitApplyFieldWrite(
        ILGenerator il,
        MethodInfo operation,
        LocalBuilder continuation,
        int operationArgumentOffset)
    {
        ParameterInfo[] parameters = operation.GetParameters();
        Type fieldType = parameters[^1].ParameterType;
        int valueArgument =
            operationArgumentOffset + parameters.Length - 1;
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Ldarga, valueArgument);
        il.Emit(
            OpCodes.Call,
            ApplyFieldWriteDefinition.MakeGenericMethod(fieldType));
    }

    private static void EmitApplyFieldRead(
        ILGenerator il,
        MethodInfo operation,
        LocalBuilder continuation,
        LocalBuilder result)
    {
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Ldloca, result);
        il.Emit(
            OpCodes.Call,
            ApplyFieldReadDefinition.MakeGenericMethod(
                operation.ReturnType));
    }

    private static void EmitTrackedFactory(
        ILGenerator il,
        MethodInfo operation,
        MethodInfo finalizer,
        LocalBuilder factory,
        LocalBuilder alias,
        LocalBuilder continuation,
        int operationArgumentOffset)
    {
        Label returned = il.DefineLabel();
        il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldloc, factory);
        il.Emit(
            OpCodes.Callvirt,
            MockInterceptionWrapperCompletionIl.FactoryInvoke(
                operation.ReturnType));
        il.Emit(OpCodes.Stloc, alias);
        il.Emit(OpCodes.Leave, returned);
        il.BeginCatchBlock(typeof(Exception));
        MockInterceptionWrapperCompletionIl.EmitThrown(
            il,
            operation,
            finalizer,
            null,
            alias,
            continuation,
            operationArgumentOffset);
        il.Emit(OpCodes.Rethrow);
        il.EndExceptionBlock();

        il.MarkLabel(returned);
        MockInterceptionWrapperCompletionIl.EmitReturned(
            il,
            operation,
            finalizer,
            null,
            alias,
            continuation,
            operationArgumentOffset);
        il.Emit(OpCodes.Ldloc, alias);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitOriginalCall(
        ILGenerator il,
        Type delegateType,
        MethodInfo invoke)
    {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, OriginalGetter);
        il.Emit(OpCodes.Castclass, delegateType);
        ParameterInfo[] parameters = invoke.GetParameters();
        for (int index = 0; index < parameters.Length; index++)
            il.Emit(OpCodes.Ldarg, index + 1);
        il.Emit(OpCodes.Callvirt, invoke);
    }
}
