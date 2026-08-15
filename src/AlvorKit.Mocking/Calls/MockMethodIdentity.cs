using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Identifies a runtime method definition and the declaring-type and method arguments used for one construction.
/// </summary>
internal sealed class MockMethodIdentity : IEquatable<MockMethodIdentity>
{
    private readonly MockTypeIdentity declaringType;
    private readonly string name;
    private readonly MemberTypes memberType;
    private readonly MockCanonicalSignature signature;
    private readonly int genericArity;
    private readonly ImmutableArray<MockTypeIdentity> declaringTypeArguments;
    private readonly ImmutableArray<MockTypeIdentity> methodArguments;

    private MockMethodIdentity(
        MockTypeIdentity declaringType,
        string name,
        MemberTypes memberType,
        MockCanonicalSignature signature,
        int genericArity,
        ImmutableArray<MockTypeIdentity> declaringTypeArguments,
        ImmutableArray<MockTypeIdentity> methodArguments)
    {
        this.declaringType = declaringType;
        this.name = name;
        this.memberType = memberType;
        this.signature = signature;
        this.genericArity = genericArity;
        this.declaringTypeArguments = declaringTypeArguments;
        this.methodArguments = methodArguments;
    }

    internal MockTypeIdentity DeclaringType => declaringType;
    internal string Name => name;
    internal MemberTypes MemberType => memberType;
    internal MockCanonicalSignature Signature => signature;
    internal int GenericArity => genericArity;
    internal ImmutableArray<MockTypeIdentity> DeclaringTypeArguments => declaringTypeArguments;
    internal ImmutableArray<MockTypeIdentity> MethodArguments => methodArguments;

    /// <summary>
    /// Builds an exact runtime identity from a method or constructor construction.
    /// </summary>
    internal static MockMethodIdentity Create(MethodBase method)
    {
        Type declaringType = method.DeclaringType
            ?? throw new ArgumentException("A cacheable method must have a runtime declaring type.", nameof(method));
        Type[] methodArguments =
            method is MethodInfo { IsGenericMethod: true } methodInfo
                ? methodInfo.GetGenericArguments()
                : [];

        return new MockMethodIdentity(
            new MockTypeIdentity(declaringType),
            method.Name,
            method.MemberType,
            MockCanonicalSignature.Create(method),
            methodArguments.Length,
            GetTypeArguments(declaringType),
            GetTypeArguments(methodArguments));
    }

    /// <inheritdoc />
    public bool Equals(MockMethodIdentity? other)
    {
        return other is not null
            && declaringType == other.declaringType
            && StringComparer.Ordinal.Equals(name, other.name)
            && memberType == other.memberType
            && signature.Equals(other.signature)
            && genericArity == other.genericArity
            && declaringTypeArguments.AsSpan().SequenceEqual(other.declaringTypeArguments.AsSpan())
            && methodArguments.AsSpan().SequenceEqual(other.methodArguments.AsSpan());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MockMethodIdentity other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(declaringType);
        hash.Add(name, StringComparer.Ordinal);
        hash.Add(memberType);
        hash.Add(signature);
        hash.Add(genericArity);

        foreach (MockTypeIdentity argument in declaringTypeArguments)
            hash.Add(argument);
        foreach (MockTypeIdentity argument in methodArguments)
            hash.Add(argument);

        return hash.ToHashCode();
    }

    private static ImmutableArray<MockTypeIdentity> GetTypeArguments(Type type)
    {
        return type.IsGenericType ? GetTypeArguments(type.GetGenericArguments()) : [];
    }

    private static ImmutableArray<MockTypeIdentity> GetTypeArguments(Type[] arguments)
    {
        if (arguments.Length == 0)
            return [];

        ImmutableArray<MockTypeIdentity>.Builder result = ImmutableArray.CreateBuilder<MockTypeIdentity>(arguments.Length);
        foreach (Type argument in arguments)
            result.Add(new MockTypeIdentity(argument));
        return result.MoveToImmutable();
    }
}
