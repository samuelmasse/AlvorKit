namespace AlvorKit.Mocking;

internal static partial class Patcher
{
    /// <summary>Builds or returns the Harmony prefix used for one patched method.</summary>
    private static MethodInfo DynamicMethod(MethodBase method)
    {
        if (dynamicMethods.TryGetValue(method, out var cached))
            return cached;

        var returnType = ((MethodInfo)method).ReturnType;
        var isVoid = returnType == typeof(void);
        var parameters = method.GetParameters();

        var (wrapperParamTypes, wrapperParamNames) = BuildWrapperParams(parameters, isVoid, returnType);
        var (valueParamIndices, refParamIndices, refStructParamIndices, _) = Indices.ClassifyParameters(parameters);

        var valueParamTypes = NormalizeTypes(parameters, valueParamIndices, 16);
        var refParamTypes = NormalizeTypes(parameters, refParamIndices, 16, isRef: true);
        var refStructParamTypes = NormalizeTypes(parameters, refStructParamIndices, 16);

        var rewireArgsType = typeof(Rewire<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>)
            .MakeGenericType([.. valueParamTypes, .. refParamTypes, .. refStructParamTypes]);

        var dyn = CreateDynamicMethod(method, wrapperParamTypes, wrapperParamNames);
        var il = dyn.GetILGenerator();

        EmitRewireArgInit(il, rewireArgsType, valueParamIndices, refParamIndices, refStructParamIndices, isVoid);
        EmitRewireMethodCall(il, rewireArgsType, isVoid, returnType);

        dynamicMethods[method] = dyn;
        return dyn;
    }
}
