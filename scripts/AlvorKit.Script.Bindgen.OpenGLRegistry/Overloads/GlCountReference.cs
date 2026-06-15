namespace AlvorKit.Script.Bindgen;

/// <summary>Pointer parameter that references a count parameter in a combined overload plan.</summary>
/// <param name="Pointer">Pointer parameter index.</param>
/// <param name="Divisor">Multiplier or divisor used to infer the count.</param>
internal readonly record struct GlCountReference(int Pointer, int Divisor);
