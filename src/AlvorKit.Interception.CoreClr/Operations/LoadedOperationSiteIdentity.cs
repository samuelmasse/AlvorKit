using System.Collections.Immutable;
using System.Security.Cryptography;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Creates stable site identities from exact loaded-baseline coordinates.</summary>
internal static class LoadedOperationSiteIdentity
{
    /// <summary>Creates a versioned SHA-256 identity without rewritten offsets.</summary>
    internal static string Create(
        Guid moduleVersionId,
        int containingMethodToken,
        string constructedContext,
        LoadedMethodBodyIdentity bodyIdentity,
        LoadedIlInstruction instruction,
        LoadedRecognizedOperation operation)
    {
        var canonical = new StringBuilder();
        _ = canonical
            .Append("li1|")
            .Append(moduleVersionId.ToString("N"))
            .Append('|')
            .Append(Hex(containingMethodToken))
            .Append('|')
            .Append(bodyIdentity.Value)
            .Append('|')
            .Append(Hex(instruction.BaselineOffset))
            .Append('|')
            .Append(instruction.OpCodeValue.ToString("X4"))
            .Append('|')
            .Append(Invariant((int)operation.Kind))
            .Append('|')
            .Append(Hex(operation.MetadataToken));
        Append(canonical, constructedContext);
        Append(canonical, operation.CanonicalSignature);
        Append(canonical, operation.Prefixes);
        var digest = SHA256.HashData(
            Encoding.UTF8.GetBytes(canonical.ToString()));
        return $"li1-{Convert.ToHexString(digest)}";
    }

    /// <summary>Appends one length-delimited identity component.</summary>
    private static void Append(StringBuilder target, string value) =>
        _ = target
            .Append('|')
            .Append(Invariant(value.Length))
            .Append(':')
            .Append(value);

    /// <summary>Appends accepted prefixes in immutable baseline order.</summary>
    private static void Append(
        StringBuilder target,
        ImmutableArray<LoadedOperationPrefixDescriptor> prefixes)
    {
        _ = target.Append('|').Append(Invariant(prefixes.Length));
        foreach (var prefix in prefixes)
        {
            _ = target
                .Append(':')
                .Append(Invariant((int)prefix.Kind))
                .Append(':')
                .Append(Invariant(prefix.BaselineOffset))
                .Append(':')
                .Append(Hex(prefix.MetadataToken));
            Append(target, prefix.OperandSignature);
        }
    }

    /// <summary>Formats one signed value with culture-independent decimal digits.</summary>
    private static string Invariant(int value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Formats one token or coordinate as fixed-width unsigned hexadecimal.</summary>
    private static string Hex(int value) =>
        unchecked((uint)value).ToString(
            "X8",
            System.Globalization.CultureInfo.InvariantCulture);
}
