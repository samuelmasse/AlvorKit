namespace AlvorKit.Script.Bindgen;

/// <summary>Generated enum group with its registry source name and members.</summary>
/// <param name="NativeName">OpenGL registry group name.</param>
/// <param name="ManagedName">Generated C# enum type name.</param>
/// <param name="IsFlags">Whether any member came from a bitmask block.</param>
/// <param name="Members">Members emitted into this enum.</param>
public sealed record GlEnumGroup(
    string NativeName,
    string ManagedName,
    bool IsFlags,
    IReadOnlyList<GlEnumMember> Members)
{
    /// <summary>Managed enum backing type used for all members in the group.</summary>
    public string UnderlyingType { get; init; } = "uint";
}
