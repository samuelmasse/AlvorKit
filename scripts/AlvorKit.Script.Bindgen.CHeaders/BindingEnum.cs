namespace AlvorKit.Script.Bindgen;

/// <summary>Describes a managed enum emitted from a native enum or configured macro group.</summary>
/// <param name="NativeName">Native enum, typedef, or configured group name.</param>
/// <param name="ManagedName">Managed C# enum name.</param>
/// <param name="UnderlyingType">Managed integral backing type.</param>
/// <param name="IsFlags">Whether the enum should receive <see cref="FlagsAttribute"/>.</param>
/// <param name="Members">Enum members in declaration order.</param>
/// <param name="Documentation">Optional XML documentation text.</param>
public record BindingEnum(
    string NativeName,
    string ManagedName,
    string UnderlyingType,
    bool IsFlags,
    List<BindingEnumMember> Members,
    string? Documentation);
