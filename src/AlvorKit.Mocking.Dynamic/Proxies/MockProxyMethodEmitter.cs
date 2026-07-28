namespace AlvorKit.Mocking;

/// <summary>
/// Emits one non-generic proxy method's exact metadata cache and direct body.
/// </summary>
internal static class MockProxyMethodEmitter
{
    private static int nextCacheId;

    /// <summary>Emits one direct non-generic proxy dispatch artifact.</summary>
    internal static TypeBuilder Emit(
        ModuleBuilder module,
        TypeBuilder proxyType,
        MethodBuilder proxyMethod,
        MethodInfo source,
        Type returnType,
        MockIlParameter[] parameters)
    {
        TypeBuilder cache = module.DefineType(
            $"ProxyMethodCache_{Interlocked.Increment(ref nextCacheId)}",
            TypeAttributes.NotPublic |
            TypeAttributes.Abstract |
            TypeAttributes.Sealed);
        FieldBuilder method = cache.DefineField(
            "Method",
            typeof(MethodInfo),
            FieldAttributes.Assembly |
            FieldAttributes.Static |
            FieldAttributes.InitOnly);
        Type? callbackType =
            CanUseTypedCallback(returnType)
                ? MockTypedCallbackDelegateCache.GetOrCreate(source)
                : null;
        MethodBuilder prefix = MockProxyDispatchEmitter.DefinePrefix(
            cache,
            source,
            returnType,
            parameters,
            callbackType);
        MethodBuilder? finalizer =
            MockManagedReferenceAbi.IsSupported(returnType)
                ? MockProxyDispatchEmitter.DefineFinalizer(cache, source)
                : null;
        FieldBuilder? resultStorage =
            DefineResultStorage(proxyType, source, returnType);
        EmitInitializer(cache, proxyMethod, method);
        MockProxyMethodBodyIl.Emit(
            proxyMethod.GetILGenerator(),
            method,
            prefix,
            finalizer,
            resultStorage,
            returnType,
            parameters);
        return cache;
    }

    private static void EmitInitializer(
        TypeBuilder cache,
        MethodBuilder proxyMethod,
        FieldBuilder method)
    {
        ConstructorBuilder initializer = cache.DefineTypeInitializer();
        ILGenerator il = initializer.GetILGenerator();
        il.Emit(OpCodes.Ldtoken, proxyMethod);
        il.Emit(
            OpCodes.Call,
            typeof(MethodBase).GetMethod(
                nameof(MethodBase.GetMethodFromHandle),
                [typeof(RuntimeMethodHandle)])!);
        il.Emit(OpCodes.Castclass, typeof(MethodInfo));
        il.Emit(OpCodes.Stsfld, method);
        il.Emit(OpCodes.Ret);
    }

    private static bool CanUseTypedCallback(Type returnType) =>
        !returnType.IsByRef &&
        !returnType.IsPointer &&
        !returnType.IsFunctionPointer;

    private static FieldBuilder? DefineResultStorage(
        TypeBuilder proxyType,
        MethodInfo source,
        Type returnType)
    {
        if (!returnType.IsByRef ||
            MockManagedReferenceAbi.IsSupported(returnType))
        {
            return null;
        }

        Type elementType = returnType.GetElementType()!;
        if (elementType.IsByRefLike)
            return null;

        return proxyType.DefineField(
            $"__direct_ref_{source.Name}_{Guid.NewGuid():N}",
            elementType,
            FieldAttributes.Private);
    }
}
