using System.Collections.Immutable;
using System.Security.Cryptography;

namespace AlvorKit;

/// <summary>Creates a deterministic identity for one baseline-plus-sites composition.</summary>
internal static class LoadedSymbolicGenerationIdentity
{
    /// <summary>Hashes exact baseline, caller context, and ordered site identities.</summary>
    internal static string Create(
        LoadedMethodBodySnapshot body,
        Guid moduleVersionId,
        int containingMethodToken,
        string constructedContext,
        ImmutableArray<LoadedOperationSiteDescriptor> sites)
    {
        var canonical = new System.Text.StringBuilder()
            .Append("sg1|")
            .Append(moduleVersionId.ToString("N"))
            .Append('|')
            .Append(Hex(containingMethodToken))
            .Append('|')
            .Append(body.Identity.Value)
            .Append('|')
            .Append(Invariant(constructedContext.Length))
            .Append(':')
            .Append(constructedContext)
            .Append('|')
            .Append(Invariant(sites.Length));
        foreach (var site in sites)
        {
            _ = canonical
                .Append('|')
                .Append(Invariant(site.StableId.Length))
                .Append(':')
                .Append(site.StableId);
        }
        var digest = SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(canonical.ToString()));
        return $"sg1-{Convert.ToHexString(digest)}";
    }

    /// <summary>Formats one count with invariant decimal digits.</summary>
    private static string Invariant(int value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Formats one metadata token as fixed-width unsigned hexadecimal.</summary>
    private static string Hex(int value) =>
        unchecked((uint)value).ToString(
            "X8",
            System.Globalization.CultureInfo.InvariantCulture);
}
