namespace AlvorKit.Mocking;

internal static partial class ProxyTypeBuilder
{
    /// <summary>Defines proxy methods for virtual, abstract, and interface members.</summary>
    private static void ImplementVirtualOrAbstractMethods(Type baseType, TypeBuilder typeBuilder)
    {
        var methods = baseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.DeclaringType != typeof(object))
            .Where(m => m.IsAbstract || (m.IsVirtual && !m.IsFinal) || baseType.IsInterface)
            .Distinct()
            .ToList();

        if (baseType.IsInterface)
            methods.AddRange(baseType.GetInterfaces().SelectMany(i => i.GetMethods(BindingFlags.Public | BindingFlags.Instance)));

        foreach (var method in methods)
            DefineMethod(typeBuilder, method);
    }

    /// <summary>Defines one proxy method and maps it to the inherited or interface method.</summary>
    private static void DefineMethod(TypeBuilder typeBuilder, MethodInfo method)
    {
        var parameters = method.GetParameters();
        var paramTypes = parameters.Select(p => p.ParameterType).ToArray();
        var returnType = method.ReturnType;
        var isRefReturn = returnType.IsByRef;
        var actualReturnType = isRefReturn ? returnType.GetElementType()! : returnType;

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

        DefineGenericParameters(method, methodBuilder);
        DefineParameters(methodBuilder, parameters);
        EmitDefaultMethodBody(typeBuilder, methodBuilder, method, isRefReturn, actualReturnType, returnType);

        var baseMethod = method;
        if (method.IsGenericMethodDefinition)
            baseMethod = baseMethod.MakeGenericMethod(method.GetGenericArguments());
        typeBuilder.DefineMethodOverride(methodBuilder, baseMethod);
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
