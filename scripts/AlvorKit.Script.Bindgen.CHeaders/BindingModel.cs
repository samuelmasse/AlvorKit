namespace AlvorKit.Script.Bindgen;

/// <summary>Complete generated binding model for one C-header library.</summary>
/// <param name="Enums">Enums to emit.</param>
/// <param name="Structs">Structs and unions to emit.</param>
/// <param name="Handles">Opaque handles to emit.</param>
/// <param name="Delegates">Callback delegates to emit.</param>
/// <param name="Functions">Functions to emit.</param>
/// <param name="Constants">Constants to emit.</param>
/// <param name="SkippedFunctions">Native functions skipped with reasons.</param>
/// <param name="SizeofTypes">Native types needing runtime sizeof shims.</param>
public record BindingModel(
    List<BindingEnum> Enums,
    List<BindingStruct> Structs,
    List<BindingHandle> Handles,
    List<BindingDelegate> Delegates,
    List<BindingFunction> Functions,
    List<BindingConstant> Constants,
    List<string> SkippedFunctions,
    List<string> SizeofTypes);
