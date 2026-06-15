namespace AlvorKit.Script.Bindgen;

/// <summary>Parsed callback function-pointer signature.</summary>
/// <param name="ReturnType">Native return type text.</param>
/// <param name="Parameters">Native callback parameters.</param>
internal sealed record GlCallbackSignature(
    string ReturnType,
    IReadOnlyList<GlCallbackParameterSignature> Parameters);
