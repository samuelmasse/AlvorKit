namespace AlvorKit;

/// <summary>
/// Adapts a receiver-only constructor callback to the exact
/// receiver-plus-arguments delegate shape.
/// </summary>
internal static class MockConstructorCallbackAdapter
{
    /// <summary>Normalizes an exact callback or emits a receiver-only adapter.</summary>
    internal static Delegate Normalize(
        Delegate callback,
        MethodInfo logicalMethod)
    {
        MethodInfo invoke = callback.GetType().GetMethod(
            nameof(Action.Invoke))!;
        ParameterInfo[] source = invoke.GetParameters();
        ParameterInfo[] target = logicalMethod.GetParameters();
        if (source.Length != 1 ||
            target.Length == 0 ||
            invoke.ReturnType != typeof(void) ||
            source[0].ParameterType != target[0].ParameterType)
        {
            return MockTypedCallbackContract.Normalize(
                callback,
                logicalMethod);
        }

        Type stableType =
            MockTypedCallbackDelegateCache.GetOrCreate(
                logicalMethod);
        var parameterTypes = new Type[target.Length + 1];
        parameterTypes[0] = callback.GetType();
        for (int index = 0; index < target.Length; index++)
            parameterTypes[index + 1] = target[index].ParameterType;
        var adapter = new DynamicMethod(
            $"ConstructorCallback_{Guid.NewGuid():N}",
            typeof(void),
            parameterTypes,
            typeof(MockConstructorCallbackAdapter).Module,
            skipVisibility: true);
        ILGenerator il = adapter.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Callvirt, invoke);
        il.Emit(OpCodes.Ret);
        return adapter.CreateDelegate(
            stableType,
            callback);
    }
}
