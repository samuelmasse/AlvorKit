namespace AlvorKit.Interception;

/// <summary>Owns one reviewed exact target signature used by emitted handler types.</summary>
internal sealed record InterceptionExactSignature(
    Type ReturnType,
    Type[] ReturnRequiredModifiers,
    Type[] ReturnOptionalModifiers,
    Type[] ParameterTypes,
    Type[][] ParameterRequiredModifiers,
    Type[][] ParameterOptionalModifiers,
    ParameterAttributes[] ParameterAttributes,
    string[] ParameterNames)
{
    private static readonly ParameterInfo ReadOnlyReceiverMetadata =
        typeof(InterceptionExactSignature).GetMethod(
            nameof(ReadOnlyReceiver),
            BindingFlags.NonPublic | BindingFlags.Static)!
        .GetParameters()[0];

    /// <summary>Creates and validates the supported exact shape of one target method.</summary>
    internal static InterceptionExactSignature Create(MethodInfo target)
    {
        _ = InterceptionTarget.FromMethod(target);
        return Create(InterceptionCallShape.FromMethod(target));
    }

    /// <summary>Creates and validates one explicitly reviewed exact call shape.</summary>
    internal static InterceptionExactSignature Create(
        InterceptionCallShape callShape)
    {
        ArgumentNullException.ThrowIfNull(callShape);
        MethodInfo target = callShape.Operation;
        if ((target.CallingConvention & CallingConventions.VarArgs) != 0)
        {
            throw new NotSupportedException(
                "Varargs call shapes are unsupported.");
        }
        ValidateConstructedGenericContext(target);
        if (target.ReturnType.IsByRef)
        {
            var elementType = target.ReturnType.GetElementType()!;
            if (elementType.IsByRefLike ||
                elementType.IsPointer ||
                elementType.IsFunctionPointer ||
                elementType.ContainsGenericParameters)
            {
                throw new NotSupportedException(
                    "Managed-reference returns to ref-struct, pointer, " +
                    "function-pointer, or open element types are unsupported.");
            }
        }
        var declared = target.GetParameters();
        var receiverCount =
            callShape.ReceiverOwnership ==
                InterceptionReceiverOwnership.None
                ? 0
                : 1;
        var types = new Type[declared.Length + receiverCount];
        var required = new Type[types.Length][];
        var optional = new Type[types.Length][];
        var attributes = new ParameterAttributes[types.Length];
        var names = new string[types.Length];
        if (receiverCount != 0)
        {
            bool managedReference =
                callShape.ReceiverOwnership is
                    InterceptionReceiverOwnership.ManagedReference or
                    InterceptionReceiverOwnership.ReadOnlyManagedReference;
            types[0] = managedReference
                ? callShape.ReceiverType!.MakeByRefType()
                : callShape.ReceiverType!;
            bool readOnly =
                callShape.ReceiverOwnership ==
                InterceptionReceiverOwnership.ReadOnlyManagedReference;
            required[0] = readOnly
                ? ReadOnlyReceiverMetadata.GetRequiredCustomModifiers()
                : [];
            optional[0] = readOnly
                ? ReadOnlyReceiverMetadata.GetOptionalCustomModifiers()
                : [];
            attributes[0] = readOnly
                ? ReadOnlyReceiverMetadata.Attributes
                : System.Reflection.ParameterAttributes.None;
            names[0] = "receiver";
        }
        for (var index = 0; index < declared.Length; index++)
        {
            var destination = index + receiverCount;
            types[destination] = declared[index].ParameterType;
            required[destination] =
                declared[index].GetRequiredCustomModifiers();
            optional[destination] =
                declared[index].GetOptionalCustomModifiers();
            attributes[destination] = declared[index].Attributes;
            names[destination] =
                declared[index].Name ?? $"argument{index}";
        }

        return new(
            target.ReturnType,
            target.ReturnParameter.GetRequiredCustomModifiers(),
            target.ReturnParameter.GetOptionalCustomModifiers(),
            types,
            required,
            optional,
            attributes,
            names);
    }

    private static void ReadOnlyReceiver(in int receiver) =>
        _ = receiver;

    /// <summary>
    /// Accepts only closed generic contexts whose construction arguments have
    /// ordinary runtime representation. Native code-version correlation remains
    /// the caller's separate responsibility.
    /// </summary>
    private static void ValidateConstructedGenericContext(
        MethodInfo target)
    {
        if (!target.IsGenericMethod &&
            target.DeclaringType?.IsGenericType != true)
        {
            return;
        }

        if (target.ContainsGenericParameters ||
            target.DeclaringType?.ContainsGenericParameters == true)
        {
            throw new NotSupportedException(
                "Exact generic trampoline targets must be fully closed.");
        }

        if (target.DeclaringType?.IsGenericType == true)
        {
            foreach (var argument in
                target.DeclaringType.GetGenericArguments())
            {
                ValidateConstructionArgument(argument);
            }
        }

        if (target.IsGenericMethod)
        {
            foreach (var argument in target.GetGenericArguments())
                ValidateConstructionArgument(argument);
        }
    }

    /// <summary>Rejects construction arguments that need a distinct unsafe ABI proof.</summary>
    private static void ValidateConstructionArgument(Type argument)
    {
        if (argument.ContainsGenericParameters ||
            argument.IsByRefLike ||
            argument.IsByRef ||
            argument.IsPointer ||
            argument.IsFunctionPointer)
        {
            throw new NotSupportedException(
                $"Constructed generic argument '{argument}' has an " +
                "unsupported exact trampoline shape.");
        }

        if (argument.HasElementType)
        {
            ValidateConstructionArgument(argument.GetElementType()!);
            return;
        }

        if (!argument.IsGenericType)
            return;

        foreach (var nested in argument.GetGenericArguments())
            ValidateConstructionArgument(nested);
    }

    /// <summary>Rejects a submitted handler whose exact signature differs from the target.</summary>
    internal void ValidateHandler(
        object? handlerInstance,
        MethodInfo handlerMethod)
    {
        if (!handlerMethod.IsStatic && handlerInstance is null)
            throw new ArgumentNullException(nameof(handlerInstance));
        if (handlerMethod.ContainsGenericParameters)
            throw new NotSupportedException("The handler method must be closed.");
        if (handlerMethod.ReturnType != ReturnType)
        {
            throw new ArgumentException(
                "The handler return type does not exactly match the target.",
                nameof(handlerMethod));
        }
        if (!handlerMethod.ReturnParameter
                .GetRequiredCustomModifiers()
                .SequenceEqual(ReturnRequiredModifiers) ||
            !handlerMethod.ReturnParameter
                .GetOptionalCustomModifiers()
                .SequenceEqual(ReturnOptionalModifiers))
        {
            throw new ArgumentException(
                "The handler return modifiers do not exactly match the target.",
                nameof(handlerMethod));
        }

        var actual = handlerMethod.GetParameters();
        if (actual.Length != ParameterTypes.Length)
        {
            throw new ArgumentException(
                "The handler parameter count does not exactly match the target.",
                nameof(handlerMethod));
        }
        for (var index = 0; index < actual.Length; index++)
        {
            if (actual[index].ParameterType != ParameterTypes[index] ||
                (actual[index].Attributes &
                    (System.Reflection.ParameterAttributes.In |
                     System.Reflection.ParameterAttributes.Out)) !=
                (ParameterAttributes[index] &
                    (System.Reflection.ParameterAttributes.In |
                     System.Reflection.ParameterAttributes.Out)) ||
                !actual[index].GetRequiredCustomModifiers().SequenceEqual(
                    ParameterRequiredModifiers[index]) ||
                !actual[index].GetOptionalCustomModifiers().SequenceEqual(
                    ParameterOptionalModifiers[index]))
            {
                throw new ArgumentException(
                    $"Handler parameter {index} does not exactly match " +
                    $"'{ParameterTypes[index]}'.",
                    nameof(handlerMethod));
            }
        }
    }
}
