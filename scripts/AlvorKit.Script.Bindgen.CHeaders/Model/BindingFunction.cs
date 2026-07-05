namespace AlvorKit.Script.Bindgen;

/// <summary>Describes a managed function emitted from a native exported function.</summary>
/// <param name="NativeName">Native exported function name.</param>
/// <param name="ManagedName">Managed C# method name.</param>
/// <param name="ReturnType">Public managed return type.</param>
/// <param name="ReturnInteropType">Raw native interop return type.</param>
/// <param name="Parameters">Managed function parameters.</param>
/// <param name="Documentation">Parsed upstream XML or Doxygen documentation.</param>
/// <param name="ReturnsCString">Whether the native return is a const C string pointer.</param>
/// <param name="IsAdvanced">Whether normal IntelliSense should de-emphasize the raw native-shaped method.</param>
/// <param name="Platform">OS platform name whose native library builds export the function, or null when always exported.</param>
public record BindingFunction(
    string NativeName,
    string ManagedName,
    string ReturnType,
    string ReturnInteropType,
    List<BindingParameter> Parameters,
    XmlDocComment? Documentation,
    bool ReturnsCString = false,
    bool IsAdvanced = false,
    string? Platform = null);
