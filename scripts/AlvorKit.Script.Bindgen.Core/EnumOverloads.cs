namespace AlvorKit.Script.Bindgen;

/// <summary>Type hints for overloads that turn raw integer parameters into enums.</summary>
public class EnumOverloads
{
    /// <summary>Parameter names that always map to a given enum, across all functions.</summary>
    public Dictionary<string, string> ByParamName { get; set; } = [];

    /// <summary>Per-function overrides, including return values that have no parameter name.</summary>
    public Dictionary<string, FunctionEnums> Functions { get; set; } = [];
}
