namespace AlvorKit.Interception;

/// <summary>Emits exact delegate and managed-callable trampoline types for reviewed target signatures.</summary>
internal static class InterceptionHandlerTrampolineFactory
{
    private static long nextTypeId;

    /// <summary>
    /// Creates a trampoline for a handler method whose explicit parameters are the target receiver
    /// followed by every declared argument, or only declared arguments for a static target.
    /// Managed-reference returns require <see cref="InterceptionHandlerExceptionPolicy.Propagate"/>.
    /// </summary>
    public static InterceptionHandlerTrampoline Create(
        MethodInfo target,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy =
            InterceptionHandlerExceptionPolicy.Propagate)
    {
        ArgumentNullException.ThrowIfNull(target);
        return Create(
            InterceptionExactSignature.Create(target),
            handlerInstance,
            handlerMethod,
            exceptionPolicy);
    }

    /// <summary>
    /// Creates a trampoline for an explicitly reviewed hidden-receiver call
    /// shape while preserving the operation's declared signature.
    /// </summary>
    public static InterceptionHandlerTrampoline Create(
        InterceptionCallShape callShape,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy =
            InterceptionHandlerExceptionPolicy.Propagate)
    {
        ArgumentNullException.ThrowIfNull(callShape);
        return Create(
            InterceptionExactSignature.Create(callShape),
            handlerInstance,
            handlerMethod,
            exceptionPolicy);
    }

