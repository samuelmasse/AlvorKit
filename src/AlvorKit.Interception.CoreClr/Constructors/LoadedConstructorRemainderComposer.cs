namespace AlvorKit;

/// <summary>Lowers a validated loaded constructor split into executable managed artifacts.</summary>
public static class LoadedConstructorRemainderComposer
{
    /// <summary>
    /// Preserves the initializer prefix, routes after initialization, and extracts the original suffix.
    /// </summary>
    public static LoadedConstructorRemainderGeneration Compose(
        ConstructorInfo constructor,
        LoadedMethodBodySnapshot body,
        LoadedConstructorRemainderPlan remainder,
        MethodInfo route,
        Type originalDelegateType,
        ulong generationId,
        ulong priorGenerationId = 0)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(remainder);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(originalDelegateType);
        if (!body.Identity.Equals(remainder.BodyIdentity))
        {
            throw new ArgumentException(
                "The constructor remainder does not belong to the supplied loaded body.",
                nameof(remainder));
        }

        Type declaringType = constructor.DeclaringType ??
            throw new ArgumentException(
                "A constructor must have a declaring type.",
                nameof(constructor));
        InterceptionTarget target =
            InterceptionTarget.FromConstructor(constructor);
        if (constructor.GetParameters().Any(HasModifiers))
        {
            throw new NotSupportedException(
                "Constructor remainder parameters with custom modifiers are not yet supported.");
        }
        Type[] signature = Signature(constructor, declaringType);
        ValidateDelegate(originalDelegateType, signature);
        ValidateRoute(constructor, route, signature);

        Delegate original =
            LoadedConstructorRemainderDelegateEmitter.Emit(
                constructor,
                body,
                remainder,
                originalDelegateType,
                signature);
        InterceptionMethodBody generatedBody =
            LoadedConstructorRemainderMethodBodyEmitter.Emit(
                body,
                remainder,
                route,
                signature.Length);
        var ilMap = remainder.PreservedPrefix.Instructions
            .Select(instruction =>
                new InterceptionGenerationIlMapEntry(
                    ((uint)instruction.BaselineOffset),
                    ((uint)instruction.BaselineOffset)))
            .Append(
                new(
                    ((uint)remainder.MovedRemainder.StartOffset),
                    ((uint)remainder.MovedRemainder.StartOffset),
                    false));
        var plan = new InterceptionGenerationPlan(
            target,
            generatedBody,
            body.Identity,
            generationId,
            priorGenerationId,
            [],
            ilMap);
        return new(plan, original);
    }

    private static Type[] Signature(
        ConstructorInfo constructor,
        Type declaringType) =>
        [
            declaringType,
            .. constructor
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
        ];

    private static void ValidateDelegate(
        Type delegateType,
        Type[] signature)
    {
        if (!typeof(Delegate).IsAssignableFrom(delegateType))
        {
            throw new ArgumentException(
                "The original remainder type must be a delegate.",
                nameof(delegateType));
        }

        MethodInfo invoke = delegateType.GetMethod(nameof(Action.Invoke)) ??
            throw new ArgumentException(
                "The original remainder delegate has no Invoke method.",
                nameof(delegateType));
        if (invoke.ReturnType != typeof(void) ||
            !invoke.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .SequenceEqual(signature))
        {
            throw new ArgumentException(
                "The original remainder delegate must return void and accept " +
                "the exact constructor receiver followed by every declared argument.",
                nameof(delegateType));
        }
        if (invoke.GetParameters().Any(HasModifiers))
        {
            throw new NotSupportedException(
                "Constructor remainder parameters with custom modifiers are not yet supported.");
        }
    }

    private static void ValidateRoute(
        ConstructorInfo constructor,
        MethodInfo route,
        Type[] signature)
    {
        if (!route.IsStatic ||
            route.IsGenericMethod ||
            route.DeclaringType?.IsGenericType == true ||
            route.ReturnType != typeof(void) ||
            route.Module != constructor.Module ||
            (route.MetadataToken & unchecked((int)0xFF000000)) !=
                0x06000000 ||
            !route.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .SequenceEqual(signature))
        {
            throw new ArgumentException(
                "The constructor route must be a same-module static MethodDef " +
                "on a non-generic declaring type, returning void with the " +
                "exact receiver-and-arguments signature.",
                nameof(route));
        }
        if (route.GetParameters().Any(HasModifiers))
        {
            throw new NotSupportedException(
                "Constructor routes with custom modifiers are not yet supported.");
        }
    }

    private static bool HasModifiers(ParameterInfo parameter) =>
        parameter.GetRequiredCustomModifiers().Length != 0 ||
        parameter.GetOptionalCustomModifiers().Length != 0;
}
