namespace AlvorKit.Script.AlvorEye;

/// <summary>JSON helpers shared by scenario and session command parsing.</summary>
internal static class ScenarioJson
{
    /// <summary>Serializer settings used for manifests and persistent session state.</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>Reads an optional string property.</summary>
    public static string? String(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    /// <summary>Reads an optional boolean property.</summary>
    public static bool Bool(JsonElement element, string name, bool defaultValue = false) =>
        element.TryGetProperty(name, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : defaultValue;

    /// <summary>Reads an optional integer property.</summary>
    public static int? Int(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;

    /// <summary>Reads seconds or milliseconds duration properties from JSON.</summary>
    public static TimeSpan Duration(JsonElement element, string secondsName, string millisecondsName, TimeSpan defaultValue)
    {
        if (element.TryGetProperty(millisecondsName, out var milliseconds))
            return TimeSpan.FromMilliseconds(milliseconds.GetDouble());
        return element.TryGetProperty(secondsName, out var seconds) ? TimeSpan.FromSeconds(seconds.GetDouble()) : defaultValue;
    }
}
