namespace AlvorKit.Mocking;

/// <summary>Emits direct exact dispatch bodies for generated proxy methods.</summary>
internal static class MockProxyMethodBodyIl
{
    private static readonly ConstructorInfo MockExceptionConstructor =
        typeof(MockException).GetConstructor([typeof(string)])!;

    /// <summary>Emits a complete direct proxy body over an exact prefix.</summary>
    internal static void Emit(
        ILGenerator il,
        FieldInfo method,
        MethodInfo prefix,
        MethodInfo? finalizer,
        FieldInfo? resultStorage,
        Type returnType,
        IReadOnlyList<MockIlParameter> parameters)
    {
        bool managedReference =
            MockManagedReferenceAbi.IsSupported(returnType);
        Type? factoryType = managedReference
            ? MockManagedReferenceAbi.FactoryType(returnType)
            : null;
        bool unsupportedReference =
            returnType.IsByRef && !managedReference;
        LocalBuilder? result = returnType == typeof(void) ||
            resultStorage is not null
            ? null
            : il.DeclareLocal(
                factoryType ??
                (unsupportedReference
                    ? returnType.GetElementType()!
                    : returnType));
        LocalBuilder state =
            il.DeclareLocal(typeof(MockDispatchContinuation));
        Label unhandled = il.DefineLabel();

        il.Emit(OpCodes.Ldsfld, method);
        il.Emit(OpCodes.Ldarg_0);
        if (resultStorage is not null)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, resultStorage);
        }
        else if (result is not null)
        {
            il.Emit(OpCodes.Ldloca, result);
        }
        for (int index = 0; index < parameters.Count; index++)
            il.Emit(OpCodes.Ldarg, index + 1);
        il.Emit(OpCodes.Ldloca, state);
        il.Emit(OpCodes.Call, prefix);
        il.Emit(OpCodes.Brtrue, unhandled);

        if (managedReference)
        {
            EmitManagedReferenceReturn(
                il,
                factoryType!,
                returnType,
                parameters,
                result!,
                state,
                finalizer!);
        }
        else if (resultStorage is not null)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, resultStorage);
            il.Emit(OpCodes.Ret);
        }
        else if (unsupportedReference)
        {
            EmitFailure(
                il,
                "Generated proxy dispatch cannot expose a managed reference " +
                "to a ref-struct value.");
        }
        else
        {
            if (result is not null)
                il.Emit(OpCodes.Ldloc, result);
            il.Emit(OpCodes.Ret);
        }

        il.MarkLabel(unhandled);
        EmitFailure(
            il,
            "Generated proxy dispatch unexpectedly requested an original body.");
    }

    private static void EmitManagedReferenceReturn(
        ILGenerator il,
        Type factoryType,
        Type returnType,
        IReadOnlyList<MockIlParameter> parameters,
        LocalBuilder factory,
        LocalBuilder state,
        MethodInfo finalizer)
    {
        MethodInfo invoke = factoryType.GetMethod(nameof(Action.Invoke))!;
        LocalBuilder alias = il.DeclareLocal(returnType);
        LocalBuilder exception = il.DeclareLocal(typeof(Exception));
        Label tracked = il.DefineLabel();
        Label returned = il.DefineLabel();

        il.Emit(OpCodes.Ldloc, state);
        il.Emit(OpCodes.Brtrue, tracked);
        il.Emit(OpCodes.Ldloc, factory);
        il.Emit(OpCodes.Callvirt, invoke);
        il.Emit(OpCodes.Ret);

        il.MarkLabel(tracked);
        il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldloc, factory);
        il.Emit(OpCodes.Callvirt, invoke);
        il.Emit(OpCodes.Stloc, alias);
        il.Emit(OpCodes.Leave, returned);
        il.BeginCatchBlock(typeof(Exception));
        il.Emit(OpCodes.Stloc, exception);
        il.Emit(OpCodes.Ldloc, exception);
        EmitFinalizerArguments(
            il,
            parameters,
            state,
            alias);
        il.Emit(OpCodes.Call, finalizer);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Rethrow);
        il.EndExceptionBlock();

        il.MarkLabel(returned);
        il.Emit(OpCodes.Ldnull);
        EmitFinalizerArguments(
            il,
            parameters,
            state,
            alias);
        il.Emit(OpCodes.Call, finalizer);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldloc, alias);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitFinalizerArguments(
        ILGenerator il,
        IReadOnlyList<MockIlParameter> parameters,
        LocalBuilder state,
        LocalBuilder alias)
    {
        il.Emit(OpCodes.Ldloc, state);
        il.Emit(OpCodes.Ldloc, alias);
        for (int index = 0; index < parameters.Count; index++)
            il.Emit(OpCodes.Ldarg, index + 1);
    }

    private static void EmitFailure(
        ILGenerator il,
        string message)
    {
        il.Emit(OpCodes.Ldstr, message);
        il.Emit(OpCodes.Newobj, MockExceptionConstructor);
        il.Emit(OpCodes.Throw);
    }
}
