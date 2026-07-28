namespace AlvorKit.Mocking;

/// <summary>Emits runtime proxy types used for interface and inheritable class mocks.</summary>
internal static partial class ProxyTypeBuilder
{
    private static readonly MockBackendIdentity Backend = new(
        MockBackendKind.Proxy,
        2);

    /// <summary>Creates a proxy type for one mockable interface or class.</summary>
    internal static Type CreateType(
        ModuleBuilder module,
        Type baseType)
    {
        List<MethodInfo> methods = GetVirtualOrAbstractMethods(baseType);
        ValidateMethods(methods);
        var dispatchCaches = new List<TypeBuilder>();
        TypeBuilder typeBuilder = module.DefineType(
            name: $"{baseType.Name}_Proxy_{Guid.NewGuid()}",
            attr: TypeAttributes.Public | TypeAttributes.Class,
            parent: baseType.IsClass ? baseType : null,
            interfaces: baseType.IsInterface ? [typeof(IMock), baseType] : [typeof(IMock)]);

        DefineMockedProperty(typeBuilder);
        ImplementVirtualOrAbstractMethods(
            typeBuilder,
            methods,
            dispatchCaches,
            module);
        DefineConstructors(baseType, typeBuilder);

        foreach (TypeBuilder dispatchCache in dispatchCaches)
            dispatchCache.CreateType();
        return typeBuilder.CreateType();
    }

    private static void ValidateMethods(List<MethodInfo> methods)
    {
        foreach (MethodInfo method in methods)
        {
            if (method.IsGenericMethodDefinition
                && method.ReturnType.IsByRef)
            {
                throw new MockException(
                    $"Proxy ABI 2 does not support managed-reference return " +
                    $"'{method.DeclaringType?.FullName}.{method.Name}' on a " +
                    "generic method.");
            }

            if (method.ContainsGenericParameters
                || ReturnsManagedReferenceToRefStruct(method))
                continue;

            MockSignatureValidation validation = MockSignatureValidator.Validate(
                method,
                Backend,
                MockOperationKind.InstanceMethod);
            if (!validation.IsSupported)
                throw new MockException(validation.Rejection!.Message);
        }
    }

    /// <summary>Returns whether the method returns a managed reference to a ref-struct value.</summary>
    private static bool ReturnsManagedReferenceToRefStruct(MethodInfo method) =>
        method.ReturnType.IsByRef
        && method.ReturnType.GetElementType()!.IsByRefLike;
}
