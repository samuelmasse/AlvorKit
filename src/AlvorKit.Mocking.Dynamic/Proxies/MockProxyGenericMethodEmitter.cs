namespace AlvorKit.Mocking;

/// <summary>
/// Emits a generic proxy method's exact prefix and constructed-method cache.
/// </summary>
internal static class MockProxyGenericMethodEmitter
{
    private static int nextCacheId;

    /// <summary>Emits one proxy-owned generic dispatch cache and method body.</summary>
    internal static TypeBuilder Emit(
        ModuleBuilder module,
        MethodBuilder proxyMethod,
        MethodInfo source,
        GenericTypeParameterBuilder[] proxyArguments,
        Type proxyReturnType,
        MockIlParameter[] proxyParameters)
    {
        TypeBuilder cache = module.DefineType(
            $"ProxyGenericCache_{Interlocked.Increment(ref nextCacheId)}",
            TypeAttributes.NotPublic
            | TypeAttributes.Abstract
            | TypeAttributes.Sealed);
        Type[] originalArguments = source.GetGenericArguments();
        GenericTypeParameterBuilder[] cacheArguments =
            cache.DefineGenericParameters(
                [.. originalArguments.Select(static argument => argument.Name)]);
        Dictionary<Type, Type> substitutions =
            MockGenericTypeSubstitution.CreateMap(
                originalArguments,
                cacheArguments);
        MockGenericTypeSubstitution.CopyConstraints(
            originalArguments,
            cacheArguments,
            substitutions);
        Type cacheReturnType = MockGenericTypeSubstitution.Replace(
            source.ReturnType,
            substitutions);
        MockIlParameter[] cacheParameters = CreateParameters(
            source.GetParameters(),
            substitutions);
        FieldBuilder methodField = cache.DefineField(
            "Method",
            typeof(MethodInfo),
            FieldAttributes.Assembly
            | FieldAttributes.Static
            | FieldAttributes.InitOnly);
        Type? callbackType =
            MockTypedCallbackDelegateShape.Create(
                cacheReturnType,
                cacheParameters,
                source);
        MethodBuilder prefix = MockProxyDispatchEmitter.DefinePrefix(
            cache,
            source,
            cacheReturnType,
            cacheParameters,
            substitutions,
            callbackType);
        EmitInitializer(
            cache,
            proxyMethod,
            cacheArguments,
            methodField);
        EmitProxyBody(
            proxyMethod,
            cache,
            proxyArguments,
            methodField,
            prefix,
            proxyReturnType,
            proxyParameters);
        return cache;
    }

    private static void EmitInitializer(
        TypeBuilder cache,
        MethodBuilder proxyMethod,
        GenericTypeParameterBuilder[] cacheArguments,
        FieldBuilder methodField)
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
        il.Emit(OpCodes.Ldc_I4, cacheArguments.Length);
        il.Emit(OpCodes.Newarr, typeof(Type));
        for (int index = 0; index < cacheArguments.Length; index++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldtoken, cacheArguments[index]);
            il.Emit(
                OpCodes.Call,
                typeof(Type).GetMethod(
                    nameof(Type.GetTypeFromHandle))!);
            il.Emit(OpCodes.Stelem_Ref);
        }

        il.Emit(
            OpCodes.Callvirt,
            typeof(MethodInfo).GetMethod(
                nameof(MethodInfo.MakeGenericMethod))!);
        il.Emit(OpCodes.Stsfld, methodField);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitProxyBody(
        MethodBuilder proxyMethod,
        TypeBuilder cache,
        GenericTypeParameterBuilder[] proxyArguments,
        FieldBuilder methodField,
        MethodBuilder prefix,
        Type returnType,
        MockIlParameter[] parameters)
    {
        Type constructedCache = cache.MakeGenericType(proxyArguments);
        FieldInfo constructedMethod = TypeBuilder.GetField(
            constructedCache,
            methodField);
        MethodInfo constructedPrefix = TypeBuilder.GetMethod(
            constructedCache,
            prefix);
        MockProxyMethodBodyIl.Emit(
            proxyMethod.GetILGenerator(),
            constructedMethod,
            constructedPrefix,
            null,
            null,
            returnType,
            parameters);
    }

    private static MockIlParameter[] CreateParameters(
        ParameterInfo[] source,
        IReadOnlyDictionary<Type, Type> substitutions)
    {
        var result = new MockIlParameter[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            result[index] = new(
                MockGenericTypeSubstitution.Replace(
                    source[index].ParameterType,
                    substitutions),
                source[index].IsIn,
                source[index].IsOut);
        }

        return result;
    }
}
