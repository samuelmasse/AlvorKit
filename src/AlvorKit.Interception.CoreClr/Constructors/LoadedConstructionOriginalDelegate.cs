namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Emits one exact original <c>newobj</c> operation as a managed delegate.</summary>
public static class LoadedConstructionOriginalDelegate
{
    /// <summary>Creates a constructor-argument delegate returning the exact allocated type.</summary>
    public static TDelegate Create<TDelegate>(
        ConstructorInfo constructor)
        where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(constructor);
        Type declaringType = constructor.DeclaringType ??
            throw new ArgumentException(
                "A constructor must have a declaring type.",
                nameof(constructor));
        if (constructor.IsStatic)
        {
            throw new NotSupportedException(
                "Static constructors are not newobj construction operations.");
        }
        if (declaringType.ContainsGenericParameters ||
            constructor.ContainsGenericParameters)
        {
            throw new NotSupportedException(
                "Open construction signatures cannot be emitted.");
        }

        MethodInfo invoke = typeof(TDelegate).GetMethod(
            nameof(Action.Invoke))!;
        ParameterInfo[] constructorParameters =
            constructor.GetParameters();
        ParameterInfo[] delegateParameters = invoke.GetParameters();
        if (invoke.ReturnType != declaringType ||
            delegateParameters.Length != constructorParameters.Length)
        {
            throw new ArgumentException(
                "The construction delegate must return the exact allocated " +
                "type and accept every declared constructor argument.",
                nameof(TDelegate));
        }
        for (var index = 0;
            index < constructorParameters.Length;
            ++index)
        {
            ParameterInfo expected = constructorParameters[index];
            ParameterInfo actual = delegateParameters[index];
            if (expected.ParameterType != actual.ParameterType ||
                expected.IsIn != actual.IsIn ||
                expected.IsOut != actual.IsOut ||
                HasModifiers(expected) ||
                HasModifiers(actual))
            {
                throw new NotSupportedException(
                    "Construction arguments with mismatched direction or " +
                    "custom modifiers are not yet supported.");
            }
        }

        Type[] parameterTypes =
        [
            .. constructorParameters
                .Select(parameter => parameter.ParameterType)
        ];
        var method = new DynamicMethod(
            $"{declaringType.Name}_OriginalConstruction",
            declaringType,
            parameterTypes,
            constructor.Module,
            true);
        ILGenerator il = method.GetILGenerator();
        for (var index = 0;
            index < constructorParameters.Length;
            ++index)
        {
            il.Emit(OpCodes.Ldarg, index);
        }
        il.Emit(OpCodes.Newobj, constructor);
        il.Emit(OpCodes.Ret);
        return (TDelegate)method.CreateDelegate(typeof(TDelegate));
    }

    private static bool HasModifiers(ParameterInfo parameter) =>
        parameter.GetRequiredCustomModifiers().Length != 0 ||
        parameter.GetOptionalCustomModifiers().Length != 0;
}
