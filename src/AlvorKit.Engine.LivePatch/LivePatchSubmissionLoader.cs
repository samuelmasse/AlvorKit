namespace AlvorKit.Engine;

/// <summary>
/// Loads collectible LivePatch submissions and resolves their exact target and
/// constructor dependencies against assemblies already owned by the game.
/// </summary>
internal sealed class LivePatchSubmissionLoader(LivePatchBridgeProtocol protocol)
{
    /// <summary>Loads and validates one compiled handler submission.</summary>
    internal LivePatchLoadedSubmission Load(JsonElement request)
    {
        var entryTypeName = protocol.RequiredString(request, "entryType");
        var assemblyBytes = protocol.RequiredBytes(request, "assembly");
        var symbolsBytes = protocol.OptionalBytes(request, "symbols");
        var loadContext = new LivePatchSubmissionLoadContext();
        try
        {
            using var assemblyStream = new MemoryStream(assemblyBytes, writable: false);
            using var symbolsStream = symbolsBytes is null
                ? null
                : new MemoryStream(symbolsBytes, writable: false);
            var assembly = symbolsStream is null
                ? loadContext.LoadFromStream(assemblyStream)
                : loadContext.LoadFromStream(assemblyStream, symbolsStream);
            var handlerType = assembly.GetType(entryTypeName, throwOnError: false)
                ?? throw new ArgumentException(
                    $"Submitted handler type '{entryTypeName}' was not found.");
            if (handlerType.IsAbstract || handlerType.IsInterface)
                throw new ArgumentException("The submitted handler type must be a concrete class.");

            var methods = handlerType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.DeclaredOnly)
                .Where(static method =>
                    method.IsDefined(typeof(LivePatchHandlerAttribute), inherit: false))
                .ToArray();
            if (methods.Length != 1)
            {
                throw new ArgumentException(
                    $"Submitted type '{entryTypeName}' must declare exactly one " +
                    $"[{nameof(LivePatchHandlerAttribute)}] method; found {methods.Length}.");
            }

            return new(loadContext, entryTypeName, handlerType, methods[0]);
        }
        catch
        {
            loadContext.Unload();
            throw;
        }
    }

    /// <summary>Resolves the one target overload whose exact signature matches the handler.</summary>
    internal MethodInfo ResolveTarget(JsonElement descriptor, MethodInfo handler)
    {
        var assemblyName = protocol.OptionalString(descriptor, "assembly");
        var typeName = protocol.RequiredString(descriptor, "type");
        var methodName = protocol.RequiredString(descriptor, "method");
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic)
            .Where(assembly => assemblyName is null ||
                string.Equals(
                    assembly.GetName().Name,
                    assemblyName,
                    StringComparison.Ordinal))
            .Select(assembly => assembly.GetType(typeName, throwOnError: false))
            .OfType<Type>()
            .ToArray();
        if (types.Length != 1)
        {
            throw new ArgumentException(
                $"Target type '{typeName}' resolved {types.Length} times; provide its exact assembly name.");
        }

        var matches = types[0]
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.DeclaredOnly)
            .Where(method => method.Name == methodName && HandlerMatches(method, handler))
            .ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new ArgumentException(
                $"No overload of '{typeName}.{methodName}' exactly matches handler '{handler}'."),
            _ => throw new ArgumentException(
                $"Handler '{handler}' ambiguously matches {matches.Length} target overloads.")
        };
    }

    /// <summary>Creates a handler using dependencies owned by an active injector scope.</summary>
    internal object? CreateHandler(Type handlerType, InjectorScope executor)
    {
        var constructors = handlerType.GetConstructors();
        if (constructors.Length != 1)
        {
            throw new ArgumentException(
                $"Submitted handler '{handlerType.FullName}' must have exactly one public constructor.");
        }

        var parameters = constructors[0].GetParameters();
        var arguments = new object?[parameters.Length];
        for (var index = 0; index < parameters.Length; index++)
        {
            var dependencyType = parameters[index].ParameterType;
            if (AssemblyLoadContext.GetLoadContext(dependencyType.Assembly)?.IsCollectible == true)
            {
                throw new NotSupportedException(
                    $"Handler constructor dependency '{dependencyType}' is defined in submitted code. " +
                    "Use dependencies already loaded by the game so the submission remains collectible.");
            }
            arguments[index] = executor.Get(dependencyType);
        }

        try
        {
            return constructors[0].Invoke(arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }

    private static bool HandlerMatches(MethodInfo target, MethodInfo handler)
    {
        if (target.ReturnType != handler.ReturnType ||
            !ModifiersMatch(target.ReturnParameter, handler.ReturnParameter))
        {
            return false;
        }

        var targetParameters = target.GetParameters();
        var handlerParameters = handler.GetParameters();
        var receiverCount = target.IsStatic ? 0 : 1;
        if (handlerParameters.Length != targetParameters.Length + receiverCount)
            return false;
        if (!target.IsStatic &&
            handlerParameters[0].ParameterType != target.DeclaringType)
        {
            return false;
        }

        for (var index = 0; index < targetParameters.Length; index++)
        {
            if (targetParameters[index].ParameterType !=
                    handlerParameters[index + receiverCount].ParameterType ||
                !ModifiersMatch(
                    targetParameters[index],
                    handlerParameters[index + receiverCount]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ModifiersMatch(ParameterInfo left, ParameterInfo right) =>
        left.GetRequiredCustomModifiers().SequenceEqual(
            right.GetRequiredCustomModifiers()) &&
        left.GetOptionalCustomModifiers().SequenceEqual(
            right.GetOptionalCustomModifiers());
}

/// <summary>Contains one validated collectible handler submission.</summary>
internal sealed record LivePatchLoadedSubmission(
    LivePatchSubmissionLoadContext Context,
    string EntryType,
    Type HandlerType,
    MethodInfo HandlerMethod);

/// <summary>Tracks an installed handler and the collectible context that owns it.</summary>
internal sealed record LivePatchSubmittedPatch(
    LivePatchLease Lease,
    LivePatchSubmissionLoadContext Context,
    WeakReference ContextReference,
    InjectorScopeId ExecutorScopeId,
    string EntryType,
    string HandlerMethod);