    private static InterceptionHandlerTrampoline Create(
        InterceptionExactSignature signature,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy)
    {
        ArgumentNullException.ThrowIfNull(handlerMethod);
        if (!Enum.IsDefined(exceptionPolicy))
            throw new ArgumentOutOfRangeException(nameof(exceptionPolicy));
        if (exceptionPolicy ==
                InterceptionHandlerExceptionPolicy.ContainAndDeactivate &&
            signature.ReturnType.IsByRef)
        {
            throw new NotSupportedException(
                "Managed-reference returns require the Propagate exception " +
                "policy because no safe default managed reference exists.");
        }
        signature.ValidateHandler(handlerInstance, handlerMethod);

        var id = Interlocked.Increment(ref nextTypeId);
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new($"AlvorKit.Interception.Dynamic.{id}"),
            AssemblyBuilderAccess.RunAndCollect);
        var module = assembly.DefineDynamicModule(
            $"AlvorKit.Interception.Dynamic.{id}");
        var delegateType = EmitDelegate(module, id, signature);
        var handler = handlerMethod.IsStatic
            ? handlerMethod.CreateDelegate(delegateType)
            : handlerMethod.CreateDelegate(delegateType, handlerInstance);
        return EmitTrampoline(
            module,
            id,
            signature,
            delegateType,
            handler,
            exceptionPolicy);
    }

    private static Type EmitDelegate(
        ModuleBuilder module,
        long id,
        InterceptionExactSignature signature)
    {
        var builder = module.DefineType(
            $"AlvorKit.Interception.Dynamic.ExactDelegate{id}",
            TypeAttributes.Public |
            TypeAttributes.Sealed |
            TypeAttributes.Class,
            typeof(MulticastDelegate));
        var constructor = builder.DefineConstructor(
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.RTSpecialName |
            MethodAttributes.SpecialName,
            CallingConventions.Standard,
            [typeof(object), typeof(nint)]);
        constructor.SetImplementationFlags(
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed);
        var invoke = DefineExactMethod(
            builder,
            "Invoke",
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.NewSlot |
            MethodAttributes.Virtual,
            signature);
        invoke.SetImplementationFlags(
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed);
        return builder.CreateType()!;
    }

    private static InterceptionHandlerTrampoline EmitTrampoline(
        ModuleBuilder module,
        long id,
        InterceptionExactSignature signature,
        Type delegateType,
        Delegate handler,
        InterceptionHandlerExceptionPolicy exceptionPolicy)
    {
        var builder = module.DefineType(
            $"AlvorKit.Interception.Dynamic.ExactTrampoline{id}",
            TypeAttributes.Public |
            TypeAttributes.Abstract |
            TypeAttributes.Sealed);
        var handlerField = builder.DefineField(
            "Handler",
            delegateType,
            FieldAttributes.Public |
            FieldAttributes.Static);
        var releaseField = builder.DefineField(
            "Release",
            typeof(Action),
            FieldAttributes.Public |
            FieldAttributes.Static);
        var failField = builder.DefineField(
            "Fail",
            typeof(Action<Exception>),
            FieldAttributes.Public |
            FieldAttributes.Static);
        var invoke = DefineExactMethod(
            builder,
            "Invoke",
            MethodAttributes.Public |
            MethodAttributes.Static |
            MethodAttributes.HideBySig,
            signature);

        var il = invoke.GetILGenerator();
        LocalBuilder? result = null;
        LocalBuilder? exception = exceptionPolicy ==
            InterceptionHandlerExceptionPolicy.ContainAndDeactivate
                ? il.DeclareLocal(typeof(Exception))
                : null;
        if (signature.ReturnType != typeof(void))
            result = il.DeclareLocal(signature.ReturnType);
        var completed = il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldsfld, handlerField);
        for (var index = 0; index < signature.ParameterTypes.Length; index++)
            il.Emit(OpCodes.Ldarg, index);
        il.Emit(OpCodes.Callvirt, delegateType.GetMethod("Invoke")!);
        if (result is not null)
            il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Leave, completed);
        if (exceptionPolicy ==
            InterceptionHandlerExceptionPolicy.ContainAndDeactivate)
        {
            il.BeginCatchBlock(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception!);
            il.Emit(OpCodes.Ldsfld, failField);
            il.Emit(OpCodes.Ldloc, exception!);
            il.Emit(
                OpCodes.Callvirt,
                typeof(Action<Exception>).GetMethod(
                    nameof(Action<>.Invoke))!);
            if (result is not null)
            {
                il.Emit(OpCodes.Ldloca, result);
                il.Emit(OpCodes.Initobj, signature.ReturnType);
            }
            il.Emit(OpCodes.Leave, completed);
        }
        il.BeginFinallyBlock();
        il.Emit(OpCodes.Ldsfld, releaseField);
        il.Emit(
            OpCodes.Callvirt,
            typeof(Action).GetMethod(nameof(Action.Invoke))!);
        il.EndExceptionBlock();
        if (result is not null)
            il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ret);

        var type = builder.CreateType()!;
        var exactHandlerField = type.GetField(handlerField.Name)!;
        var exactReleaseField = type.GetField(releaseField.Name)!;
        var exactFailField = type.GetField(failField.Name)!;
        var state = new InterceptionTrampolineState(
            () =>
            {
                exactHandlerField.SetValue(null, null);
                exactReleaseField.SetValue(null, null);
                exactFailField.SetValue(null, null);
            });
        exactHandlerField.SetValue(null, handler);
        exactReleaseField.SetValue(null, (Action)state.Release);
        exactFailField.SetValue(null, (Action<Exception>)state.Fail);
        var method = type.GetMethod(invoke.Name)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return new(
            method.MethodHandle.GetFunctionPointer(),
            handler,
            state);
    }

    private static MethodBuilder DefineExactMethod(
        TypeBuilder builder,
        string name,
        MethodAttributes attributes,
        InterceptionExactSignature signature)
    {
        var method = builder.DefineMethod(
            name,
            attributes,
            CallingConventions.Standard,
            signature.ReturnType,
            signature.ReturnRequiredModifiers,
            signature.ReturnOptionalModifiers,
            signature.ParameterTypes,
            signature.ParameterRequiredModifiers,
            signature.ParameterOptionalModifiers);
        for (var index = 0; index < signature.ParameterNames.Length; index++)
        {
            method.DefineParameter(
                index + 1,
                signature.ParameterAttributes[index],
                signature.ParameterNames[index]);
        }

        return method;
    }

}
