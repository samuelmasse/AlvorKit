namespace AlvorKit.Mocking;

/// <summary>Emits shared exact typed initialization helpers.</summary>
internal static partial class MockTypedTrampolineIl
{
    private static void EmitInitializeResult(
        ILGenerator il,
        Type returnType)
    {
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Initobj, GetValueType(returnType));
    }

    private static void EmitBooleanReturn(
        ILGenerator il,
        bool value)
    {
        il.Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ret);
    }

    private static Type GetValueType(Type type) =>
        type.IsByRef ? type.GetElementType()! : type;
}
