namespace AlvorKit.Mocking;

internal static partial class ProxyTypeBuilder
{
    /// <summary>Returns the virtual, abstract, and interface methods represented by a proxy.</summary>
    private static List<MethodInfo> GetVirtualOrAbstractMethods(Type baseType)
    {
        var methods = baseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.DeclaringType != typeof(object))
            .Where(m => m.IsAbstract || (m.IsVirtual && !m.IsFinal) || baseType.IsInterface)
            .Distinct()
            .ToList();

        if (baseType.IsInterface)
            methods.AddRange(baseType.GetInterfaces().SelectMany(i => i.GetMethods(BindingFlags.Public | BindingFlags.Instance)));

        return methods;
    }

    /// <summary>Defines validated proxy methods for virtual, abstract, and interface members.</summary>
    private static void ImplementVirtualOrAbstractMethods(
        TypeBuilder typeBuilder,
        List<MethodInfo> methods,
        List<TypeBuilder> dispatchCaches,
        ModuleBuilder module)
    {
        foreach (var method in methods)
            DefineMethod(
                typeBuilder,
                method,
                dispatchCaches,
                module);
    }

    /// <summary>Defines one proxy method and maps it to the inherited or interface method.</summary>
    private static void DefineMethod(
        TypeBuilder typeBuilder,
        MethodInfo method,
        List<TypeBuilder> dispatchCaches,
        ModuleBuilder module)
    {
        if (method.IsGenericMethodDefinition)
        {
            dispatchCaches.Add(
                DefineGenericMethod(
                    module,
                    typeBuilder,
                    method));
            return;
        }

        var parameters = method.GetParameters();
        var paramTypes = parameters.Select(p => p.ParameterType).ToArray();
        var returnType = method.ReturnType;
        var methodBuilder = typeBuilder.DefineMethod(
            method.Name,
            MethodAttributes.Public | MethodAttributes.Virtual,
            CallingConventions.HasThis,
            returnType,
            MethodReturnRequiredModifiers(method),
            MethodReturnOptionalModifiers(method),
            paramTypes,
            [.. parameters.Select(p => p.GetRequiredCustomModifiers())],
            [.. parameters.Select(p => p.GetOptionalCustomModifiers())]);

        DefineReturnParameter(methodBuilder, method.ReturnParameter);
        DefineParameters(methodBuilder, parameters);
        dispatchCaches.Add(
            MockProxyMethodEmitter.Emit(
                module,
                typeBuilder,
                methodBuilder,
                method,
                returnType,
                MockIlParameter.Create(parameters)));

        typeBuilder.DefineMethodOverride(methodBuilder, method);
    }

    /// <summary>Returns required return custom modifiers or <see langword="null"/> when none are present.</summary>
    private static Type[]? MethodReturnRequiredModifiers(MethodInfo method)
    {
        var modifiers = method.ReturnParameter.GetRequiredCustomModifiers();
        return modifiers.Length > 0 ? modifiers : null;
    }

    /// <summary>Returns optional return custom modifiers or <see langword="null"/> when none are present.</summary>
    private static Type[]? MethodReturnOptionalModifiers(MethodInfo method)
    {
        var modifiers = method.ReturnParameter.GetOptionalCustomModifiers();
        return modifiers.Length > 0 ? modifiers : null;
    }
}
