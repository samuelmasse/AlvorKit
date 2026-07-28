using System.Buffers.Binary;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Lowers one recognized loaded <c>newobj</c> site to an exact static route.</summary>
public static class LoadedConstructionCallerComposer
{
    private const ushort NewObject = 0x0073;
    private const byte Call = 0x28;

    /// <summary>
    /// Rewrites only the selected construction opcode in the authoritative caller body.
    /// </summary>
    public static InterceptionGenerationPlan Compose(
        MethodInfo caller,
        LoadedMethodBodySnapshot body,
        LoadedOperationSiteDescriptor site,
        ConstructorInfo constructor,
        MethodInfo route,
        ulong generationId,
        ulong priorGenerationId = 0)
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(site);
        ArgumentNullException.ThrowIfNull(constructor);
        ArgumentNullException.ThrowIfNull(route);
        if (caller.IsGenericMethod ||
            caller.DeclaringType?.IsGenericType == true)
        {
            throw new NotSupportedException(
                "Construction lowering cannot target a generic caller " +
                "definition until constructed code versions are correlated.");
        }

        InterceptionTarget target = InterceptionTarget.FromMethod(caller);
        ValidateSite(caller, body, site, constructor);
        ValidateRoute(caller, constructor, route);

        byte[] generated = [.. body.Bytes];
        int operationOffset = checked(
            body.HeaderSize + site.BaselineOffset);
        generated[operationOffset] = Call;
        BinaryPrimitives.WriteInt32LittleEndian(
            generated.AsSpan(operationOffset + 1),
            route.MetadataToken);

        return new(
            target,
            InterceptionMethodBody.FromRaw(generated),
            body.Identity,
            generationId,
            priorGenerationId,
            [],
            body.Instructions.Select(instruction =>
                new InterceptionGenerationIlMapEntry(
                    checked((uint)instruction.BaselineOffset),
                    checked((uint)instruction.BaselineOffset))));
    }

    private static void ValidateSite(
        MethodInfo caller,
        LoadedMethodBodySnapshot body,
        LoadedOperationSiteDescriptor site,
        ConstructorInfo constructor)
    {
        if (!body.Identity.Equals(site.BodyIdentity))
        {
            throw new ArgumentException(
                "The selected construction site does not belong to the " +
                "authoritative loaded body.",
                nameof(site));
        }
        if (site.ModuleVersionId != caller.Module.ModuleVersionId ||
            site.ContainingMethodToken != caller.MetadataToken)
        {
            throw new ArgumentException(
                "The selected construction site does not belong to the caller.",
                nameof(site));
        }
        if (site.Kind != LoadedOperationKind.ObjectConstruction ||
            site.OpCodeValue != NewObject ||
            !site.Prefixes.IsEmpty)
        {
            throw new ArgumentException(
                "The selected site must be one unprefixed newobj operation.",
                nameof(site));
        }

        var resolver =
            new ReflectionLoadedOperationMetadataResolver(caller);
        if (!string.Equals(
                site.ConstructedContext,
                resolver.ConstructedContext,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The selected construction site has a different constructed " +
                "caller context.",
                nameof(site));
        }

        LoadedOperationRecognition recognition =
            LoadedOperationRecognizer.Recognize(
                body,
                caller.Module.ModuleVersionId,
                caller.MetadataToken,
                resolver,
                resolver.ConstructedContext);
        if (!recognition.IsSuccessful)
        {
            throw new ArgumentException(
                "The authoritative caller body no longer has a pristine " +
                "recognized operation set.",
                nameof(body));
        }

        LoadedOperationSiteDescriptor[] matches =
        [
            .. recognition.Sites.Where(candidate =>
                string.Equals(
                    candidate.StableId,
                    site.StableId,
                    StringComparison.Ordinal))
        ];
        if (matches.Length != 1 ||
            matches[0].BaselineOffset != site.BaselineOffset ||
            matches[0].MetadataToken != site.MetadataToken ||
            !string.Equals(
                matches[0].CanonicalSignature,
                site.CanonicalSignature,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The selected construction site is not unique in the " +
                "authoritative caller body.",
                nameof(site));
        }

        MethodBase? resolved;
        try
        {
            resolved = caller.Module.ResolveMethod(
                site.MetadataToken,
                caller.DeclaringType?.GetGenericArguments(),
                caller.GetGenericArguments());
        }
        catch (Exception exception) when (
            exception is ArgumentException or
                BadImageFormatException or
                NotSupportedException)
        {
            throw new ArgumentException(
                "The selected newobj metadata token could not be resolved.",
                nameof(site),
                exception);
        }
        if (resolved is not ConstructorInfo actual ||
            !SameConstructor(actual, constructor))
        {
            throw new ArgumentException(
                "The selected newobj metadata token does not name the " +
                "supplied constructor.",
                nameof(constructor));
        }
    }

    private static void ValidateRoute(
        MethodInfo caller,
        ConstructorInfo constructor,
        MethodInfo route)
    {
        Type declaringType = constructor.DeclaringType ??
            throw new ArgumentException(
                "A construction constructor must have a declaring type.",
                nameof(constructor));
        if (constructor.IsStatic ||
            constructor.ContainsGenericParameters ||
            declaringType.ContainsGenericParameters)
        {
            throw new NotSupportedException(
                "Construction lowering requires a closed instance constructor.");
        }

        ParameterInfo[] expected = constructor.GetParameters();
        ParameterInfo[] actual = route.GetParameters();
        if (!route.IsStatic ||
            route.IsGenericMethod ||
            route.DeclaringType?.IsGenericType == true ||
            route is DynamicMethod ||
            route.Module != caller.Module ||
            (route.CallingConvention & CallingConventions.VarArgs) != 0 ||
            (route.MetadataToken & unchecked((int)0xFF000000)) !=
                0x06000000 ||
            route.ReturnType != declaringType ||
            route.ReturnParameter.GetRequiredCustomModifiers().Length != 0 ||
            route.ReturnParameter.GetOptionalCustomModifiers().Length != 0 ||
            actual.Length != expected.Length)
        {
            throw new ArgumentException(
                "The construction route must be a same-module nongeneric " +
                "static MethodDef returning the exact constructed type and " +
                "accepting every constructor argument.",
                nameof(route));
        }
        for (int index = 0; index < expected.Length; ++index)
        {
            if (!SameParameter(expected[index], actual[index]))
            {
                throw new ArgumentException(
                    $"Construction route parameter {index} does not preserve " +
                    "the constructor's exact type, direction, or custom modifiers.",
                    nameof(route));
            }
        }
    }

    private static bool SameConstructor(
        ConstructorInfo left,
        ConstructorInfo right) =>
        left.Module == right.Module &&
        left.MetadataToken == right.MetadataToken &&
        left.DeclaringType == right.DeclaringType;

    private static bool SameParameter(
        ParameterInfo expected,
        ParameterInfo actual) =>
        expected.ParameterType == actual.ParameterType &&
        expected.IsIn == actual.IsIn &&
        expected.IsOut == actual.IsOut &&
        expected.GetRequiredCustomModifiers().SequenceEqual(
            actual.GetRequiredCustomModifiers()) &&
        expected.GetOptionalCustomModifiers().SequenceEqual(
            actual.GetOptionalCustomModifiers());
}
