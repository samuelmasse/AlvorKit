namespace AlvorKit.Mocking;

/// <summary>
/// Emits an exact wrapper that composes the shared typed prefix,
/// preserved original delegate, and exact completion artifact.
/// </summary>
internal static partial class MockInterceptionWrapperIl
{
    private static readonly MethodInfo LogicalMethodGetter =
        typeof(MockInterceptionBindingState).GetProperty(
            nameof(MockInterceptionBindingState.LogicalMethod),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo OriginalGetter =
        typeof(MockInterceptionBindingState).GetProperty(
            nameof(MockInterceptionBindingState.Original),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo ReceiverGetter =
        typeof(MockInterceptionBindingState).GetProperty(
            nameof(MockInterceptionBindingState.Receiver),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo ApplyFieldWriteDefinition =
        typeof(MockReceiverFreeFieldRuntime).GetMethod(
            nameof(MockReceiverFreeFieldRuntime.ApplyWrite),
            BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo ApplyFieldReadDefinition =
        typeof(MockReceiverFreeFieldRuntime).GetMethod(
            nameof(MockReceiverFreeFieldRuntime.ApplyRead),
            BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo IsConstructorBehaviorGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.IsReceiverFreeConstructorBehavior),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo ReplacesConstructorBodyGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.ReplacesReceiverFreeConstructorBody),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo ConstructorCallbackGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.ReceiverFreeConstructorCallback),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo CompleteBehaviorThrown =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.CompleteReceiverFreeBehaviorThrown),
            BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo CompleteConstructorReplacement =
        typeof(MockDispatchContinuation).GetMethod(
            nameof(MockDispatchContinuation.CompleteReceiverFreeConstructorReplacement),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>Emits one complete exact wrapper body.</summary>
    internal static void Emit(
        ILGenerator il,
        MethodInfo operation,
        Type delegateType,
        MethodInfo invoke,
        MockTypedTrampolineArtifact trampoline,
        MockOperationKind operationKind)
    {
        bool receiverFree =
            operationKind != MockOperationKind.InstanceMethod;
        Type returnType = operation.ReturnType;
        bool hasResult = returnType != typeof(void);
        bool isManagedReference =
            MockManagedReferenceAbi.IsSupported(returnType);
        LocalBuilder continuation = il.DeclareLocal(
            typeof(MockDispatchContinuation));
        LocalBuilder? result = hasResult && !isManagedReference
            ? il.DeclareLocal(returnType)
            : null;
        LocalBuilder? factory = isManagedReference
            ? il.DeclareLocal(
                MockManagedReferenceAbi.FactoryType(returnType))
            : null;
        LocalBuilder? alias = isManagedReference
            ? il.DeclareLocal(returnType)
            : null;
        LocalBuilder? receiver = receiverFree
            ? il.DeclareLocal(typeof(object))
            : null;
        int operationArgumentOffset = receiverFree ? 1 : 2;
        Label dispatch = il.DefineLabel();
        Label original = il.DefineLabel();
        Label trackedOriginal = il.DefineLabel();

        if (receiverFree)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, ReceiverGetter);
            il.Emit(OpCodes.Stloc, receiver!);
            il.Emit(OpCodes.Ldloc, receiver!);
        }
        else
        {
            il.Emit(OpCodes.Ldarg_1);
        }
        il.Emit(OpCodes.Brtrue, dispatch);
        EmitOriginalCall(il, delegateType, invoke);
        il.Emit(OpCodes.Ret);

        il.MarkLabel(dispatch);
        EmitPrefix(
            il,
            operation,
            trampoline.Prefix,
            result,
            factory,
            receiver,
            operationArgumentOffset,
            continuation);
        il.Emit(OpCodes.Brtrue, original);
        EmitHandledReturn(
            il,
            operation,
            trampoline.Finalizer,
            result,
            factory,
            alias,
            continuation,
            operationArgumentOffset);

        il.MarkLabel(original);
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Brtrue, trackedOriginal);
        EmitOriginalCall(il, delegateType, invoke);
        il.Emit(OpCodes.Ret);

        il.MarkLabel(trackedOriginal);
        EmitTrackedOriginal(
            il,
            operation,
            delegateType,
            invoke,
            trampoline.Finalizer,
            result,
            alias,
            continuation,
            operationArgumentOffset,
            operationKind);
    }

    private static void EmitPrefix(
        ILGenerator il,
        MethodInfo operation,
        MethodInfo prefix,
        LocalBuilder? result,
        LocalBuilder? factory,
        LocalBuilder? receiver,
        int operationArgumentOffset,
        LocalBuilder continuation)
    {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, LogicalMethodGetter);
        if (receiver is null)
            il.Emit(OpCodes.Ldarg_1);
        else
            il.Emit(OpCodes.Ldloc, receiver);
        if (operation.ReturnType != typeof(void))
        {
            il.Emit(
                OpCodes.Ldloca,
                factory ?? result!);
        }

        MockInterceptionWrapperCompletionIl.EmitOperationArguments(
            il,
            operation,
            operationArgumentOffset);
        il.Emit(OpCodes.Ldloca, continuation);
        il.Emit(OpCodes.Call, prefix);
    }

    private static void EmitHandledReturn(
        ILGenerator il,
        MethodInfo operation,
        MethodInfo finalizer,
        LocalBuilder? result,
        LocalBuilder? factory,
        LocalBuilder? alias,
        LocalBuilder continuation,
        int operationArgumentOffset)
    {
        if (factory is null)
        {
            if (result is not null)
                il.Emit(OpCodes.Ldloc, result);
            il.Emit(OpCodes.Ret);
            return;
        }

        Label tracked = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, continuation);
        il.Emit(OpCodes.Brtrue, tracked);
        il.Emit(OpCodes.Ldloc, factory);
        il.Emit(
            OpCodes.Callvirt,
            MockInterceptionWrapperCompletionIl.FactoryInvoke(
                operation.ReturnType));
        il.Emit(OpCodes.Ret);

        il.MarkLabel(tracked);
        EmitTrackedFactory(
            il,
            operation,
            finalizer,
            factory,
            alias!,
            continuation,
            operationArgumentOffset);
    }

}
