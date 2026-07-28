using System.Collections.Concurrent;

namespace AlvorKit.Mocking.Test.Contracts.RefSafe;

internal static class RefSafeStableDelegateCache
{
    private static readonly AssemblyBuilder Assembly =
        AssemblyBuilder.DefineDynamicAssembly(
            new("MockingRefSafeApiContract.StableDelegates"),
            AssemblyBuilderAccess.RunAndCollect);
    private static readonly ModuleBuilder Module =
        Assembly.DefineDynamicModule("StableDelegates");
    private static readonly ConcurrentDictionary<MethodInfo, Type> Types = [];
    private static readonly Lock EmitLock = new();
    private static int nextId;

    internal static Type GetOrCreate(MethodInfo capturedMethod)
    {
        if (capturedMethod.ContainsGenericParameters)
        {
            throw new ArgumentException(
                "The captured callback signature must be closed.",
                nameof(capturedMethod));
        }

        if (Types.TryGetValue(capturedMethod, out Type? stableType))
            return stableType;

        lock (EmitLock)
        {
            if (Types.TryGetValue(capturedMethod, out stableType))
                return stableType;

            stableType = Emit(capturedMethod);
            Types.TryAdd(capturedMethod, stableType);
            return stableType;
        }
    }

    internal static TDelegate CreateDirectInvoker<TDelegate>(
        Delegate normalized,
        MethodInfo capturedMethod)
        where TDelegate : Delegate
    {
        Type stableType = GetOrCreate(capturedMethod);
        if (normalized.GetType() != stableType)
        {
            throw new ArgumentException(
                "The callback is not normalized to this closed captured signature.",
                nameof(normalized));
        }

        MethodInfo directInvoke = typeof(TDelegate).GetMethod(
            nameof(Action.Invoke))!;
        RefSafeCallbackContract.ValidateInvoke(
            directInvoke,
            capturedMethod);
        ParameterInfo[] source = capturedMethod.GetParameters();
        Type[] bridgeParameters = new Type[source.Length + 1];
        bridgeParameters[0] = stableType;
        for (int index = 0; index < source.Length; index++)
            bridgeParameters[index + 1] = source[index].ParameterType;

        var bridge = new DynamicMethod(
            $"DirectInvoke_{capturedMethod.Name}_{Guid.NewGuid():N}",
            capturedMethod.ReturnType,
            bridgeParameters,
            typeof(RefSafeStableDelegateCache).Module,
            true);
        ILGenerator il = bridge.GetILGenerator();
        for (int index = 0; index < bridgeParameters.Length; index++)
            il.Emit(OpCodes.Ldarg, index);
        il.Emit(
            OpCodes.Callvirt,
            stableType.GetMethod(nameof(Action.Invoke))!);
        il.Emit(OpCodes.Ret);
        return (TDelegate)bridge.CreateDelegate(
            typeof(TDelegate),
            normalized);
    }

    private static Type Emit(MethodInfo capturedMethod)
    {
        TypeBuilder type = Module.DefineType(
            $"StableCallback_{Interlocked.Increment(ref nextId)}",
            TypeAttributes.Class |
            TypeAttributes.Sealed |
            TypeAttributes.NotPublic,
            typeof(MulticastDelegate));
        ConstructorBuilder constructor = type.DefineConstructor(
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.RTSpecialName,
            CallingConventions.Standard,
            [typeof(object), typeof(nint)]);
        constructor.SetImplementationFlags(
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed);

        ParameterInfo[] source = capturedMethod.GetParameters();
        MethodBuilder invoke = type.DefineMethod(
            nameof(Action.Invoke),
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.NewSlot |
            MethodAttributes.Virtual,
            CallingConventions.Standard);
        invoke.SetSignature(
            capturedMethod.ReturnType,
            capturedMethod.ReturnParameter.GetRequiredCustomModifiers(),
            capturedMethod.ReturnParameter.GetOptionalCustomModifiers(),
            [.. source.Select(static parameter => parameter.ParameterType)],
            [.. source.Select(static parameter => parameter.GetRequiredCustomModifiers())],
            [.. source.Select(static parameter => parameter.GetOptionalCustomModifiers())]);
        DefineParameter(
            invoke,
            0,
            capturedMethod.ReturnParameter);
        for (int index = 0; index < source.Length; index++)
            DefineParameter(invoke, index + 1, source[index]);
        invoke.SetImplementationFlags(
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed);
        return type.CreateType()!;
    }

    private static void DefineParameter(
        MethodBuilder invoke,
        int position,
        ParameterInfo source)
    {
        ParameterBuilder parameter = invoke.DefineParameter(
            position,
            source.Attributes,
            source.Name);
        foreach (CustomAttributeData attribute in source.GetCustomAttributesData())
        {
            if (attribute.AttributeType.FullName !=
                "System.Runtime.CompilerServices.ScopedRefAttribute")
            {
                continue;
            }

            parameter.SetCustomAttribute(
                new(attribute.Constructor, []));
        }
    }
}
