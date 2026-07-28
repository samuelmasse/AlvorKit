namespace AlvorKit.Mocking;

internal static partial class ProxyTypeBuilder
{
    /// <summary>Defines forwarding constructors for class proxies.</summary>
    private static void DefineConstructors(Type baseType, TypeBuilder typeBuilder)
    {
        if (!baseType.IsClass)
            return;

        var baseConstructors = baseType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var baseCtor in baseConstructors)
            DefineConstructor(typeBuilder, baseCtor);
    }

    /// <summary>Defines one proxy constructor that forwards all arguments to the base constructor.</summary>
    private static void DefineConstructor(TypeBuilder typeBuilder, ConstructorInfo baseCtor)
    {
        var ctorParams = baseCtor.GetParameters();
        var paramTypes = ctorParams.Select(p => p.ParameterType).ToArray();

        var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes);

        for (int i = 0; i < ctorParams.Length; i++)
            ctorBuilder.DefineParameter(i + 1, ParameterAttributes.None, ctorParams[i].Name);

        var il = ctorBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);

        for (int i = 0; i < paramTypes.Length; i++)
            il.Emit(OpCodes.Ldarg, i + 1);

        il.Emit(OpCodes.Call, baseCtor);
        il.Emit(OpCodes.Ret);
    }
}
