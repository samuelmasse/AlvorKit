namespace AlvorKit.Mocking;

/// <summary>Emits runtime proxy types used for interface and inheritable class mocks.</summary>
internal static partial class ProxyTypeBuilder
{
    /// <summary>Generated proxy assembly identity.</summary>
    private static AssemblyName? assemblyName;

    /// <summary>Generated proxy assembly builder.</summary>
    private static AssemblyBuilder? assemblyBuilder;

    /// <summary>Generated proxy module builder.</summary>
    private static ModuleBuilder? moduleBuilder;

    /// <summary>Creates a proxy type for one mockable interface or class.</summary>
    internal static Type CreateType(Type baseType)
    {
        TypeBuilder typeBuilder = GetModuleBuilder().DefineType(
            name: $"{baseType.Name}_Proxy_{Guid.NewGuid()}",
            attr: TypeAttributes.Public | TypeAttributes.Class,
            parent: baseType.IsClass ? baseType : null,
            interfaces: baseType.IsInterface ? [typeof(IMock), baseType] : [typeof(IMock)]);

        DefineMockedProperty(typeBuilder);
        ImplementVirtualOrAbstractMethods(baseType, typeBuilder);
        DefineConstructors(baseType, typeBuilder);

        return typeBuilder.CreateType();
    }

    /// <summary>Returns the shared proxy module, creating the dynamic assembly on first use.</summary>
    private static ModuleBuilder GetModuleBuilder()
    {
        assemblyName ??= new("AlvorKit.Mocking.Proxies");
        assemblyBuilder ??= AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        moduleBuilder ??= assemblyBuilder.DefineDynamicModule("Proxies");

        return moduleBuilder;
    }
}
