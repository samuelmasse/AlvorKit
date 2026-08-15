using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Captures the exact constructed CLR signature used to generate typed dispatch code.
/// </summary>
internal sealed class MockCanonicalSignature : IEquatable<MockCanonicalSignature>
{
    private const string ScopedRefAttributeName = "System.Runtime.CompilerServices.ScopedRefAttribute";
    private const string IsReadOnlyAttributeName = "System.Runtime.CompilerServices.IsReadOnlyAttribute";
    private const string RequiresLocationAttributeName = "System.Runtime.CompilerServices.RequiresLocationAttribute";
    private readonly CallingConventions callingConvention;
    private readonly MockReturnShape returnShape;
    private readonly ImmutableArray<MockParameterShape> parameters;

    /// <summary>
    /// Creates an immutable exact signature.
    /// </summary>
    internal MockCanonicalSignature(
        CallingConventions callingConvention,
        MockReturnShape returnShape,
        ImmutableArray<MockParameterShape> parameters)
    {
        this.callingConvention = callingConvention;
        this.returnShape = returnShape;
        this.parameters = parameters.IsDefault ? [] : parameters;
    }

    internal CallingConventions CallingConvention => callingConvention;
    internal MockReturnShape Return => returnShape;
    internal ImmutableArray<MockParameterShape> Parameters => parameters;

    /// <summary>
    /// Builds the exact signature from the constructed method or constructor that will execute.
    /// </summary>
    internal static MockCanonicalSignature Create(MethodBase method)
    {
        ParameterInfo[] declaredParameters = method.GetParameters();
        ImmutableArray<MockParameterShape>.Builder parameters = ImmutableArray.CreateBuilder<MockParameterShape>(declaredParameters.Length);

        for (int index = 0; index < declaredParameters.Length; index++)
            parameters.Add(CreateParameter(declaredParameters[index], index));

        return new MockCanonicalSignature(method.CallingConvention, CreateReturn(method), parameters.MoveToImmutable());
    }

    /// <inheritdoc />
    public bool Equals(MockCanonicalSignature? other)
    {
        if (other is null
            || callingConvention != other.callingConvention
            || !returnShape.Equals(other.returnShape)
            || parameters.Length != other.parameters.Length)
        {
            return false;
        }

        for (int index = 0; index < parameters.Length; index++)
        {
            if (!parameters[index].Equals(other.parameters[index]))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MockCanonicalSignature other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(callingConvention);
        hash.Add(returnShape);

        foreach (MockParameterShape parameter in parameters)
            hash.Add(parameter);

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        string parameterText = string.Join(", ", parameters.Select(FormatParameter));
        return $"{callingConvention} {returnShape.Kind}:{returnShape.Type} ({parameterText})";
    }

    private static MockParameterShape CreateParameter(ParameterInfo parameter, int declaredIndex)
    {
        Type type = parameter.ParameterType;
        return new MockParameterShape(
            declaredIndex,
            new MockTypeIdentity(type),
            GetPassingKind(type),
            parameter.IsIn,
            parameter.IsOut,
            HasAttribute(parameter.GetCustomAttributesData(), ScopedRefAttributeName),
            GetModifiers(parameter.GetRequiredCustomModifiers()),
            GetModifiers(parameter.GetOptionalCustomModifiers()));
    }

    private static MockReturnShape CreateReturn(MethodBase method)
    {
        if (method is not MethodInfo methodInfo)
            return new MockReturnShape(new MockTypeIdentity(typeof(void)), MockReturnKind.Void, [], []);

        ParameterInfo returnParameter = methodInfo.ReturnParameter;
        Type returnType = methodInfo.ReturnType;
        ImmutableArray<MockCustomModifier> requiredModifiers = GetModifiers(returnParameter.GetRequiredCustomModifiers());
        ImmutableArray<MockCustomModifier> optionalModifiers = GetModifiers(returnParameter.GetOptionalCustomModifiers());
        MockReturnKind kind = GetReturnKind(returnType, returnParameter, requiredModifiers, optionalModifiers);
        return new MockReturnShape(new MockTypeIdentity(returnType), kind, requiredModifiers, optionalModifiers);
    }

    private static MockPassingKind GetPassingKind(Type type)
    {
        if (type.IsByRef)
            return MockPassingKind.ManagedReference;
        if (type.IsPointer)
            return MockPassingKind.Pointer;
        if (type.IsFunctionPointer)
            return MockPassingKind.FunctionPointer;
        return type.IsByRefLike ? MockPassingKind.RefStructValue : MockPassingKind.Value;
    }

    private static MockReturnKind GetReturnKind(
        Type type,
        ParameterInfo returnParameter,
        ImmutableArray<MockCustomModifier> requiredModifiers,
        ImmutableArray<MockCustomModifier> optionalModifiers)
    {
        if (type == typeof(void))
            return MockReturnKind.Void;
        if (type.IsByRef)
        {
            return IsReadOnlyReturn(returnParameter, requiredModifiers, optionalModifiers)
                ? MockReturnKind.ReadOnlyManagedReference
                : MockReturnKind.ManagedReference;
        }
        if (type.IsPointer)
            return MockReturnKind.Pointer;
        if (type.IsFunctionPointer)
            return MockReturnKind.FunctionPointer;
        return type.IsByRefLike ? MockReturnKind.RefStructValue : MockReturnKind.Value;
    }

    private static bool IsReadOnlyReturn(
        ParameterInfo returnParameter,
        ImmutableArray<MockCustomModifier> requiredModifiers,
        ImmutableArray<MockCustomModifier> optionalModifiers)
    {
        return returnParameter.IsIn
            || ContainsModifier(requiredModifiers, typeof(System.Runtime.InteropServices.InAttribute).FullName!)
            || ContainsModifier(optionalModifiers, typeof(System.Runtime.InteropServices.InAttribute).FullName!)
            || ContainsModifier(requiredModifiers, IsReadOnlyAttributeName)
            || ContainsModifier(optionalModifiers, IsReadOnlyAttributeName)
            || ContainsModifier(requiredModifiers, RequiresLocationAttributeName)
            || ContainsModifier(optionalModifiers, RequiresLocationAttributeName);
    }

    private static bool ContainsModifier(ImmutableArray<MockCustomModifier> modifiers, string fullName)
    {
        foreach (MockCustomModifier modifier in modifiers)
        {
            if (modifier.Type.RuntimeType.FullName == fullName)
                return true;
        }

        return false;
    }

    private static bool HasAttribute(IList<CustomAttributeData> attributes, string fullName)
    {
        foreach (CustomAttributeData attribute in attributes)
        {
            if (attribute.AttributeType.FullName == fullName)
                return true;
        }

        return false;
    }

    private static ImmutableArray<MockCustomModifier> GetModifiers(Type[] types)
    {
        if (types.Length == 0)
            return [];

        ImmutableArray<MockCustomModifier>.Builder modifiers = ImmutableArray.CreateBuilder<MockCustomModifier>(types.Length);
        foreach (Type type in types)
            modifiers.Add(new MockCustomModifier(type));
        return modifiers.MoveToImmutable();
    }

    private static string FormatParameter(MockParameterShape parameter)
    {
        return $"{parameter.DeclaredIndex}:{parameter.Passing}:{parameter.Type}:in={parameter.IsIn}:out={parameter.IsOut}:scoped={parameter.IsScoped}";
    }
}
