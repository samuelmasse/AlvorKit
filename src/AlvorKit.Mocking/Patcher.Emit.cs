namespace AlvorKit.Mocking;

internal static partial class Patcher
{
    /// <summary>Emits IL that copies method arguments into the generic rewire argument carrier.</summary>
    private static void EmitRewireArgInit(
        ILGenerator il,
        Type rewireType,
        int[] valueParamIndices,
        int[] refParamIndices,
        int[] refStructParamIndices,
        bool isVoid)
    {
        var argsLocal = il.DeclareLocal(rewireType);
        int offset = isVoid ? 2 : 3;

        EmitFields(il, rewireType, argsLocal, valueParamIndices, "V", offset);
        EmitFields(il, rewireType, argsLocal, refParamIndices, "R", offset);
        EmitFields(il, rewireType, argsLocal, refStructParamIndices, "S", offset);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        if (!isVoid)
            il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldloc, 0);
    }

    /// <summary>Emits one group of argument carrier field stores.</summary>
    private static void EmitFields(ILGenerator il, Type rewireType, LocalBuilder argsLocal, int[] indices, string prefix, int offset)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            var field = rewireType.GetField($"{prefix}{i}")!;
            il.Emit(OpCodes.Ldloca_S, argsLocal);
            il.Emit(OpCodes.Ldarg, indices[i] + offset);
            il.Emit(OpCodes.Stfld, field);
        }
    }

    /// <summary>Emits IL that calls the generic rewire dispatch method and returns its Harmony prefix result.</summary>
    private static void EmitRewireMethodCall(ILGenerator il, Type rewireType, bool isVoid, Type returnType)
    {
        var method = rewireType.GetMethod(isVoid ? "Void" : "Method", BindingFlags.Static | BindingFlags.NonPublic)!;

        if (!isVoid)
        {
            var resultType = returnType.IsByRef
                ? NormalizeType(returnType.GetElementType()!)
                : NormalizeType(returnType);
            method = method.MakeGenericMethod(resultType);
        }

        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ret);
    }
}
