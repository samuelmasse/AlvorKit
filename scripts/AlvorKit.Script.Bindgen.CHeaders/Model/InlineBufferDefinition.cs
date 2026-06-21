namespace AlvorKit.Script.Bindgen;

/// <summary>Describes an inline-array helper type emitted for a native fixed-size array field.</summary>
/// <param name="ManagedName">Managed C# inline buffer type name.</param>
/// <param name="NativeName">Native fixed-size array field name.</param>
/// <param name="ElementType">Managed element type name.</param>
/// <param name="Count">Number of elements in the fixed-size array.</param>
public record InlineBufferDefinition(string ManagedName, string NativeName, string ElementType, int Count);
