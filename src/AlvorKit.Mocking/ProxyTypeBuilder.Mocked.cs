namespace AlvorKit.Mocking;

internal static partial class ProxyTypeBuilder
{
    /// <summary>Defines the private field and interface property used to attach mock state to a generated proxy.</summary>
    private static void DefineMockedProperty(TypeBuilder typeBuilder)
    {
        var mockedField = typeBuilder.DefineField($"__mocked_{Guid.NewGuid()}", typeof(Mocked), FieldAttributes.Private);

        var mockedProp = typeof(IMock).GetProperty(nameof(IMock.__Mocked_cc6d2cf7))!;
        var getMocked = mockedProp.GetGetMethod()!;
        var setMocked = mockedProp.GetSetMethod()!;

        var getterBuilder = typeBuilder.DefineMethod(
            getMocked.Name,
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            typeof(Mocked),
            Type.EmptyTypes);

        var ilGetter = getterBuilder.GetILGenerator();
        ilGetter.Emit(OpCodes.Ldarg_0);
        ilGetter.Emit(OpCodes.Ldfld, mockedField);
        ilGetter.Emit(OpCodes.Ret);

        var setterBuilder = typeBuilder.DefineMethod(
            setMocked.Name,
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            null,
            [typeof(Mocked)]);

        var ilSetter = setterBuilder.GetILGenerator();
        ilSetter.Emit(OpCodes.Ldarg_0);
        ilSetter.Emit(OpCodes.Ldarg_1);
        ilSetter.Emit(OpCodes.Stfld, mockedField);
        ilSetter.Emit(OpCodes.Ret);

        var propertyBuilder = typeBuilder.DefineProperty(mockedProp.Name, PropertyAttributes.None, typeof(Mocked), null);
        propertyBuilder.SetGetMethod(getterBuilder);
        propertyBuilder.SetSetMethod(setterBuilder);

        typeBuilder.DefineMethodOverride(getterBuilder, getMocked);
        typeBuilder.DefineMethodOverride(setterBuilder, setMocked);
    }
}
