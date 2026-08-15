using System.Security.Cryptography;

namespace AlvorKit;

/// <summary>Provides a stable SHA-256 identity for exact authoritative method-body bytes.</summary>
public sealed class LoadedMethodBodyIdentity :
    IEquatable<LoadedMethodBodyIdentity>
{
    /// <summary>The fixed-width immutable digest text.</summary>
    private readonly string value;

    /// <summary>Creates an identity from validated digest text.</summary>
    private LoadedMethodBodyIdentity(string value) => this.value = value;

    /// <summary>Gets the uppercase, fixed-width SHA-256 hexadecimal identity.</summary>
    public string Value => value;

    /// <summary>Computes an identity from the complete loaded method body and extra sections.</summary>
    internal static LoadedMethodBodyIdentity Compute(
        ReadOnlySpan<byte> body) =>
        new(Convert.ToHexString(SHA256.HashData(body)));

    /// <summary>Tests identity equality by exact ordinal digest text.</summary>
    public bool Equals(LoadedMethodBodyIdentity? other) =>
        other is not null &&
        string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>Tests identity equality against an arbitrary object.</summary>
    public override bool Equals(object? obj) =>
        obj is LoadedMethodBodyIdentity other && Equals(other);

    /// <summary>Gets the digest's stable ordinal hash code for in-process collections.</summary>
    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(value);

    /// <summary>Returns the fixed-width SHA-256 hexadecimal identity.</summary>
    public override string ToString() => value;
}
