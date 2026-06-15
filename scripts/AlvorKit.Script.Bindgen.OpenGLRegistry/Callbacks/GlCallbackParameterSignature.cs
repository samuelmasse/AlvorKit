namespace AlvorKit.Script.Bindgen;

/// <summary>Native callback parameter declaration.</summary>
/// <param name="CType">Native parameter type text.</param>
/// <param name="Name">Native parameter name.</param>
internal sealed record GlCallbackParameterSignature(string CType, string Name);
