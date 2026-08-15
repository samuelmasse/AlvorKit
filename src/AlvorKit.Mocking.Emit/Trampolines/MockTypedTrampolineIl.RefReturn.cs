namespace AlvorKit;

/// <summary>Emits shared exact managed-reference return paths.</summary>
internal static partial class MockTypedTrampolineIl
{
    private static readonly MethodInfo IsTypedRefReturnFactoryGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.IsTypedRefReturnFactory),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;
    private static readonly MethodInfo TypedRefReturnFactoryGetter =
        typeof(MockDispatchContinuation).GetProperty(
            nameof(MockDispatchContinuation.TypedRefReturnFactory),
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetMethod!;

    /// <summary>Publishes the selected stable alias factory through the exact result-ref ABI.</summary>
    private static void EmitManagedReferenceResult(
        ILGenerator il,
        Type returnType,
        int stateArgumentIndex,
        LocalBuilder result)
    {
        Type factoryType = MockManagedReferenceAbi.FactoryType(returnType);
        LocalBuilder factory = il.DeclareLocal(factoryType);
        Label useResult = il.DefineLabel();
        Label publish = il.DefineLabel();

        EmitLoadState(il, stateArgumentIndex);
        il.Emit(OpCodes.Brfalse, useResult);
        EmitLoadState(il, stateArgumentIndex);
        il.Emit(OpCodes.Callvirt, IsTypedRefReturnFactoryGetter);
        il.Emit(OpCodes.Brfalse, useResult);
        EmitLoadState(il, stateArgumentIndex);
        il.Emit(OpCodes.Callvirt, TypedRefReturnFactoryGetter);
        il.Emit(OpCodes.Castclass, factoryType);
        il.Emit(OpCodes.Stloc, factory);
        il.Emit(OpCodes.Br, publish);

        il.MarkLabel(useResult);
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Castclass, factoryType);
        il.Emit(OpCodes.Stloc, factory);

        il.MarkLabel(publish);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldloc, factory);
        il.Emit(OpCodes.Stind_Ref);
        EmitBooleanReturn(il, false);
    }
}
