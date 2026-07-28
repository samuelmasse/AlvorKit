namespace AlvorKit.Interception;

/// <summary>Stable runtime identity for one exact method definition.</summary>
public readonly struct InterceptionTarget : IEquatable<InterceptionTarget>
{
    private const ulong FnvOffsetBasis = 14695981039346656037;
    private const ulong FnvPrime = 1099511628211;

    private InterceptionTarget(
        Guid moduleMvid,
        int methodToken,
        ulong signatureHash,
        string displayName)
    {
        ModuleMvid = moduleMvid;
        MethodToken = methodToken;
        SignatureHash = signatureHash;
        DisplayName = displayName;
    }

    /// <summary>Gets the defining module's version ID.</summary>
    public Guid ModuleMvid { get; }

    /// <summary>Gets the exact MethodDef token.</summary>
    public int MethodToken { get; }

    /// <summary>Gets the hash of the raw metadata signature.</summary>
    public ulong SignatureHash { get; }

    /// <summary>Gets a non-identity diagnostic name.</summary>
    public string DisplayName { get; }

    /// <summary>Creates an identity from a loaded non-dynamic method definition.</summary>
    public static InterceptionTarget FromMethod(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);
        return FromMethodBase(method);
    }

    /// <summary>Creates an identity from a loaded non-dynamic constructor definition.</summary>
    public static InterceptionTarget FromConstructor(
        ConstructorInfo constructor)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        if (constructor.DeclaringType?.IsGenericType == true)
        {
            throw new NotSupportedException(
                "Constructors on generic declaring types require " +
                "construction-specific runtime routing.");
        }
        return FromMethodBase(constructor);
    }

    private static InterceptionTarget FromMethodBase(MethodBase method)
    {
        Validate(method);
        byte[] signature;
        try
        {
            signature = method.Module.ResolveSignature(method.MetadataToken);
        }
        catch (Exception exception) when (
            exception is ArgumentException or
                BadImageFormatException or
                NotSupportedException)
        {
            throw new NotSupportedException(
                $"The metadata signature for '{method}' could not be resolved.",
                exception);
        }

        return new(
            method.Module.ModuleVersionId,
            method.MetadataToken,
            Hash(signature),
            $"{method.DeclaringType?.FullName}.{method.Name}");
    }

    internal static InterceptionTarget FromIdentity(
        Guid moduleMvid,
        int methodToken,
        ulong signatureHash,
        string displayName)
    {
        if (moduleMvid == Guid.Empty)
            throw new ArgumentException("A module MVID is required.", nameof(moduleMvid));
        if (methodToken == 0 ||
            (methodToken & unchecked((int)0xFF000000)) != 0x06000000)
        {
            throw new ArgumentException(
                "The target must have a MethodDef metadata token.",
                nameof(methodToken));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new(
            moduleMvid,
            methodToken,
            signatureHash,
            displayName);
    }

    internal bool IsValid =>
        ModuleMvid != Guid.Empty &&
        MethodToken != 0 &&
        (MethodToken & unchecked((int)0xFF000000)) == 0x06000000 &&
        !string.IsNullOrWhiteSpace(DisplayName);

    private static void Validate(MethodBase method)
    {
        if (method.Module.Assembly.IsDynamic)
            throw new NotSupportedException("Dynamic modules do not have a stable MVID and method definition token.");
        if (method.MetadataToken == 0 ||
            (method.MetadataToken & unchecked((int)0xFF000000)) != 0x06000000)
        {
            throw new NotSupportedException("The target must have a MethodDef metadata token.");
        }
        if (method.ContainsGenericParameters ||
            method.DeclaringType?.ContainsGenericParameters == true)
        {
            throw new NotSupportedException("Open generic targets cannot be intercepted.");
        }
        if (method is MethodInfo { IsAbstract: true })
            throw new NotSupportedException("Abstract methods have no body to intercept.");
        if ((method.MethodImplementationFlags &
            (MethodImplAttributes.InternalCall | MethodImplAttributes.Native |
             MethodImplAttributes.Runtime | MethodImplAttributes.Unmanaged)) != 0)
        {
            throw new NotSupportedException("Runtime-implemented and native methods cannot be intercepted.");
        }
        if (method.GetMethodBody() is null)
            throw new NotSupportedException("The target method has no managed IL body.");
    }

    private static ulong Hash(ReadOnlySpan<byte> bytes)
    {
        var value = FnvOffsetBasis;
        foreach (var item in bytes)
        {
            value ^= item;
            value *= FnvPrime;
        }

        return value;
    }

    /// <inheritdoc />
    public bool Equals(InterceptionTarget other) =>
        ModuleMvid == other.ModuleMvid &&
        MethodToken == other.MethodToken &&
        SignatureHash == other.SignatureHash;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is InterceptionTarget other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(ModuleMvid, MethodToken, SignatureHash);

    /// <summary>Tests exact runtime identity.</summary>
    public static bool operator ==(
        InterceptionTarget left,
        InterceptionTarget right) =>
        left.Equals(right);

    /// <summary>Tests exact runtime identity.</summary>
    public static bool operator !=(
        InterceptionTarget left,
        InterceptionTarget right) =>
        !left.Equals(right);
}
