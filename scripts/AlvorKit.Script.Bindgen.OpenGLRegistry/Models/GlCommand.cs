namespace AlvorKit.Script.Bindgen;

/// <summary>OpenGL command selected from the registry and mapped into managed and interop shapes.</summary>
/// <param name="NativeName">Native OpenGL command name.</param>
/// <param name="ManagedName">Generated C# method name.</param>
/// <param name="ReturnType">Public managed return type.</param>
/// <param name="ReturnInteropType">Raw function-pointer return type.</param>
/// <param name="Parameters">Mapped command parameters.</param>
/// <param name="Availability">Registry availability for generated documentation.</param>
/// <param name="Documentation">Optional imported XML documentation.</param>
/// <param name="ReturnsCString">Whether the raw return value is a const GLchar pointer.</param>
public sealed record GlCommand(
    string NativeName,
    string ManagedName,
    string ReturnType,
    string ReturnInteropType,
    IReadOnlyList<GlParameter> Parameters,
    GlAvailability Availability,
    XmlDocComment? Documentation,
    bool ReturnsCString);
