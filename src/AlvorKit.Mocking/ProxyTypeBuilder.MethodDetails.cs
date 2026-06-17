namespace AlvorKit.Mocking;

internal static partial class ProxyTypeBuilder
{
    /// <summary>Copies generic parameter constraints from an inherited generic method definition.</summary>
    private static void DefineGenericParameters(MethodInfo method, MethodBuilder methodBuilder)
    {
        if (!method.IsGenericMethodDefinition)
            return;

        var genericArgs = method.GetGenericArguments();
        var genericBuilders = methodBuilder.DefineGenericParameters([.. genericArgs.Select(g => g.Name)]);

        for (int i = 0; i < genericBuilders.Length; i++)
        {
            var originalArg = genericArgs[i];
            var builder = genericBuilders[i];

            builder.SetGenericParameterAttributes(originalArg.GenericParameterAttributes);

            var constraints = originalArg.GetGenericParameterConstraints();
            var interfaceConstraints = constraints.Where(c => c.IsInterface).ToArray();
            var baseConstraint = constraints.FirstOrDefault(c => c.IsClass);

            if (baseConstraint != null)
                builder.SetBaseTypeConstraint(baseConstraint);
            if (interfaceConstraints.Length > 0)
                builder.SetInterfaceConstraints(interfaceConstraints);
        }
    }

    /// <summary>Defines proxy method parameters and preserves by-reference direction attributes.</summary>
    private static void DefineParameters(MethodBuilder methodBuilder, ParameterInfo[] parameters)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var attrs = ParameterAttributes.None;

            if (param.ParameterType.IsByRef)
                attrs = param.IsOut && !param.IsIn ? ParameterAttributes.Out : ParameterAttributes.In;

            methodBuilder.DefineParameter(i + 1, attrs, param.Name);
        }
    }

    /// <summary>Emits a placeholder method body that Harmony later intercepts before the body runs.</summary>
    private static void EmitDefaultMethodBody(
        TypeBuilder typeBuilder,
        MethodBuilder methodBuilder,
        MethodInfo method,
        bool isRefReturn,
        Type actualReturnType,
        Type returnType)
    {
        var il = methodBuilder.GetILGenerator();

        if (isRefReturn)
            EmitRefReturn(typeBuilder, method, actualReturnType, il);
        else if (returnType != typeof(void))
            EmitDefaultValue(actualReturnType, il);

        il.Emit(OpCodes.Ret);
    }

    /// <summary>Emits a ref return backed by a private field, or throws for ref-struct returns.</summary>
    private static void EmitRefReturn(TypeBuilder typeBuilder, MethodInfo method, Type actualReturnType, ILGenerator il)
    {
        if (actualReturnType.IsByRefLike)
        {
            var exceptionConstructor = typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!;
            il.Emit(OpCodes.Newobj, exceptionConstructor);
            il.Emit(OpCodes.Throw);
            return;
        }

        string fieldName = $"__ref_{method.Name}_{Guid.NewGuid()}";
        var field = typeBuilder.DefineField(fieldName, actualReturnType, FieldAttributes.Private);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldflda, field);
    }

    /// <summary>Emits the default value for a non-void, non-ref return.</summary>
    private static void EmitDefaultValue(Type actualReturnType, ILGenerator il)
    {
        if (actualReturnType.IsValueType)
        {
            var local = il.DeclareLocal(actualReturnType);
            il.Emit(OpCodes.Ldloca_S, local);
            il.Emit(OpCodes.Initobj, actualReturnType);
            il.Emit(OpCodes.Ldloc_0);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }
    }
}
