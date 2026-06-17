namespace AlvorKit.Mocking;

internal static partial class Patcher
{
    /// <summary>Builds Harmony prefix parameter types and names for a target method.</summary>
    private static (Type[], string?[]) BuildWrapperParams(ParameterInfo[] parameters, bool isVoid, Type returnType)
    {
        var types = new List<Type> { typeof(MethodInfo), typeof(object) };
        var names = new List<string?> { "__originalMethod", "__instance" };

        if (!isVoid)
        {
            types.Add(returnType.IsByRef ? returnType : returnType.MakeByRefType());
            names.Add("__result");
        }

        foreach (var param in parameters)
        {
            types.Add(param.ParameterType);
            names.Add(param.Name);
        }

        return ([.. types], [.. names]);
    }

    /// <summary>Normalizes a selected parameter group for generic rewire carrier construction.</summary>
    private static List<Type> NormalizeTypes(ParameterInfo[] parameters, int[] indices, int max, bool isRef = false)
    {
        var types = new List<Type>();

        foreach (var i in indices)
        {
            var type = parameters[i].ParameterType;
            types.Add(NormalizeType(isRef ? type.GetElementType()! : type));
        }

        while (types.Count < max)
            types.Add(typeof(RewireEmpty));

        return types;
    }

    /// <summary>Creates the dynamic Harmony prefix method shell for one target method.</summary>
    private static DynamicMethod CreateDynamicMethod(MethodBase method, Type[] paramTypes, string?[] paramNames)
    {
        var dyn = new DynamicMethod(
            name: $"__Method_{method.Name}_{Guid.NewGuid()}",
            returnType: typeof(bool),
            parameterTypes: paramTypes,
            m: typeof(Patcher).Module,
            skipVisibility: true);

        for (int i = 0; i < paramTypes.Length; i++)
            dyn.DefineParameter(i + 1, ParameterAttributes.None, paramNames[i]);

        return dyn;
    }

    /// <summary>Maps pointer parameters onto integer-sized values for generic rewire dispatch.</summary>
    private static Type NormalizeType(Type type)
    {
        if (type.IsPointer)
            return typeof(nint);
        else return type;
    }
}
