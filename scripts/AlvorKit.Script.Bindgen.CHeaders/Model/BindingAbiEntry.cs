namespace AlvorKit;

/// <summary>Identifies a native size or offset and its corresponding managed type and nested field path.</summary>
public record BindingAbiEntry(string Type, string Field, string NativeExpression);
