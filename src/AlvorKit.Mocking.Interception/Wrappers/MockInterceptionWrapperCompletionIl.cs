namespace AlvorKit.Mocking;

/// <summary>
/// Emits exact completion calls for intercepted original and ref-factory paths.
/// </summary>
internal static class MockInterceptionWrapperCompletionIl
{
    /// <summary>Emits exceptional completion from inside a catch block.</summary>
    internal static void EmitThrown(
        ILGenerator il,
        MethodInfo operation,
        MethodInfo finalizer,
        LocalBuilder? result,
        LocalBuilder? alias,
        LocalBuilder continuation,
        int operationArgumentOffset)
    {
        LocalBuilder exception = il.DeclareLocal(typeof(Exception));
        il.Emit(OpCodes.Stloc, exception);
        il.Emit(OpCodes.Ldloc, exception);
        EmitTail(
            il,
            operation,
            finalizer,
            result,
            alias,
            continuation,
            operationArgumentOffset);
        il.Emit(OpCodes.Pop);
    }

    /// <summary>Emits successful completion after an exact return.</summary>
    internal static void EmitReturned(
        ILGenerator il,
        MethodInfo operation,
        MethodInfo finalizer,
        LocalBuilder? result,
        LocalBuilder? alias,
        LocalBuilder continuation,
        int operationArgumentOffset)
    {
        il.Emit(OpCodes.Ldnull);
        EmitTail(
            il,
            operation,
            finalizer,
            result,
            alias,
            continuation,
            operationArgumentOffset);
        Label completed = il.DefineLabel();
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Brfalse, completed);
        il.Emit(OpCodes.Throw);
        il.MarkLabel(completed);
        il.Emit(OpCodes.Pop);
    }

    /// <summary>Loads the operation arguments from the wrapper ABI.</summary>
    internal static void EmitOperationArguments(
        ILGenerator il,
        MethodInfo operation,
        int operationArgumentOffset)
    {
        ParameterInfo[] parameters = operation.GetParameters();
        for (int index = 0; index < parameters.Length; index++)
            il.Emit(
                OpCodes.Ldarg,
                index + operationArgumentOffset);
    }

    /// <summary>Gets the exact managed-reference factory entry point.</summary>
    internal static MethodInfo FactoryInvoke(Type returnType) =>
        MockManagedReferenceAbi.FactoryType(returnType)
            .GetMethod(nameof(Action.Invoke))!;

    private static void EmitTail(
        ILGenerator il,
        MethodInfo operation,
        MethodInfo finalizer,
        LocalBuilder? result,
        LocalBuilder? alias,
        LocalBuilder continuation,
        int operationArgumentOffset)
    {
        il.Emit(OpCodes.Ldloc, continuation);
        if (operation.ReturnType != typeof(void))
        {
            if (alias is not null)
                il.Emit(OpCodes.Ldloc, alias);
            else
                il.Emit(OpCodes.Ldloca, result!);
        }

        EmitOperationArguments(
            il,
            operation,
            operationArgumentOffset);
        il.Emit(OpCodes.Call, finalizer);
    }
}
